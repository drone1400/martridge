using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SevenZipExtractor;

namespace Martridge.Models.Installer {
    public class DinkInstaller {

        public event EventHandler<DinkInstallerProgressEventArgs>? ProgressReport;
        public event EventHandler<DinkInstallerProgressEventArgs>? SecondaryProgressReport;
        public event EventHandler<DinkInstallerDoneEventArgs>? InstallerDone;

        public MyTrace CustomTrace { get; }
        
        private double _progPhaseCurrent = 0;
        private double _progPhaseTotal = 0;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        

        private DinkInstallPhase _installPhase = DinkInstallPhase.Inactive;
        private readonly object _syncRoot = new object();
        
        private readonly TimeSpan _timeoutHttpClient = new TimeSpan(0,0,10);
        private readonly TimeSpan _downloadProgressReportInterval = new TimeSpan(0,0,0,0,500);

        private DinkTempFileHelper _temp = new DinkTempFileHelper();

        public DinkInstaller() {
            this.CustomTrace = new MyTrace(this.GetType().ToString());

            // add text logger listener to trace...
            MyTraceListenerLogger traceLogger = new MyTraceListenerLogger(this.GetType().ToString());
            this.CustomTrace.Listeners.Add(traceLogger);
            
            this._temp.SetLogCallback(this.LogMessage);
        }
        
        private void ReportPrimaryProgress(string heading, string detail = "")
        {
            this.ProgressReport?.Invoke(this, new DinkInstallerProgressEventArgs(heading, detail, this._progPhaseCurrent++ / this._progPhaseTotal));
        }
        
        private void ReportSecondaryProgress(string heading, string detail = "", double progressPercent = double.NaN)
        {
            // NOTE: NaN progress means indeterminate...
            this.SecondaryProgressReport?.Invoke(this, new DinkInstallerProgressEventArgs(heading, detail, progressPercent));
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
        private void LogMessage(string line1, string line2, string line3, string line4)
        {
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, [line1, line2, line3, line4]);
        }
        
        public void Cancel()
        {
            this._cancellationTokenSource.Cancel();

            lock (this._syncRoot)
            {
                if (this._installPhase != DinkInstallPhase.Inactive) return;
                
                // jump to cleanup state to prevent starting initialization or installation...
                this._installPhase = DinkInstallPhase.Cleanup;
            }
            
            this.LogMessage(Localizer.Instance["DmodInstaller/Log/CancelledByUser"]);
                
            this.CleanUp();
                
            this.CustomTrace.Flush();
            this.CustomTrace.Close();
                
            lock (this._syncRoot)
            {
                this._installPhase = DinkInstallPhase.Finished;
            }
            
            this.InstallerDone?.Invoke(this, new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled, null, null));
        }

