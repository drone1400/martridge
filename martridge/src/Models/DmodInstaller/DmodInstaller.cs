using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.Trace;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Compressors.BZip2MT.InputStream;
using SharpCompress.Readers;
namespace Martridge.Models.DmodInstaller {

    public class DmodInstaller {
        public event EventHandler<DmodInstallerProgressEventArgs>? ProgressReport;
        public event EventHandler? DmodInstallerActivityStarted;
        public event EventHandler? DmodInstallerActivityEnded;

        public DmodInstallPhase InstallPhase => this._installPhase;

        // data that gets set during initialization
        public FileInfo? SourceFile => this._sourceFile;
        public string DmodSourceNameNoExt => this._sourceFile?.Name.Substring(0, this._sourceFile.Name.Length - this._sourceFile.Extension.Length) ?? "";
        public string? DmodRootName => this._dmodRootDirName;

        // data that gets set during installation...
        public DirectoryInfo? InstallationFinalDestination => this._installationFinalDestination;
        public DinkInstallerResult InstallResult => this._installResult;
        public Exception? InstallException => this._installException;


        // temporary file/directory helper
        private readonly DinkTempFileHelper _temp = new DinkTempFileHelper();

        // temporary directory to extract non standard dmod archives to
        private DirectoryInfo? _tempArchiveDir = null;

        private DmodInstallPhase _installPhase = DmodInstallPhase.Inactive;

        private FileInfo? _sourceFile = null;

        // entries found in the archive
        private string? _dmodRootDirName = null;

        private DirectoryInfo? _installationFinalDestination = null;

        private DinkInstallerResult _installResult = DinkInstallerResult.Error;
        private Exception? _installException = null;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public MyTrace CustomTrace { get; }

        private readonly object _syncRoot = new object();

