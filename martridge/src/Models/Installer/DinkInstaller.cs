using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#if PLATF_WINDOWS
using SevenZipExtractor;
#endif

namespace Martridge.Models.Installer {
    public class DinkInstaller : InstallerBase {

        public event EventHandler<DinkInstallerDoneEventArgs>? InstallerDone;

        // download config parameters, maybe make them not be hardcoded in the future?...
        private readonly TimeSpan _timeoutHttpClient = new TimeSpan(0,0,10);
        private readonly TimeSpan _downloadProgressReportInterval = new TimeSpan(0,0,0,0,500);

        private readonly List<DirectoryInfo> _tempDirs = new List<DirectoryInfo>();
        private readonly List<FileInfo> _tempFiles = new List<FileInfo>();

        #if PLATF_WINDOWS

        public void StartInstallingDink(DirectoryInfo destinationDirectory, bool overrideDestination, ConfigInstaller config, bool cleanupDownloadsWhenDone = false) {
            // only allow installer to run once
            if (this.IsBusy || this.IsDone) return;
            
            this.IsBusy = true;

            Task task = new Task( async () => {
                bool cancelled = false;
                Exception? exception = null;
                
                try {
                    // starting...
                    this.StartTime = DateTime.Now;
                    this.ProgPhaseCurrent = 0;
                    this.ProgPhaseTotal = 2 + 2 * (config.InstallerComponents.Count + 1);
                    DirectoryInfo webCacheDir = new DirectoryInfo(LocationHelper.WebCache);
                    // log start of installation
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        "",
                        Localizer.Instance[@"DinkInstaller/StartInstalling"],
                        $"    {config.Name}",
                    });
                    // prepare directories
                    this.PrepareLocations(config, webCacheDir, destinationDirectory, overrideDestination);
                    // download resources if needed
                    await this.DownloadResources(config, webCacheDir);
                    // install dink
                    this.InstallDink(config, webCacheDir, destinationDirectory);
                    // All done!
                }catch (DinkInstallerCancelledByUserException) {
                    cancelled = true;

                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        Localizer.Instance[@"DinkInstaller/Heading/CancelledByUser"],
                    }, MyTraceLevel.Warning);
                } catch (Exception ex) {
                    exception = ex;

                    this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, exception);
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, exception);
                } finally {
                    DinkInstallerResult result;
                    
                    this.CleanUp(cleanupDownloadsWhenDone);

                    if (cancelled == false && exception == null) {
                        result = DinkInstallerResult.Success;
                        this.ReportProgress(InstallerReportLevel.Primary,
                            Localizer.Instance[@"DinkInstaller/Heading/AllDone"],
                            "",
                            1.0);
                    } else if (cancelled) {
                        result = DinkInstallerResult.Cancelled;
                        this.ReportProgress(InstallerReportLevel.Primary,
                            Localizer.Instance[@"DinkInstaller/Heading/CancelledByUser"],
                            "",
                            1.0);
                    } else {
                        result = DinkInstallerResult.Error;
                        this.ReportProgress(InstallerReportLevel.Primary,
                            Localizer.Instance[@"DinkInstaller/Heading/ErrorOccured"],
                            "",
                            1.0);
                    }

                    this.CustomTrace.Flush();
                    this.CustomTrace.Close(); // closes all trace listeners...

                    this.IsDone = true;
                    this.IsBusy = false;
                    this.EndTime = DateTime.Now;

                    this.InstallerDone?.Invoke(this, exception == null
                        ? new DinkInstallerDoneEventArgs(result, config, destinationDirectory) 
                        : new DinkInstallerDoneEventArgs(exception, config, destinationDirectory));
                }
            });

            task.Start();
        }

        private void PrepareLocations(ConfigInstaller config, DirectoryInfo webCacheDir, DirectoryInfo destinationDirectory, bool overrideDestination) {
            // start of phase 1
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/Preparing"],
                Localizer.Instance[@"DinkInstaller/Preparing/PreparingDirectoriesStart"],
                this.ProgPhaseCurrent / this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/Preparing/PreparingDirectoriesStart"]
            });

            // safety checks for destination...
            if (destinationDirectory.Parent == null) {
                throw new DinkInstallerFileSystemException(
                    Localizer.Instance[@"DinkInstaller/Preparing/DestinationErrorIsRoot"] + $" \"{destinationDirectory.FullName}\"");
            }
            if (Path.IsPathRooted(destinationDirectory.FullName) == false) {
                throw new DinkInstallerFileSystemException(
                    Localizer.Instance[@"DinkInstaller/Preparing/DestinationErrorIsNotRooted"] + $" \"{destinationDirectory.FullName}\"");
            }

            // prepare web cache directory
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "    " + webCacheDir.FullName
            });
            this.PrepareDirectories_CreateIfNotExists(webCacheDir);

            // prepare destination directory
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "    " + destinationDirectory.FullName
            });
            if (overrideDestination) {
                this.PrepareDirectories_CreateNew(destinationDirectory);
            } else {
                this.PrepareDirectories_CreateIfNotExists(destinationDirectory);
            }

            // prepare temp dirs for all the resources...
            DirectoryInfo theTempDir = new DirectoryInfo(Path.GetTempPath());
            foreach (ConfigInstallerComponent comp in config.InstallerComponents) {
                if (this.CancelToken.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

                // create a temporary subdirectory for the resource unzipping process
                DirectoryInfo tempDirInfo = theTempDir.CreateSubdirectory(Path.Combine("dink_temp", Path.GetRandomFileName()));
                this._tempDirs.Add(tempDirInfo);
            }

            // end of phase 1
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/Preparing"],
                Localizer.Instance[@"DinkInstaller/Preparing/PreparingDirectoriesDone"],
                this.ProgPhaseCurrent++ / this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/Preparing/PreparingDirectoriesDone"],
                "",
            });
        }

        private async Task DownloadResources(ConfigInstaller config, DirectoryInfo webCacheDir) {
            // start of phase 2
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/DownloadingResources/Start"],
            });

            foreach (ConfigInstallerComponent comp in config.InstallerComponents) {
                if (this.CancelToken.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

                this.ReportProgress(InstallerReportLevel.Primary,
                    Localizer.Instance[@"DinkInstaller/Heading/Downloading"],
                    comp.WebResource.Uri,
                    this.ProgPhaseCurrent++ / this.ProgPhaseTotal);

                FileInfo finfo = new FileInfo(Path.Combine(webCacheDir.FullName, comp.WebResource.Name));
                this._tempFiles.Add(finfo);

                await this.DownloadFile(finfo, comp.WebResource);
            }

            // end of phase 2
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/Downloading"],
                Localizer.Instance[@"DinkInstaller/DownloadingResources/Done"],
                this.ProgPhaseCurrent++ / this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/DownloadingResources/Done"],
                "",
            });
        }

        private void InstallDink(ConfigInstaller config, DirectoryInfo webCacheDir, DirectoryInfo destinationDirectory) {
            // start of phase 3
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResource/Start"],
                "",
            });

            foreach (ConfigInstallerComponent comp in config.InstallerComponents) {
                if (this.CancelToken.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

                string tempName = Path.GetFileNameWithoutExtension(comp.WebResource.Name) + "_tmp";
                DirectoryInfo tempDirInfo = new DirectoryInfo(Path.Combine(webCacheDir.FullName,tempName));
                FileInfo finfo = new FileInfo(Path.Combine(webCacheDir.FullName, comp.WebResource.Name));

                this.ReportProgress(InstallerReportLevel.Primary,
                    Localizer.Instance[@"DinkInstaller/Heading/Installing"],
                    comp.WebResource.Name,
                    this.ProgPhaseCurrent++ / this.ProgPhaseTotal);

                this.ReportProgress(InstallerReportLevel.Indeterminate,
                    finfo.FullName,
                    Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResourceUnzipping/Attempt"],
                    0.0);

                bool success = this.TryUnzipFile(finfo, tempDirInfo, comp.WebResource.ResourceArchiveFormat);

                this.ReportProgress(InstallerReportLevel.Indeterminate,
                    finfo.FullName,
                    Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResourceUnzipping/Attempt"],
                    1.0);

                if (!success) {
                    throw new DinkInstallerUnzipException(finfo.FullName);
                }

                DirectoryInfo sourceDirectory = tempDirInfo;

                if (!string.IsNullOrWhiteSpace(comp.SourceSubFolder)) {
                    sourceDirectory = new DirectoryInfo(Path.Combine(tempDirInfo.FullName, comp.SourceSubFolder));
                } 
                
                this.MoveDirectoryContents(sourceDirectory, destinationDirectory, comp.FileFilterMode, comp.FileFilterList);
            }
            
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/Installing"],
                Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResource/Done"],
                this.ProgPhaseCurrent++ / this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResource/Done"],
            });
        }

        private void PrepareDirectories_CreateIfNotExists(DirectoryInfo dirInfo) {
            try {
                dirInfo.Refresh();

                if (dirInfo.Parent == null) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance[@"DinkInstaller/Preparing/CreatingDirectoryErrorDirectoryRoot"]);
                }

                if (dirInfo.Exists == false) {
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        Localizer.Instance[@"DinkInstaller/Preparing/CreatingDirectory"],
                        $"    \"{dirInfo.FullName}\""
                    });
                    dirInfo.Create();
                }
            } catch (Exception ex) {
                throw new DinkInstallerFileSystemException(Localizer.Instance[@"DinkInstaller/Preparing/CreatingDirectoryError"], ex);
            }
        }

        private void PrepareDirectories_CreateNew(DirectoryInfo dirInfo) {
            try {
                dirInfo.Refresh();

                if (dirInfo.Parent == null) {
                    throw new DinkInstallerFileSystemException(Localizer.Instance[@"DinkInstaller/Preparing/CreatingDirectoryErrorDirectoryRoot"]);
                }

                if (dirInfo.Exists) {
                    dirInfo.Delete(true);
                }

                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    Localizer.Instance[@"DinkInstaller/Preparing/CreatingDirectory"],
                    $"    \"{dirInfo.FullName}\""
                });
                dirInfo.Create();

            } catch (Exception ex) {
                throw new DinkInstallerFileSystemException(Localizer.Instance[@"DinkInstaller/Preparing/CreatingDirectoryError"], ex);
            }
        }

        private void CleanUp(bool removeFiles) {
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/Cleanup"],
                "",
                this.ProgPhaseCurrent / this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/CleaningUp/Start"],
            });

            foreach (DirectoryInfo dir in this._tempDirs) {
                try {
                    dir.Refresh();
                    if (dir.Exists) {
                        this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        $"    {dir.FullName}",
                    });
                        dir.Delete(true);
                    }
                } catch (Exception ex) {
                    this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            }

            if (removeFiles) {

                foreach (FileInfo file in this._tempFiles) {
                    try {
                        file.Refresh();
                        if (file.Exists) {
                            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                            $"    {file.FullName}",
                        });
                            file.Delete();
                        }
                    } catch (Exception ex) {
                        this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, ex);
                    }
                }
            }

            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/CleaningUp/Done"],
            });
        }

        private void IndexAllFilesInDirectory(DirectoryInfo sourceDir, List<FileInfo> outputList) {
            Stack<DirectoryInfo> dirsToIndex = new Stack<DirectoryInfo>();
            dirsToIndex.Push(sourceDir);

            // searching remaining directories...
            do {
                if (this.CancelToken.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

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
                        this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                            Localizer.Instance[@"DinkInstaller/DinkInstaller/InstallingDink/MovingFiles/Not"],
                            $"    Path        = {fullwhite}",
                        }, MyTraceLevel.Warning);
                        //TODO... maybe give the user a confirmation warning or something?....
                    }

                }
            }
        }

        private void MoveDirectoryContents(DirectoryInfo source, DirectoryInfo destination, InstallerFiltering filterMode, List<string> filterList) {
            List<FileInfo> filesToMove = new List<FileInfo>();
            List<FileInfo> bannedFiles = new List<FileInfo>();
            Dictionary<string, bool>? bannedDict = null;

            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/InstallingDink/MovingFiles/Start"],
                $"    Source      = {source.FullName}",
                $"    Destination = {destination.FullName}",
                Localizer.Instance[@"DinkInstaller/InstallingDink/MovingFiles/Indexing"],
            });                
            
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

            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/InstallingDink/MovingFiles/ActuallyMoving"]
                        + $"({filesToMove.Count} {Localizer.Instance[@"DinkInstaller/InstallingDink/MovingFiles/FilesWord"]})",
            });

            foreach (FileInfo file in filesToMove) {
                if (this.CancelToken.IsCancellationRequested) {
                    throw new DinkInstallerCancelledByUserException();
                }

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

                this.ReportProgress(InstallerReportLevel.Secondary,
                    Localizer.Instance[@"DinkInstaller/InstallingDink/MovingFiles/ActuallyMoving"],
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
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller,
                        new List<string>() { $"{indentStr}\"{relativePath}\" (blacklisted!)" },
                        MyTraceLevel.Verbose);
                }
            }

            this.ReportProgress(InstallerReportLevel.Secondary,
                Localizer.Instance[@"DinkInstaller/InstallingDink/MovingFiles/Done"],
                "",
                1.0);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/InstallingDink/MovingFiles/Done"],
                "",
            });
        }

        private bool TryUnzipFile(FileInfo file, DirectoryInfo destination, string expectedFormatName) {
            
            if (!Enum.TryParse( expectedFormatName, out SevenZipFormat expectedFormat))
            {
                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResourceUnzipping/ErrorParsing7ZipFormat"],
                    $"    FormatName  = \"{ expectedFormatName }\"",
                });
            }
            
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResourceUnzipping/Attempt"],
                $"    Archive  = \"{ expectedFormat }\"",
                $"    Source   = \"{ file.FullName }\"",
                $"    Dest     = \"{ destination.FullName }\"",
            });

            bool success = this.TryUnzipFile_SingleFormat(file, destination, expectedFormat);
            if (!success) {
                // failed to extract archive with expected format... attempt all known formats?..
                Array enumVals = Enum.GetValues(typeof(SevenZipFormat));
                foreach (SevenZipFormat format in enumVals) {
                    if (this.CancelToken.IsCancellationRequested) {
                        throw new DinkInstallerCancelledByUserException();
                    }

                    if (format != expectedFormat) {
                        this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                            Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResourceUnzipping/Error"],
                            $"    Archive  = \"{ format }\"",
                        });

                        success = this.TryUnzipFile_SingleFormat(file, destination, format);

                        if (success) {
                            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                                Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResourceUnzipping/Success"]
                            });
                            return true;
                        }
                    }
                }
            } else {
                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResourceUnzipping/Success"]
                });
                return true;
            }

            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/InstallingDink/InstallingResourceUnzipping/Failure"]
            });
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
        
        private async Task DownloadFile(FileInfo dest, ConfigWebResource res) {
            HttpClient client = new HttpClient();
            client.Timeout = this._timeoutHttpClient;

            try {
                string sha256;

                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    "",
                    Localizer.Instance[@"DinkInstaller/DownloadingResources/Prepare"],
                    $"    Name         = \"{res.Name}\"",
                    $"    Source       = \"{res.Uri}\"",
                    $"    Destination  = \"{dest.FullName}\"",
                    $"    Check SHA256 = \"{res.CheckSha256}\"",
                    $"    SHA256       = \"{res.Sha256}\"",
                });

                dest.Refresh();
                if (dest.Exists) {
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        Localizer.Instance[@"DinkInstaller/DownloadingResources/FileAlreadyFound"]
                    });

                    if (res.CheckSha256) {
                        sha256 = HashHelper.ComputeFileSha256Hash(dest.FullName);
                        if (sha256 == res.Sha256) {
                            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                                Localizer.Instance[@"DinkInstaller/DownloadingResources/FileHashMatch"],
                                Localizer.Instance[@"DinkInstaller/DownloadingResources/UseExistingFile"],
                            });
                            return;
                        } else {
                            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                                Localizer.Instance[@"DinkInstaller/DownloadingResources/FileHashMismatch"],
                                $"    File     SHA256 = \"{sha256}\"",
                                $"    Expected SHA256 = \"{res.Sha256}\"",
                            });
                            // proceed to download new file...
                        }
                    } else {
                        // TODO maybe check file length against web resource?...
                        this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                            Localizer.Instance[@"DinkInstaller/DownloadingResources/UseExistingFile"],
                        });
                        return;
                    }
                }

                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    Localizer.Instance[@"DinkInstaller/DownloadingResources/DownloadingFile"],
                    res.Uri
                });

                long downloaded;

                using (HttpResponseMessage response = await client.GetAsync(res.Uri, HttpCompletionOption.ResponseHeadersRead))
                using (FileStream fstream = new FileStream(dest.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)) {

                    Stream webstream = await response.Content.ReadAsStreamAsync();

                    byte[] buffer = new byte[65536];
                    downloaded = 0;
                    DateTime started = DateTime.Now;
                    DateTime nextReport = started + this._downloadProgressReportInterval;

                    while (true) {
                        // check if user is cancelling operation...
                        if (this.CancelToken.IsCancellationRequested) {
                            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                                    Localizer.Instance[@"DinkInstaller/DownloadingResources/DownloadingCancelled"],
                                    res.Uri
                                });

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
                                this.ReportProgress(InstallerReportLevel.Secondary,
                                    res.Name,
                                    res.Uri,
                                    progress);
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
                this.ReportProgress(InstallerReportLevel.Secondary,
                    res.Name,
                    res.Uri,
                    1.0);
                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    $"    ({(int)Math.Ceiling((double)downloaded/1024)}kB/{(int)Math.Ceiling((double)downloaded/1024)}kB)"
                });

                // check SHA256 if necessary
                if (res.CheckSha256) {
                    sha256 = HashHelper.ComputeFileSha256Hash(dest.FullName);
                    if (sha256 == res.Sha256) {
                        this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                            Localizer.Instance[@"DinkInstaller/DownloadingResources/FileHashMatch"],
                        });
                        // all done
                        return;
                    } else {
                        this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                            Localizer.Instance[@"DinkInstaller/DownloadingResources/FileHashMismatch"],
                            $"    File     SHA256 = \"{sha256}\"",
                            $"    Expected SHA256 = \"{res.Sha256}\"",
                        });
                        throw new DinkInstallerDownloadException("Checksum mismatch...");
                    }
                } else {
                    // all done
                    return;
                }

                // this should be unreachable...
                // return false;
            } catch (DinkInstallerCancelledByUserException ex) {
                // just forward exception?
                throw ex;
            } catch (Exception ex) {
                throw new DinkInstallerDownloadException(null, ex);
            } finally {
                client.CancelPendingRequests();
                client.Dispose();

                dest.Refresh();
                if (this.CancelToken.IsCancellationRequested && dest.Exists) {
                    dest.Delete();
                }
            }
        }
        
        #else

        //
        // NOTE, normally this should never get called in the first place,
        // this is here just so that I don't have to modify the DinkInstallerViewModel for different platforms
        //
        
        public void StartInstallingDink(DirectoryInfo destinationDirectory, bool overrideDestination, ConfigInstaller config, bool cleanupDownloadsWhenDone = false) {
            
            if (this.IsBusy || this.IsDone) return;

            this.IsBusy = true;
            
            

            Task task = new Task( async () => {
                bool cancelled = false;
                Exception? exception = null;

                try {
                    // starting...
                    this.StartTime = DateTime.Now;

                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        "",
                        Localizer.Instance[@"DinkInstaller/NotSupportedInBuild"],
                        $"    {config.Name}",
                    });
                    
                    exception = new NotSupportedException(Localizer.Instance[@"DinkInstaller/NotSupportedInBuild"]);;
                } catch (Exception ex) {
                    exception = ex;

                    this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, exception);
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, exception);
                } finally {
                    this.CustomTrace.Flush();
                    this.CustomTrace.Close(); // closes all trace listeners...

                    this.IsDone = true;
                    this.IsBusy = false;
                    this.EndTime = DateTime.Now;

                    this.InstallerDone?.Invoke(this,new DinkInstallerDoneEventArgs(exception, config, destinationDirectory));
                }
            });

            task.Start();
        }

        #endif
    }
}