        public void InstallDink(DirectoryInfo destinationDirectory, bool overrideDestination, ConfigInstaller config) {
            lock (this._syncRoot)
            {
                if (this._installPhase != DinkInstallPhase.Inactive)
                    return;

                this._installPhase = DinkInstallPhase.Preparing;
            }

            bool cancelled = false;
            Exception? exception = null;
                
            try {
                // starting...
                this._progPhaseCurrent = 0;
                this._progPhaseTotal = 2 + 2 * (config.InstallerComponents.Count + 1);
                DirectoryInfo webCacheDir = new DirectoryInfo(LocationHelper.WebCache);
                // log start of installation
                this.LogMessage(
                    Localizer.Instance["DinkInstaller/StartInstalling"],
                    $"    {config.Name}");
                
                // prepare directories
                this.PrepareLocations(config, webCacheDir, destinationDirectory, overrideDestination);
                
                // download resources if needed
                lock (this._syncRoot)
                {
                    this._installPhase = DinkInstallPhase.DownloadingResources;
                }
                this.DownloadResources(config, webCacheDir);
                
                // install dink
                lock (this._syncRoot)
                {
                    this._installPhase = DinkInstallPhase.Installing;
                }
                this.InstallDink(config, webCacheDir, destinationDirectory);
                // All done!
            }catch (DinkInstallerCancelledByUserException) {
                cancelled = true;
                this.LogMessage(Localizer.Instance["DinkInstaller/Heading/CancelledByUser"]);
            } catch (Exception ex) {
                exception = ex;
                this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, exception);
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, exception);
            } finally {
                DinkInstallerResult result;

                this.CleanUp();

                this._progPhaseCurrent = this._progPhaseTotal;
                if (cancelled == false && exception == null) {
                    result = DinkInstallerResult.Success;
                    this.ReportPrimaryProgress(Localizer.Instance["DinkInstaller/Heading/AllDone"]);
                } else if (cancelled) {
                    result = DinkInstallerResult.Cancelled;
                    this.ReportPrimaryProgress(Localizer.Instance["DinkInstaller/Heading/CancelledByUser"]);
                } else {
                    result = DinkInstallerResult.Error;
                    this.ReportPrimaryProgress(Localizer.Instance["DinkInstaller/Heading/ErrorOccured"]);
                }
                
                this.CustomTrace.Flush();
                this.CustomTrace.Close(); // closes all trace listeners...

                lock (this._syncRoot)
                {
                    this._installPhase = DinkInstallPhase.Finished;
                }
                
                this.InstallerDone?.Invoke(this, exception == null
                    ? new DinkInstallerDoneEventArgs(result, config, destinationDirectory) 
                    : new DinkInstallerDoneEventArgs(exception, config, destinationDirectory));
            }
        }

        private void PrepareLocations(ConfigInstaller config, DirectoryInfo webCacheDir, DirectoryInfo destinationDirectory, bool overrideDestination) {
            // start of phase 1
            this.ReportPrimaryProgress(
                Localizer.Instance["DinkInstaller/Heading/Preparing"],
                Localizer.Instance["DinkInstaller/Preparing/PreparingDirectoriesStart"]);
            // do not increment yet...
            this._progPhaseCurrent--;
            this.LogMessage(Localizer.Instance["DinkInstaller/Preparing/PreparingDirectoriesStart"]);

            // safety checks for destination...
            if (destinationDirectory.Parent == null) {
                throw new DinkInstallerFileSystemException(
                    Localizer.Instance["DinkInstaller/Preparing/DestinationErrorIsRoot"] + $" \"{destinationDirectory.FullName}\"");
            }
            if (Path.IsPathRooted(destinationDirectory.FullName) == false) {
                throw new DinkInstallerFileSystemException(
                    Localizer.Instance["DinkInstaller/Preparing/DestinationErrorIsNotRooted"] + $" \"{destinationDirectory.FullName}\"");
            }

            // prepare web cache directory
            this.LogMessage("    " + webCacheDir.FullName);
            this.PrepareDirectories_CreateIfNotExists(webCacheDir);

            // prepare destination directory
            this.LogMessage("    " + destinationDirectory.FullName);
            if (overrideDestination) {
                this.PrepareDirectories_CreateNew(destinationDirectory);
            } else {
                this.PrepareDirectories_CreateIfNotExists(destinationDirectory);
            }

            // end of phase 1
            this.ReportPrimaryProgress(
                Localizer.Instance["DinkInstaller/Heading/Preparing"],
                Localizer.Instance["DinkInstaller/Preparing/PreparingDirectoriesDone"]);
            this.LogMessage(Localizer.Instance["DinkInstaller/Preparing/PreparingDirectoriesDone"]);
        }

        private void DownloadResources(ConfigInstaller config, DirectoryInfo webCacheDir) {
            // start of phase 2
            this.LogMessage(Localizer.Instance["DinkInstaller/DownloadingResources/Start"]);

            foreach (ConfigInstallerComponent comp in config.InstallerComponents) {
                if (this._cancellationTokenSource.IsCancellationRequested)
                    throw new DinkInstallerCancelledByUserException();
                
                this.ReportPrimaryProgress(
                    Localizer.Instance["DinkInstaller/Heading/Downloading"],
                    comp.WebResource.Uri);

                FileInfo finfo = new FileInfo(Path.Combine(webCacheDir.FullName, comp.WebResource.Name));
                // NOTE: I think the cached file should not be cleaned up in this case.. right?...
                //this._temp.RegisterTempFile(finfo);

                this.DownloadFile(finfo, comp.WebResource);
            }

            // end of phase 2
            this.ReportPrimaryProgress(
                Localizer.Instance["DinkInstaller/Heading/Downloading"],
                Localizer.Instance["DinkInstaller/DownloadingResources/Done"]);
            this.LogMessage(Localizer.Instance["DinkInstaller/DownloadingResources/Done"]);
        }

        private void InstallDink(ConfigInstaller config, DirectoryInfo webCacheDir, DirectoryInfo destinationDirectory) {
            // start of phase 3
            this.LogMessage(Localizer.Instance["DinkInstaller/InstallingDink/InstallingResource/Start"]);

            foreach (ConfigInstallerComponent comp in config.InstallerComponents) {
                if (this._cancellationTokenSource.IsCancellationRequested)
                    throw new DinkInstallerCancelledByUserException();
                
                // create a temporary directory to extract the component to
                DirectoryInfo? tmpDirInfo = this._temp.TryCreateTempDirectory();
                if (tmpDirInfo == null)
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DinkInstaller/Preparing/CreatingTempDirectoryError"]);
                
                FileInfo finfo = new FileInfo(Path.Combine(webCacheDir.FullName, comp.WebResource.Name));
                
                this.ReportPrimaryProgress(
                    Localizer.Instance["DinkInstaller/Heading/Installing"],
                    comp.WebResource.Name);

                this.ReportSecondaryProgress(
                    finfo.FullName,
                    Localizer.Instance["DinkInstaller/InstallingDink/InstallingResourceUnzipping/Attempt"],
                    double.NaN);

                bool success = this.TryUnzipFile(finfo, tmpDirInfo, comp.WebResource.ResourceArchiveFormat);

                this.ReportSecondaryProgress(
                    finfo.FullName,
                    Localizer.Instance["DinkInstaller/InstallingDink/InstallingResourceUnzipping/Attempt"],
                    1.0);

                if (!success) {
                    throw new DinkInstallerUnzipException(finfo.FullName);
                }

                DirectoryInfo sourceDirectory = tmpDirInfo;

                if (!string.IsNullOrWhiteSpace(comp.SourceSubFolder)) {
                    sourceDirectory = new DirectoryInfo(Path.Combine(tmpDirInfo.FullName, comp.SourceSubFolder));
                } 
                
                this.MoveDirectoryContents(sourceDirectory, destinationDirectory, comp.FileFilterMode, comp.FileFilterList);
            }
            
            this.ReportPrimaryProgress(
                Localizer.Instance["DinkInstaller/Heading/Installing"],
                Localizer.Instance["DinkInstaller/InstallingDink/InstallingResource/Done"]);
            this._progPhaseCurrent++;
            this.LogMessage(Localizer.Instance["DinkInstaller/InstallingDink/InstallingResource/Done"]);
        }

        private void PrepareDirectories_CreateIfNotExists(DirectoryInfo dirInfo) {
            try {
                dirInfo.Refresh();

                if (dirInfo.Parent == null) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DinkInstaller/Preparing/CreatingDirectoryErrorDirectoryRoot"]);
                }

                if (dirInfo.Exists == false) {
                    this.LogMessage(
                        Localizer.Instance["DinkInstaller/Preparing/CreatingDirectory"],
                        $"    \"{dirInfo.FullName}\"");
                    dirInfo.Create();
                }
            } catch (Exception ex) {
                throw new DinkInstallerFileSystemException(Localizer.Instance["DinkInstaller/Preparing/CreatingDirectoryError"], ex);
            }
        }

        private void PrepareDirectories_CreateNew(DirectoryInfo dirInfo) {
            try {
                dirInfo.Refresh();

                if (dirInfo.Parent == null) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DinkInstaller/Preparing/CreatingDirectoryErrorDirectoryRoot"]);
                }

                if (dirInfo.Exists) {
                    dirInfo.Delete(true);
                }

                this.LogMessage(
                    Localizer.Instance["DinkInstaller/Preparing/CreatingDirectory"],
                    $"    \"{dirInfo.FullName}\"");
                dirInfo.Create();

            } catch (Exception ex) {
                throw new DinkInstallerFileSystemException(Localizer.Instance["DinkInstaller/Preparing/CreatingDirectoryError"], ex);
            }
        }

        private void CleanUp() {
            lock (this._syncRoot)
            {
                this._installPhase = DinkInstallPhase.Cleanup;
            }
            this.ReportPrimaryProgress(Localizer.Instance["DinkInstaller/Heading/Cleanup"]);
            this._progPhaseCurrent++;
            this.LogMessage(Localizer.Instance["DinkInstaller/CleaningUp/Start"]);
            this._temp.Dispose();
            this.LogMessage(Localizer.Instance["DinkInstaller/CleaningUp/Done"]);
        }

        private void IndexAllFilesInDirectory(DirectoryInfo sourceDir, List<FileInfo> outputList) {
            Stack<DirectoryInfo> dirsToIndex = new Stack<DirectoryInfo>();
            dirsToIndex.Push(sourceDir);

            // searching remaining directories...
            do {
                if (this._cancellationTokenSource.IsCancellationRequested)
                    throw new DinkInstallerCancelledByUserException();

                DirectoryInfo currentDir = dirsToIndex.Pop();

                FileInfo[] files = currentDir.GetFiles();
                DirectoryInfo[] dirs = currentDir.GetDirectories();

                foreach (FileInfo file in files) {
                    outputList.Add(file);
                }

                foreach (DirectoryInfo dir in dirs) {
                    dirsToIndex.Push(dir);
                }

            } while (dirsToIndex.Count > 0);
        }

        private void IndexFilteredFilesInDirectory(DirectoryInfo sourceDir, List<FileInfo> outputList, List<string> filterList, bool logMissingFiles) {
            foreach (string filteredName in filterList) {
                if (string.IsNullOrWhiteSpace(filteredName)) {
                    throw new DinkInstallerFileSystemException($"Filter string path can not be empty!");
                }
                if (filteredName.StartsWith(".") || filteredName.StartsWith("..")) {
                    throw new DinkInstallerFileSystemException($"Filter string path can not start with a '.' character: \"{filteredName}\"");
                }

                string fullwhite = Path.Combine(sourceDir.FullName, filteredName);

                FileInfo theFile = new FileInfo(fullwhite);
                DirectoryInfo theDir = new DirectoryInfo(fullwhite);
                if (theFile.Exists) {
                    outputList.Add(theFile);
                } else if (theDir.Exists) {
                    this.IndexAllFilesInDirectory(theDir, outputList);
                } else {
                    if (logMissingFiles) {
                        this.LogMessage(
                            Localizer.Instance["DinkInstaller/DinkInstaller/InstallingDink/MovingFiles/NotFound"],
                            $"    Path        = {fullwhite}");
                        //TODO... maybe give the user a confirmation warning or something?....
                    }

                }
            }
        }

        private void MoveDirectoryContents(DirectoryInfo source, DirectoryInfo destination, InstallerFiltering filterMode, List<string> filterList) {
            List<FileInfo> filesToMove = new List<FileInfo>();
            List<FileInfo> bannedFiles = new List<FileInfo>();
            Dictionary<string, bool>? bannedDict = null;

            this.LogMessage(
                Localizer.Instance["DinkInstaller/InstallingDink/MovingFiles/Start"],
                $"    Source      = {source.FullName}",
                $"    Destination = {destination.FullName}",
                Localizer.Instance["DinkInstaller/InstallingDink/MovingFiles/Indexing"]);
            
            switch(filterMode) {
                case InstallerFiltering.NoFiltering:
                    // index all of the source contents
                    this.IndexAllFilesInDirectory(source, filesToMove);
                    break;
                case InstallerFiltering.UseBlackList:
                    // index all of the source contents
                    this.IndexAllFilesInDirectory(source, filesToMove);

                    // index the blacklisted stuff...
                    this.IndexFilteredFilesInDirectory(source, bannedFiles, filterList, true);

                    // make a banned dictionary
                    bannedDict = new Dictionary<string, bool>();
                    foreach (FileInfo file in bannedFiles) {
                        string relativePath = Path.GetRelativePath(source.FullName, file.FullName);
                        bannedDict.Add(relativePath, true);
                    }

                    break;
                case InstallerFiltering.UseWhiteList:
                    // index only filtered files
                    this.IndexFilteredFilesInDirectory(source, filesToMove, filterList, true);
                    break;
            }

            double progressCount = 0;
            double progressTotal = filesToMove.Count;

            this.LogMessage(
                Localizer.Instance["DinkInstaller/InstallingDink/MovingFiles/ActuallyMoving"]
                    + $"({filesToMove.Count} {Localizer.Instance["DinkInstaller/InstallingDink/MovingFiles/FilesWord"]})");

            foreach (FileInfo file in filesToMove) {
                if (this._cancellationTokenSource.IsCancellationRequested)
                    throw new DinkInstallerCancelledByUserException();

                string relativePath = Path.GetRelativePath(source.FullName, file.FullName);
                string newPath = Path.Combine(destination.FullName, relativePath);
                FileInfo newFile = new FileInfo(newPath);

                int indentLevel = 0;
                string indentStr = "";
                for (int i = 0; i < relativePath.Length; i++) {
                    if (relativePath[i] == '\\' || relativePath[i] == '/') {
                        indentLevel++;
                        indentStr += "    ";
                    }
                }

                this.ReportSecondaryProgress(
                    Localizer.Instance["DinkInstaller/InstallingDink/MovingFiles/ActuallyMoving"],
                    relativePath,
                    progressCount++ / progressTotal);
                

                if (bannedDict == null || bannedDict.ContainsKey(relativePath) == false) {
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller,
                        new List<string>() { indentStr + relativePath },
                        MyTraceLevel.Verbose);

                    newFile.Directory?.Refresh();
                    if (newFile.Directory?.Exists == false) {
                        newFile.Directory.Create();
                    }

                    file.MoveTo(newPath, true);
                } else {
                    this.LogMessage($"{indentStr}\"{relativePath}\" (blacklisted!)");
                }
            }

            this.ReportSecondaryProgress(
                Localizer.Instance["DinkInstaller/InstallingDink/MovingFiles/Done"],
                "",
                1.0);
            this.LogMessage(Localizer.Instance["DinkInstaller/InstallingDink/MovingFiles/Done"]);
        }

        private bool TryUnzipFile(FileInfo file, DirectoryInfo destination, string expectedFormatName) {
            
            if (!Enum.TryParse( expectedFormatName, out SevenZipFormat expectedFormat))
            {
                this.LogMessage(
                    Localizer.Instance["DinkInstaller/InstallingDink/InstallingResourceUnzipping/ErrorParsing7ZipFormat"],
                    $"    FormatName  = \"{ expectedFormatName }\"");
            }
            
            this.LogMessage(
                Localizer.Instance["DinkInstaller/InstallingDink/InstallingResourceUnzipping/Attempt"],
                $"    Archive  = \"{ expectedFormat }\"",
                $"    Source   = \"{ file.FullName }\"",
                $"    Dest     = \"{ destination.FullName }\"");

            bool success = this.TryUnzipFile_SingleFormat(file, destination, expectedFormat);
            if (!success) {
                // failed to extract archive with expected format... attempt all known formats?..
                Array enumVals = Enum.GetValues(typeof(SevenZipFormat));
                foreach (SevenZipFormat format in enumVals) {
                    if (this._cancellationTokenSource.IsCancellationRequested) {
                        throw new DinkInstallerCancelledByUserException();
                    }

                    if (format != expectedFormat) {
                        this.LogMessage(
                            Localizer.Instance["DinkInstaller/InstallingDink/InstallingResourceUnzipping/Error"],
                            $"    Archive  = \"{ format }\"");

                        success = this.TryUnzipFile_SingleFormat(file, destination, format);

                        if (success) {
                            this.LogMessage(Localizer.Instance["DinkInstaller/InstallingDink/InstallingResourceUnzipping/Success"]);
                            return true;
                        }
                    }
                }
            } else {
                this.LogMessage(Localizer.Instance["DinkInstaller/InstallingDink/InstallingResourceUnzipping/Success"]);
                return true;
            }

            this.LogMessage(Localizer.Instance["DinkInstaller/InstallingDink/InstallingResourceUnzipping/Failure"]);
            return false;
        }

        private bool TryUnzipFile_SingleFormat(FileInfo file, DirectoryInfo destination, SevenZipFormat format) {
            try {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                using (ArchiveFile archive = new ArchiveFile(fs, format)) {

                    // this is MUCH faster than extracting each file one by one...
                    // probably because otherwise 7z has to seek to each file
                    archive.Extract(destination.FullName, true);
                }

                return true;
            } catch (Exception) {
                return false;
            }
        }
        
        private void DownloadFile(FileInfo dest, ConfigWebResource res) {
            HttpClient client = new HttpClient();
            client.Timeout = this._timeoutHttpClient;

            try {
                string sha256;

                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    Localizer.Instance["DinkInstaller/DownloadingResources/Prepare"],
                    $"    Name         = \"{res.Name}\"",
                    $"    Source       = \"{res.Uri}\"",
                    $"    Destination  = \"{dest.FullName}\"",
                    $"    Check SHA256 = \"{res.CheckSha256}\"",
                    $"    SHA256       = \"{res.Sha256}\"",
                });

                dest.Refresh();
                if (dest.Exists) {
                    this.LogMessage(Localizer.Instance["DinkInstaller/DownloadingResources/FileAlreadyFound"]);

                    if (res.CheckSha256) {
                        sha256 = HashHelper.ComputeFileSha256Hash(dest.FullName);
                        if (sha256 == res.Sha256) {
                            this.LogMessage(
                                Localizer.Instance["DinkInstaller/DownloadingResources/FileHashMatch"],
                                Localizer.Instance["DinkInstaller/DownloadingResources/UseExistingFile"]);
                            return;
                        } else {
                            this.LogMessage(
                                Localizer.Instance["DinkInstaller/DownloadingResources/FileHashMismatch"],
                                $"    File     SHA256 = \"{sha256}\"",
                                $"    Expected SHA256 = \"{res.Sha256}\"");
                            // proceed to download new file...
                        }
                    } else
                    {
                        // NOTE: only use local file if it's younger than 3 days... otherwise redownload to make sure we have latest version...
                        TimeSpan age = DateTime.Now - dest.LastWriteTime;
                        if (Math.Floor(age.TotalDays) <= 3.0)
                        {
                            // TODO maybe check file length against web resource?...
                            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                                Localizer.Instance["DinkInstaller/DownloadingResources/UseExistingFile"],
                            });
                            return;
                        }
                    }
                }

                this.LogMessage(
                    Localizer.Instance["DinkInstaller/DownloadingResources/DownloadingFile"],
                    res.Uri);

                long downloaded;

                FileInfo? tempDownloadFile = this._temp.TryCreateTempFile();
                if (tempDownloadFile == null)
                    throw new DinkInstallerFileSystemException(Localizer.Instance["DinkInstaller/Preparing/CreatingTempFileError"]);

                Task<HttpResponseMessage> httpTask = client.GetAsync(res.Uri, HttpCompletionOption.ResponseHeadersRead);
                httpTask.Wait();
                using (HttpResponseMessage response = httpTask.Result)
                using (FileStream fstream = new FileStream(tempDownloadFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    Task<Stream> readTask = response.Content.ReadAsStreamAsync();
                    readTask.Wait();
                    
                    Stream webstream = readTask.Result;

                    byte[] buffer = new byte[65536];
                    downloaded = 0;
                    DateTime started = DateTime.Now;
                    DateTime nextReport = started + this._downloadProgressReportInterval;

                    while (true) {
                        // check if user is cancelling operation...
                        if (this._cancellationTokenSource.IsCancellationRequested) {
                            this.LogMessage(
                                    Localizer.Instance["DinkInstaller/DownloadingResources/DownloadingCancelled"],
                                    res.Uri);
                            throw new DinkInstallerCancelledByUserException();
                        }

                        int count = webstream.Read(buffer,0, buffer.Length);

                        if (count != 0) {
                            // write bytes to file stream
                            downloaded += count;
                            fstream.Write(buffer, 0, count);

                            double progress = 0;
                            double totalsize = fstream.Length;
                            if (totalsize != 0) {
                                progress = downloaded / totalsize;
                            }

                            // check if need to report progress...
                            if (DateTime.Now > nextReport) {
                                this.ReportSecondaryProgress(res.Name, res.Uri, progress);
                                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                                        $"    ({(int)Math.Ceiling((double)downloaded/1024)}kB/{(int)Math.Ceiling(totalsize/1024.0)}kB)"
                                    });
                                nextReport = DateTime.Now + this._downloadProgressReportInterval;
                            }
                        } else {
                            // done downloading, break out of loop!
                            break;
                        }
                    }
                }
                
                // send a final progress report 
                this.ReportSecondaryProgress(res.Name, res.Uri, 1.0);
                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    $"    ({(int)Math.Ceiling((double)downloaded/1024)}kB/{(int)Math.Ceiling((double)downloaded/1024)}kB)"
                });

                // check SHA256 if necessary
                if (res.CheckSha256) {
                    sha256 = HashHelper.ComputeFileSha256Hash(tempDownloadFile.FullName);
                    if (sha256 == res.Sha256) {
                        this.LogMessage(Localizer.Instance["DinkInstaller/DownloadingResources/FileHashMatch"]);
                    } else {
                        this.LogMessage(
                            Localizer.Instance[@"DinkInstaller/DownloadingResources/FileHashMismatch"],
                            $"    File     SHA256 = \"{sha256}\"",
                            $"    Expected SHA256 = \"{res.Sha256}\"");
                        throw new DinkInstallerDownloadException("Checksum mismatch...");
                    }
                }
                
                // file seems to have been downloaded correctly...
                tempDownloadFile.MoveTo(dest.FullName, true);
            } catch (DinkInstallerCancelledByUserException) {
                // just forward exception?
                throw;
            } catch (Exception ex) {
                throw new DinkInstallerDownloadException(null, ex);
            } finally {
                client.CancelPendingRequests();
                client.Dispose();
            }
        }
    }
}