        private void ReportProgressPrimary() {
            try {

                // NOTE: the primary progress is the phase itself
                // there are a total of 5 phases to go through...
                this.ProgressReport?.Invoke(this, new DmodInstallerProgressEventArgs(this._installPhase,  this._installResult, (int)this._installPhase / 5.0));
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        private void ReportActivityStart() {
            try {
                this.DmodInstallerActivityStarted?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        private void ReportActivityEnd() {
            try {
                this.DmodInstallerActivityEnded?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        private void LogMessage(string line1) {
            this.CustomTrace.WriteMessage(line1);
        }
        private void LogMessage(string line1, string line2) {
            this.CustomTrace.WriteMessage([line1, line2]);
        }
        private void LogWarning(string line1) {
            this.CustomTrace.WriteMessage(line1, MyTraceLevel.Warning);
        }
        
        private void LogWarning(string line1, string line2) {
            this.CustomTrace.WriteMessage([line1, line2], MyTraceLevel.Warning);
        }
        private void LogError(string line1) {
            this.CustomTrace.WriteMessage(line1, MyTraceLevel.Error);
        }
        private void LogError(string line1, string line2) {
            this.CustomTrace.WriteMessage([line1, line2], MyTraceLevel.Error);
        }
        private void LogError(Exception ex) {
            this.CustomTrace.WriteException(ex, MyTraceLevel.Error);
        }

        public DmodInstaller() {
            this.CustomTrace = new MyTrace(this.GetType().ToString()) {
                MirrorToGlobalTrace = true,
            };

            this._temp.SetLogCallback(this.LogMessage);
        }
        
        /// <summary>
        /// Cleans up temporary files and flushes the log
        /// </summary>
        private void CleanUp() {
            lock (this._syncRoot) {
                this._installPhase = DmodInstallPhase.Cleanup;
            }
            this.ReportProgressPrimary();

            this.ReportActivityStart();
            this.LogMessage(Localizer.Instance["DmodInstaller/Log/CleaningUpStart"]);
            this._temp.Dispose();
            this.ReportActivityEnd();
            this.LogMessage(Localizer.Instance["DmodInstaller/Log/CleaningUpEnd"]);
            
            // flush log
            this.CustomTrace.Flush();
            this.CustomTrace.Close();
        }

        /// <summary>
        /// Cancels current DMOD installer action
        /// </summary>
        public void Cancel() {
            this._cancellationTokenSource.Cancel();

            lock (this._syncRoot) {
                if (this._installPhase != DmodInstallPhase.Inactive &&
                    this._installPhase != DmodInstallPhase.AwaitingUserInput) return;

                // jump to cleanup state to prevent starting initialization or installation...
                this._installPhase = DmodInstallPhase.Cleanup;
            }

            this.LogMessage(Localizer.Instance["DmodInstaller/Log/CancelledByUser"]);

            // clean up
            this.CleanUp();
            // finish things up
            lock (this._syncRoot) {
                this._installPhase = DmodInstallPhase.Finished;
                this._installResult = DinkInstallerResult.Cancelled;
            }
            this.ReportProgressPrimary();
        }

        /// <summary>
        /// Initializes and decompresses DMOD file to temporary location
        /// </summary>
        /// <param name="sourceFile">Source DMOD file</param>
        /// <exception cref="DinkInstallerFileSystemException">if file can not be read</exception>
        /// <exception cref="DinkInstallerUnzipException">if file can not be decompressed</exception>
        public void InitializeAndDecompress(FileInfo sourceFile) {
            lock (this._syncRoot) {
                // some kind of sanity check i suppose?...
                if (this._installPhase != DmodInstallPhase.Inactive)
                    return;

                this._installPhase = DmodInstallPhase.Decompressing;
                this._sourceFile = sourceFile;
                this._tempArchiveDir = null;
                this._dmodRootDirName = null;
            }

            try {
                this.ReportProgressPrimary();

                this._sourceFile.Refresh();
                if (this._sourceFile.Exists == false) {
                    this.LogMessage(Localizer.Instance["DmodInstaller/Log/DmodFileNotFound"], $"    \"{this._sourceFile.FullName}\"");
                    throw new DinkInstallerFileSystemException(this._sourceFile.FullName);
                }

                // begin initializing DMOD file...
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/Decompressing/StartInitializing"], $"    \"{this._sourceFile.FullName}\"");

                this.ReportActivityStart();
                // try to initialize the DMOD top entries info from the standard bzip2/tar dmod format
                // if that fails, maybe this is a different archive type like 7z, so try that instead?
                bool result = this.TryDecompressFromStandardDmod();
                if (result == false) {
                    result = this.TryInitializeEntriesFromUnknownDmod();
                }
                this.ReportActivityEnd();

                if (result == false) {
                    string message = Localizer.Instance["DmodInstaller/Log/Decompressing/Failure"];
                    this.LogMessage(message);
                    throw new DinkInstallerUnzipException(message);
                }

                // finished initializing DMOD file
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/Decompressing/Success"]);

                // move on to next phase
                lock (this._syncRoot) {
                    this._installPhase = DmodInstallPhase.AwaitingUserInput;
                }
                this.ReportProgressPrimary();
            } catch (DinkInstallerCancelledByUserException) {
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/CancelledByUser"]);
                // clean up
                this.CleanUp();
                // finish things up
                lock (this._syncRoot) {
                    this._installPhase = DmodInstallPhase.Finished;
                    this._installResult = DinkInstallerResult.Cancelled;
                }
                this.ReportProgressPrimary();
            }  catch (Exception ex) {
                // log exception
                this._installException = ex;
                this.LogError(this._installException);
                MyTrace.Global.WriteException(this._installException);
                // clean up
                this.CleanUp();
                // finish things up
                lock (this._syncRoot) {
                    this._installPhase = DmodInstallPhase.Finished;
                    this._installResult = DinkInstallerResult.Error;
                }
                this.ReportProgressPrimary();
            }
        }

        
        /// <summary>
        /// Helper class used during extraction so I don't have to write two functions for
        /// Extracting from an <see cref="IReader"/>'s current <see cref="IEntry"/> or an <see cref="IArchiveEntry"/>
        /// </summary>
        private class EntryWrapper {
            public string Key => this._entry?.Key ?? this._reader?.Entry.Key ?? string.Empty;
            public void WriteToDirectory(DirectoryInfo destination, ExtractionOptions options) {
                if (this._entry != null) {
                    this._entry.WriteToDirectory(destination.FullName, options);
                    return;
                }
                if (this._reader != null) {
                    this._reader.WriteEntryToDirectory(destination.FullName, options);
                }
            }

            private IArchiveEntry? _entry = null;
            private IReader? _reader = null;
            public EntryWrapper(IArchiveEntry entry) {
                this._entry = entry;
            }

            public EntryWrapper(IReader reader) {
                this._reader = reader;
            }
        }

        /// <summary>
        /// Extracts a DMOD file entry to the destination root directory
        /// </summary>
        /// <param name="entry">Current DMOD archive entry</param>
        /// <param name="destination">Destination root directory</param>
        /// <param name="tempTopLevelEntries">Dictionary of top level directories within</param>
        /// <param name="options">Entry extraction options</param>
        /// <exception cref="DinkInstallerCancelledByUserException">if operation is cancelled by the user</exception>
        private void ExtractDmodArchiveEntry(EntryWrapper entry, DirectoryInfo destination, Dictionary<string, bool> tempTopLevelEntries, ExtractionOptions options) {
            // skip empty entries...
            if (string.IsNullOrEmpty(entry.Key))
                return;

            // check if cancelled
            if (this._cancellationTokenSource.IsCancellationRequested) {
                throw new DinkInstallerCancelledByUserException();
            }

            bool isValidEntry = true;

            string[] split = entry.Key.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

            // check for and disallow going back in relative path
            foreach (string s in split) {
                if (s == "..") {
                    isValidEntry = false;
                    break;
                }
            }

            if (split.Length > 1) {
                if (split[0] != "..") {
                    tempTopLevelEntries[split[0]] = true;
                }
            }
            else {
                if (entry.Key != "..") {
                    tempTopLevelEntries[entry.Key] = true;
                }
            }

            if (isValidEntry) {
                entry.WriteToDirectory(destination, options);
            }
            else {
                this.LogWarning(Localizer.Instance["DmodInstaller/Log/Decompressing/SkippingInvalidEntry"], "    " + entry.Key);
            }
        }


        /// <summary>
        /// Tries to decompress a standard .tar.bz2 type .dmod file
        /// </summary>
        /// <returns>True if successfull</returns>
        /// <exception cref="DinkInstallerCancelledByUserException">if operation is cancelled by the user</exception>
        private bool TryDecompressFromStandardDmod() {
            if (this._sourceFile == null)
                return false;

            try {
                FileInfo? tempTarArchive = null;

                // initialize output archive
                if (Config.Instance.General.DecompressDmodsToMemoryStreamInsteadOfTemporaryFile == false) {
                    tempTarArchive = this._temp.TryCreateTempFile();
                }
                using Stream tempTarStream = tempTarArchive != null
                    ? new FileStream(tempTarArchive.FullName, FileMode.Create, FileAccess.ReadWrite)
                    : new MemoryStream((int)this._sourceFile.Length);

                // the DMOD should be a bzip2-ed tar archive...
                using FileStream fs = new FileStream(this._sourceFile.FullName, FileMode.Open, FileAccess.Read);

                // try decompressing the stream
                try {
                    using BZip2ParallelInputStream decompressor = new BZip2ParallelInputStream(fs, false);

                    // initialize temporary archive directory after BZip2 decompressor
                    // this way the temp dir only gets created if the file is a valid BZip2 stream (or at least has a valid header)
                    this._tempArchiveDir = this._temp.TryCreateTempDirectory();
                    if (this._tempArchiveDir == null) {
                        this.LogError(Localizer.Instance["DmodInstaller/Log/Decompressing/CouldNotCreateTempDir"]);
                        return false;
                    }

                    this.LogMessage(Localizer.Instance["DmodInstaller/Log/Decompressing/StartExtraction"]);
                    decompressor.CopyTo(tempTarStream);
                } catch (IOException) {
                    this._tempArchiveDir = null;
                    return false;
                }

                // check if cancelled
                if (this._cancellationTokenSource.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

                // now we have an extracted temporary tar file... time to read all its entries!
                tempTarStream.Seek(0, SeekOrigin.Begin);

                Dictionary<string, bool> tempTopLevelEntries = new Dictionary<string, bool>();

                using IArchive archive = ArchiveFactory.Open(tempTarStream, new ReaderOptions() {
                    ExtensionHint = ".tar"
                });

                ExtractionOptions options = new ExtractionOptions() {
                    ExtractFullPath = true,
                    Overwrite = true,
                    PreserveFileTime = true,
                    PreserveAttributes = false, // NOTE: tar archive does not have file attributes, setting this to true throws exception
                };

                foreach (IArchiveEntry entry in archive.Entries) {
                    this.ExtractDmodArchiveEntry(new EntryWrapper(entry), this._tempArchiveDir, tempTopLevelEntries, options);
                }

                // determine output root directory name, either use the single top level entry or the source file name
                // user can change this later...
                if (tempTopLevelEntries.Count > 0) {
                    this._dmodRootDirName = tempTopLevelEntries.Count > 1
                        ? Path.GetFileNameWithoutExtension(this._sourceFile.Name)
                        : tempTopLevelEntries.First().Key;
                }

            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                return false;
            }

            return string.IsNullOrWhiteSpace(this._dmodRootDirName) == false;
        }

        /// <summary>
        /// Tries to decompress a generic .rar, .zip, .7z or other possibly compatible archive types,
        /// except for standard bzip2 compressed .dmod files
        /// </summary>
        /// <returns>True if successfull</returns>
        /// <exception cref="DinkInstallerCancelledByUserException">if operation is cancelled by the user</exception>
        private bool TryInitializeEntriesFromUnknownDmod() {
            if (this._sourceFile == null)
                return false;

            this._tempArchiveDir = this._temp.TryCreateTempDirectory();
            if (this._tempArchiveDir == null) {
                this.LogError(Localizer.Instance["DmodInstaller/Log/Decompressing/CouldNotCreateTempDir"]);
                return false;
            }

            try {
                // the DMOD is some unknown kind of archive
                using FileStream fs = new FileStream(this._sourceFile.FullName, FileMode.Open, FileAccess.Read);

                Dictionary<string, bool> tempTopLevelEntries = new Dictionary<string, bool>();

                using IArchive archive = ArchiveFactory.Open(fs, new ReaderOptions() {
                    ExtensionHint = this._sourceFile.Extension
                });

                // check if cancelled
                if (this._cancellationTokenSource.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

                ExtractionOptions options = new ExtractionOptions() {
                    ExtractFullPath = true,
                    Overwrite = true,
                    PreserveFileTime = true,
                    PreserveAttributes = archive.Type != ArchiveType.Tar, // NOTE: tar archive does not have file attributes
                };

                this.LogMessage(Localizer.Instance["DmodInstaller/Log/Decompressing/StartExtraction"]);
                if (archive.Type == ArchiveType.SevenZip || archive.Type == ArchiveType.Rar && archive.IsSolid) {
                    // use ExtractAllEntries for 7zip and solid RAR archives
                    using IReader reader = archive.ExtractAllEntries();
                    EntryWrapper entryWrapper = new EntryWrapper(reader);

                    while (reader.MoveToNextEntry()) {
                        this.ExtractDmodArchiveEntry(entryWrapper, this._tempArchiveDir, tempTopLevelEntries, options);
                    }
                }
                else {
                    foreach (IArchiveEntry entry in archive.Entries) {
                        this.ExtractDmodArchiveEntry(new EntryWrapper(entry), this._tempArchiveDir, tempTopLevelEntries, options);
                    }
                }

                // determine output root directory name, either use the single top level entry or the source file name
                // user can change this later...
                if (tempTopLevelEntries.Count > 0) {
                    this._dmodRootDirName = tempTopLevelEntries.Count > 1
                        ? Path.GetFileNameWithoutExtension(this._sourceFile.Name)
                        : tempTopLevelEntries.First().Key;
                }

            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                return false;
            }

            return string.IsNullOrWhiteSpace(this._dmodRootDirName) == false;
        }
        
        /// <summary>
        /// Starts moving DMOD files from the temporary directory they were decompressed in,
        /// to the final destination
        /// </summary>
        /// <param name="destinationDirectory">destination root directory</param>
        /// <param name="destinationOverride">destination subdirectory name, if user wishes to override</param>
        /// <param name="allowOverwrite">if true, allows overwriting files in the destination</param>
        /// <exception cref="NullReferenceException">if the temporary DMOD decompression directory does not exist</exception>
        /// <exception cref="DinkInstallerCancelledByUserException">if operation is cancelled by the user</exception>
        /// <exception cref="DinkInstallerFileSystemException">if destination is drive root, or destination is not rooted at all</exception>
        public void StartMovingDmodFiles(DirectoryInfo destinationDirectory, string destinationOverride, bool allowOverwrite) {
            lock (this._syncRoot) {
                if (this._installPhase != DmodInstallPhase.AwaitingUserInput) return;
                if (this._tempArchiveDir == null)
                    throw new NullReferenceException("Temporary archive directory was not previously initialized!");

                this._installPhase = DmodInstallPhase.CopyingFiles;
            }

            bool cancelled = false;

            try {
                this.ReportProgressPrimary();

                if (this._cancellationTokenSource.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

                // sanity checks for destination
                if (destinationDirectory.Parent == null) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DmodInstaller/Log/MovingToDestination/ErrorIsRoot"] + $" \"{destinationDirectory.FullName}\"");
                }
                if (Path.IsPathRooted(destinationDirectory.FullName) == false) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DmodInstaller/Log/MovingToDestination/ErrorIsNotRooted"] + $" \"{destinationDirectory.FullName}\"");
                }

                // extracting DMOD
                this.ReportActivityStart();
                this.ExecuteMoveDmodFiles(destinationDirectory, destinationOverride, allowOverwrite);
                this.ReportActivityEnd();
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/InstallingDone"]);
            } catch (DinkInstallerCancelledByUserException) {
                cancelled = true;
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/CancelledByUser"]);
            } catch (Exception ex) {
                this._installException = ex;
                this.LogError(this._installException);
                MyTrace.Global.WriteException(this._installException);
            }
            finally {
                this.ReportActivityEnd();
                // clean up
                this.CleanUp();
                // finish things up
                lock (this._syncRoot) {
                    this._installPhase = DmodInstallPhase.Finished;
                    if (cancelled == false && this._installException == null) this._installResult = DinkInstallerResult.Success;
                    else if (cancelled) this._installResult = DinkInstallerResult.Cancelled;
                    else this._installResult = DinkInstallerResult.Error;
                }
                this.ReportProgressPrimary();
            }
        }
        
        private void ExecuteMoveDmodFiles(DirectoryInfo destinationDirectory, string destinationOverride, bool allowOverwrite) {
            if (this._cancellationTokenSource.IsCancellationRequested) {
                throw new DinkInstallerCancelledByUserException();
            }

            string dmodDirectory = this._dmodRootDirName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(destinationOverride)) {
                dmodDirectory = destinationOverride;
            }

            if (string.IsNullOrWhiteSpace(dmodDirectory)) {
                throw new DinkInstallerFileSystemException(Localizer.Instance["DmodInstaller/Log/MovingToDestination/ErrorUnknownDirectory"]);
            }

            if (this._tempArchiveDir == null) {
                throw new DinkInstallerFileSystemException(Localizer.Instance["DmodInstaller/Log/MovingToDestination/ErrorTempDirNotFound"]);
            }

            this._installationFinalDestination = new DirectoryInfo(Path.Combine(destinationDirectory.FullName, dmodDirectory));

            DirectoryInfo tempRootDir = this._tempArchiveDir;

            // navigate down into the directory structure until we encounter either some files or more than just one directory
            DirectoryInfo[] subDirs = this._tempArchiveDir.GetDirectories();
            FileInfo[] files = this._tempArchiveDir.GetFiles();
            while (subDirs.Length == 1 && files.Length == 0) {
                tempRootDir = subDirs[0];
                subDirs = tempRootDir.GetDirectories();
                files = tempRootDir.GetFiles();
            }

            // get all files!
            files = tempRootDir.GetFiles("*", SearchOption.AllDirectories);

            this.LogMessage(Localizer.Instance["DmodInstaller/Log/MovingToDestination/Location"], $"    \"{this._installationFinalDestination.FullName}\"");
            this.LogMessage(Localizer.Instance["DmodInstaller/Log/MovingToDestination/FileCount"], $"    {files.Length}");

            foreach (FileInfo file in files) {
                try {
                    string relativePath = Path.GetRelativePath(tempRootDir.FullName, file.FullName);
                    string destinationFile = Path.Combine(this._installationFinalDestination.FullName, relativePath);
                    FileInfo destinationFileInfo = new FileInfo(destinationFile);
                    if (destinationFileInfo.Directory?.Exists != true) {
                        destinationFileInfo.Directory?.Create();
                    }
                    File.Move(file.FullName, destinationFile, allowOverwrite);
                } catch (Exception ex) {
                    this.LogError(Localizer.Instance["DmodInstaller/Log/MovingToDestination/ErrorMovingFile"], $"    \"{file.FullName}\"");
                    MyTrace.Global.WriteException(ex);
                }
            }
        }
    }
}
