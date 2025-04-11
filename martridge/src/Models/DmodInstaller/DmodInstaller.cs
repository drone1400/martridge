using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Martridge.Models.Localization;
using Martridge.Trace;
using SharpCompress.Common;
using SharpCompress.Compressors.PBZip2;
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
        public ReadOnlyCollection<string> ArchiveEntries { get; }
        public ReadOnlyCollection<string> ArchiveTopLevelEntries { get; }
        public string? DmodRootName => this._dmodRootDirName;
        
        // data that gets set during installation...
        public DirectoryInfo? InstallationFinalDestination => this._installationFinalDestination;
        public DirectoryInfo? InstallDestination => this._installationDestination;
        public string? DestinationOverride => this._destinationOverride;
        public DinkInstallerResult InstallResult => this._installResult;
        public Exception? InstallException => this._installException;
        
        
        // private fields
        private readonly DinkTempFileHelper _temp = new DinkTempFileHelper();
        private FileInfo? _tempTarArchive = null;
        
        private DmodInstallPhase _installPhase = DmodInstallPhase.Inactive;
        
        private FileInfo? _sourceFile = null;
        private readonly List<string> _archiveEntries = new List<string>();
        private readonly List<string> _archiveTopLevelEntries = new List<string>();
        private string? _dmodRootDirName = null;

        private DirectoryInfo? _installationFinalDestination = null;
        private DirectoryInfo? _installationDestination = null;
        private string? _destinationOverride = null;
        private DinkInstallerResult _installResult = DinkInstallerResult.Error;
        private Exception? _installException = null;
        
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public MyTrace CustomTrace { get; }

        private readonly object _syncRoot = new object();
        
        private void ReportProgressPrimary() {
            try
            {
               
                // NOTE: the primary progress is the phase itself
                // there are a total of 5 phases to go through...
                this.ProgressReport?.Invoke(this, new DmodInstallerProgressEventArgs(this._installPhase,  this._installResult, (int)this._installPhase / 5.0));
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
        private void ReportActivityStart() {
            try
            {
                this.DmodInstallerActivityStarted?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
        
        private void ReportActivityEnd() {
            try
            {
                this.DmodInstallerActivityEnded?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        private void LogMessage(string line1)
        {
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, line1);
        }
        private void LogMessage(string line1, string line2)
        {
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, [line1, line2]);
        }
        private void LogMessage(string line1, string line2, string line3)
        {
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, [line1, line2, line3]);
        }
        
        public DmodInstaller() {
            this.CustomTrace = new MyTrace(this.GetType().ToString());
            
            this._temp.SetLogCallback(this.LogMessage);
            
            this.ArchiveEntries = new ReadOnlyCollection<string>(this._archiveEntries);
            this.ArchiveTopLevelEntries = new ReadOnlyCollection<string>(this._archiveTopLevelEntries);
        }

        public void Cancel()
        {
            this._cancellationTokenSource.Cancel();

            lock (this._syncRoot)
            {
                if (this._installPhase != DmodInstallPhase.Inactive &&
                    this._installPhase != DmodInstallPhase.AwaitingUserInput) return;
                
                // jump to cleanup state to prevent starting initialization or installation...
                this._installPhase = DmodInstallPhase.Cleanup;
            }
            
            this.LogMessage(Localizer.Instance["DmodInstaller/Log/CancelledByUser"]);
                
            this.CleanUp();
                
            this.CustomTrace.Flush();
            this.CustomTrace.Close();
                
            lock (this._syncRoot)
            {
                this._installPhase = DmodInstallPhase.Finished;
                this._installResult = DinkInstallerResult.Cancelled;
            }
            this.ReportProgressPrimary();
        }

        public void Initialize(FileInfo sourceFile, DmodInstallPreprocessingMode installPreprocessingMode = DmodInstallPreprocessingMode.None) {
            lock (this._syncRoot)
            {
                // some kind of sanity check i suppose?...
                if (this._installPhase != DmodInstallPhase.Inactive || this._sourceFile != null)
                    return;
                
                this._installPhase = DmodInstallPhase.Initializing;
                this._sourceFile = sourceFile;
                this._dmodRootDirName = null;
                this._archiveEntries.Clear();
                this._archiveTopLevelEntries.Clear();
            }

            try
            {
                this.ReportProgressPrimary();

                this._sourceFile.Refresh();
                if (this._sourceFile.Exists == false)
                {
                    this.LogMessage(Localizer.Instance["DmodInstaller/Log/DmodFileNotFound"], $"    \"{this._sourceFile.FullName}\"");
                    throw new DinkInstallerFileSystemException(this._sourceFile.FullName);
                }
                
                // begin initializing DMOD file...
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/InitializingDmodFileStart"], $"    \"{this._sourceFile.FullName}\"");

                switch (installPreprocessingMode)
                {
                    case DmodInstallPreprocessingMode.None:
                    {
                        this.LogMessage(Localizer.Instance["DmodInstaller/Log/InitializedNone"]);
                        lock (this._syncRoot)
                        {
                            this._installPhase = DmodInstallPhase.AwaitingUserInput;
                        }
                        this.ReportProgressPrimary();
                        break;
                    }
                    case DmodInstallPreprocessingMode.QuickPeek:
                    {
                        this.LogMessage(Localizer.Instance["DmodInstaller/Log/InitializingQuickPeek"]);
                        
                        this.ReportActivityStart();
                        bool result = this.TryInitializeQuickPeek();
                        this.ReportActivityEnd();

                        if (result)
                        {
                            this.LogMessage(Localizer.Instance["DmodInstaller/Log/InitializedQuickPeek"], this.DmodRootName ?? string.Empty);

                            lock (this._syncRoot)
                            {
                                this._installPhase = DmodInstallPhase.AwaitingUserInput;
                            }
                            this.ReportProgressPrimary();
                        }
                        else
                        {
                            string message = Localizer.Instance["DmodInstaller/Log/InitializeFailedQuickPeek"];
                            this.LogMessage(message);
                            throw new Exception(message);
                        }
                        break;
                    }
                    case DmodInstallPreprocessingMode.PeekAll:
                    {
                        this.LogMessage(Localizer.Instance["DmodInstaller/Log/InitializingPeekAll"]);
                        this.ReportActivityStart();
                        // try to initialize the DMOD top entries info from the standard bzip2/tar dmod format
                        // if that fails, maybe this is a different archive type like 7z, so try that instead?
                        bool result = this.TryInitializeEntriesFromStandardDmod();
                        if (result == false)
                        {
                            result = this.TryInitializeEntriesFromUnknownDmod();
                        }
                        this.ReportActivityEnd();

                        if (result)
                        {
                            this.LogMessage(Localizer.Instance["DmodInstaller/Log/InitializedPeekAll"]);

                            if (this._archiveTopLevelEntries.Count > 1)
                            {
                                this.LogMessage(Localizer.Instance["DmodInstaller/Log/WarningMultipleTopLevelItems"]);
                            }

                            lock (this._syncRoot)
                            {
                                this._installPhase = DmodInstallPhase.AwaitingUserInput;
                            }
                            this.ReportProgressPrimary();
                        }
                        else
                        {
                            string message = Localizer.Instance["DmodInstaller/Log/InitializeFailedPeekAll"];
                            this.LogMessage(message);
                            throw new Exception(message);
                        }
                        break;
                    }
                }
                
                // finished initializing DMOD file
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/InitializingDmodFileEnd"]);
            } catch (DinkInstallerCancelledByUserException)
            {
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/CancelledByUser"]);
                
                this.CleanUp();
                
                this.CustomTrace.Flush();
                this.CustomTrace.Close();
                
                lock (this._syncRoot)
                {
                    this._installPhase = DmodInstallPhase.Finished;
                    this._installResult = DinkInstallerResult.Cancelled;
                }
                this.ReportProgressPrimary();
            }  catch (Exception ex)
            {
                this._installException = ex;
                this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, this._installException);
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, this._installException);
                
                this.CleanUp();
                
                this.CustomTrace.Flush();
                this.CustomTrace.Close();
                
                lock (this._syncRoot)
                {
                    this._installPhase = DmodInstallPhase.Finished;
                    this._installResult = DinkInstallerResult.Error;
                }
                this.ReportProgressPrimary();
            }
        }

        private bool TryInitializeQuickPeek() {
            this._archiveEntries.Clear();
            this._archiveTopLevelEntries.Clear();
            this._tempTarArchive = null;
            this._dmodRootDirName = null;
            
            if (this._sourceFile == null)
                return false;

            try {
                using FileStream fs = new FileStream(this._sourceFile.FullName, FileMode.Open, FileAccess.Read);
                using IReader reader = ReaderFactory.Open(fs);

                while (reader.MoveToNextEntry()) {
                    if (string.IsNullOrEmpty(reader.Entry.Key))
                        continue;
                    
                    if (this._cancellationTokenSource.IsCancellationRequested) {
                        throw new DinkInstallerCancelledByUserException();
                    }

                    this._archiveEntries.Add(reader.Entry.Key);

                    string[] split = reader.Entry.Key.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length > 1) {
                        if (string.IsNullOrWhiteSpace(this._dmodRootDirName)) {
                            this._dmodRootDirName = split[0];
                            return true;
                        }
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                return false;
            }

            return string.IsNullOrWhiteSpace(this._dmodRootDirName) == false;

        }

        private bool TryInitializeEntriesFromStandardDmod() {
            this._archiveEntries.Clear();
            this._archiveTopLevelEntries.Clear();
            this._tempTarArchive = null;
            this._dmodRootDirName = null;
            
            if (this._sourceFile == null)
                return false;

            try {

                // the DMOD should be a bzip2-ed tar archive...
                using FileStream fs = new FileStream(this._sourceFile.FullName, FileMode.Open, FileAccess.Read);
                using BZip2InputStream decompressor = new BZip2InputStream(fs, false);

                this._tempTarArchive = this._temp.TryCreateTempFile();

                if (this._tempTarArchive == null)
                    return false;

                using FileStream fsTar = new FileStream(this._tempTarArchive.FullName, FileMode.Create, FileAccess.ReadWrite);

                try {
                    decompressor.CopyTo(fsTar);
                } catch (Exception) {
                    this._tempTarArchive = null;
                    return false;
                }

                // now we have an extracted temporary tar file... time to read all its entries...
                fsTar.Seek(0, SeekOrigin.Begin);

                using IReader reader = ReaderFactory.Open(fsTar);
                Dictionary<string, bool> tempTopLevelEntries = new Dictionary<string, bool>();

                while (reader.MoveToNextEntry()) {
                    if (string.IsNullOrEmpty(reader.Entry.Key))
                        continue;
                    
                    if (this._cancellationTokenSource.IsCancellationRequested) {
                        throw new DinkInstallerCancelledByUserException();
                    }

                    this._archiveEntries.Add(reader.Entry.Key);

                    string[] split = reader.Entry.Key.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length > 1) {
                        if (string.IsNullOrEmpty(this._dmodRootDirName)) {
                            this._dmodRootDirName = split[0];
                        }
                        tempTopLevelEntries[split[0]] = true;
                    } else {
                        tempTopLevelEntries[reader.Entry.Key] = true;
                    }
                }

                foreach (var kvp in tempTopLevelEntries) {
                    this._archiveTopLevelEntries.Add(kvp.Key);
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                return false;
            }

            return string.IsNullOrWhiteSpace(this._dmodRootDirName) == false;
        }

        private bool TryInitializeEntriesFromUnknownDmod() {
            this._archiveEntries.Clear();
            this._archiveTopLevelEntries.Clear();
            this._tempTarArchive = null;
            this._dmodRootDirName = null;
            
            if (this._sourceFile == null)
                return false;

            try {
                using FileStream fs = new FileStream(this._sourceFile.FullName, FileMode.Open, FileAccess.Read);
                using IReader reader = ReaderFactory.Open(fs);
                Dictionary<string, bool> tempTopLevelEntries = new Dictionary<string, bool>();

                while (reader.MoveToNextEntry()) {
                    if (string.IsNullOrEmpty(reader.Entry.Key))
                        continue;
                    
                    if (this._cancellationTokenSource.IsCancellationRequested) {
                        throw new DinkInstallerCancelledByUserException();
                    }

                    this._archiveEntries.Add(reader.Entry.Key);

                    string[] split = reader.Entry.Key.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length > 1) {
                        if (string.IsNullOrEmpty(this._dmodRootDirName)) {
                            this._dmodRootDirName = split[0];
                        }
                        tempTopLevelEntries[split[0]] = true;
                    } else {
                        tempTopLevelEntries[reader.Entry.Key] = true;
                    }
                }
                
                foreach (var kvp in tempTopLevelEntries) {
                    this._archiveTopLevelEntries.Add(kvp.Key);
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                return false;
            }

            return string.IsNullOrWhiteSpace(this._dmodRootDirName) == false;
        }

        public void InstallDmod(DirectoryInfo destinationDirectory, string destinationOverride, bool allowOverwrite) {
            lock (this._syncRoot)
            {
                if (this._installPhase != DmodInstallPhase.AwaitingUserInput) return;

                this._installPhase = DmodInstallPhase.Installing;
                this._installationDestination = destinationDirectory;
                this._destinationOverride = destinationOverride;
            }
            
            bool cancelled = false;

            try {
                this.ReportProgressPrimary();
                
                if (this._cancellationTokenSource.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }
                
                // sanity checks for source
                this._sourceFile?.Refresh();
                if (this._sourceFile?.Exists != true)
                {
                    this.LogMessage(Localizer.Instance["DmodInstaller/Log/DmodFileNotFound"], $"    \"{this._sourceFile?.FullName}\"");
                    throw new DinkInstallerFileSystemException(this._sourceFile?.FullName ?? "");
                }
                
                // sanity checks for destination
                if (this._installationDestination.Parent == null) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DmodInstaller/Log/DestinationErrorIsRoot"] + $" \"{destinationDirectory.FullName}\"");
                }
                if (Path.IsPathRooted(destinationDirectory.FullName) == false) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DmodInstaller/Log/DestinationErrorIsNotRooted"] + $" \"{destinationDirectory.FullName}\"");
                }
                
                // extracting DMOD
                this.ReportActivityStart();
                this.ExtractDmod(allowOverwrite);
                this.ReportActivityEnd();
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/DmodExtractEnd"]);
            } catch (DinkInstallerCancelledByUserException) {
                cancelled = true;
                this.LogMessage(Localizer.Instance["DmodInstaller/Log/CancelledByUser"]);
            } catch (Exception ex) {
                this._installException = ex;
                this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, this._installException);
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, this._installException);
            } finally {
                this.ReportActivityEnd();
                
                this.CleanUp();

                this.CustomTrace.Flush();
                this.CustomTrace.Close();
                
                lock (this._syncRoot)
                {
                    this._installPhase = DmodInstallPhase.Finished;
                    if (cancelled == false && this._installException == null) this._installResult = DinkInstallerResult.Success;
                    else if (cancelled) this._installResult = DinkInstallerResult.Cancelled;
                    else this._installResult = DinkInstallerResult.Error;
                }
                this.ReportProgressPrimary();
            }
        }


        private void CleanUp() {
            lock (this._syncRoot)
            {
                this._installPhase = DmodInstallPhase.Cleanup;
            }
            this.ReportProgressPrimary();
            
            this.ReportActivityStart();
            this.LogMessage(Localizer.Instance["DmodInstaller/Log/CleaningUpStart"]);
            this._temp.Dispose();
            this.ReportActivityEnd();
            this.LogMessage(Localizer.Instance["DmodInstaller/Log/CleaningUpEnd"]);
        }
        
        private void ExtractDmod(bool allowOverwrite) {
            if (this._sourceFile == null) return;
            if (this._installationDestination == null) return;
            
            if (this._cancellationTokenSource.IsCancellationRequested) {
                throw new DinkInstallerCancelledByUserException();
            }

            bool doTopLevelAppend = this._archiveTopLevelEntries.Count > 1;
            bool doTopLevelReplace = string.IsNullOrWhiteSpace(this._destinationOverride) == false;
            string topLevelOverride = doTopLevelReplace 
                ? this._destinationOverride! // ensured valid by previous check, suppress warning 
                : this._sourceFile.Name.Substring(0, this._sourceFile.Name.Length - this._sourceFile.Extension.Length);
            
            if (string.IsNullOrWhiteSpace(topLevelOverride)) {
                doTopLevelAppend = false;
                doTopLevelReplace = false;
            }

            if (doTopLevelAppend || doTopLevelReplace)
            {
                // check if folder already exists...
                string path =  Path.Combine(this._installationDestination.FullName, topLevelOverride);

                if (Directory.Exists(path) && allowOverwrite == false)
                {
                    string msg = Localizer.Instance["DmodInstaller/Log/DestinationErrorAlreadyExists"];
                    this.LogMessage(msg);
                    throw new DinkInstallerFileSystemException(msg);
                }
            }
            
            string archiveName = this._tempTarArchive == null 
                ? this._sourceFile.FullName
                : this._tempTarArchive.FullName;

            string finalDestination = this._installationDestination.FullName;
            if (doTopLevelAppend) finalDestination = Path.Combine(finalDestination, topLevelOverride, this._dmodRootDirName ?? string.Empty);
            else if (doTopLevelReplace) finalDestination = Path.Combine(finalDestination, topLevelOverride);
            else Path.Combine(finalDestination,  this._dmodRootDirName ?? string.Empty);

            this._installationFinalDestination = new DirectoryInfo(finalDestination);
            
            this.LogMessage(Localizer.Instance["DmodInstaller/Log/DmodExtractStart"], $"    \"{this._sourceFile.FullName}\"", $"    \"{finalDestination}\"");

            using FileStream fs = new FileStream(archiveName, FileMode.Open, FileAccess.Read);
            using IReader reader = ReaderFactory.Open(fs);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int fileCount = 0;
            
            while (reader.MoveToNextEntry())
            {
                fileCount++;
                
                // log a message every 2 seconds so it does not seem like the installer is stuck when dealing with really large DMODs...
                if (stopwatch.ElapsedMilliseconds > 2000)
                {
                    stopwatch.Restart();
                    this.LogMessage(Localizer.Instance["DmodInstaller/Log/DmodExtractingProgress"] + $" {fileCount}...");
                }
                
                if (string.IsNullOrEmpty(reader.Entry.Key))
                    continue;
                        
                if (this._cancellationTokenSource.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

                // archive entries are split using '/' 
                string[] split = reader.Entry.Key.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        
                // recreate path using system specific path separator
                if (doTopLevelAppend) {
                    split[0] = Path.Combine(topLevelOverride, split[0]);
                } else if (doTopLevelReplace) {
                    if (split[0] == this._dmodRootDirName)
                        split[0] = topLevelOverride;
                }
                        
                string relativePath = Path.Combine(split);
                        
                string fullPath = Path.Combine(this._installationDestination.FullName, relativePath);
                        
                if (reader.Entry.IsDirectory == false) {
                    DirectoryInfo? parent = Directory.GetParent(fullPath);
                    if (parent != null && parent.Exists == false) {
                        parent.Create();
                    }
                    reader.WriteEntryToFile(fullPath, new ExtractionOptions() { Overwrite = allowOverwrite, });
                }
            }
        }
    }
}
