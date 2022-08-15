using Martridge.Models.Localization;
using Martridge.Trace;
using SharpCompress.Common;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Writers.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Martridge.Models.Installer {
    public class DmodPacker : InstallerBase {
        public event EventHandler<DmodPackerDoneEventArgs>? InstallerDone;

        private readonly List<DirectoryInfo> _tempDirs = new List<DirectoryInfo>();
        private readonly List<FileInfo> _tempFiles = new List<FileInfo>();

        public void StartPackingDmod(FileInfo destinationFile, DirectoryInfo sourceDirectory) {
            // only allow installer to run once
            if (this.IsBusy || this.IsDone) {
                return;
            }

            Task task = new Task(async () => {
                bool cancelled = false;
                Exception? exception = null;

                try {
                    // starting...
                    this.IsBusy = true;
                    this.StartTime = DateTime.Now;
                    this.ProgPhaseCurrent = 0;
                    this.ProgPhaseTotal = 5;
                    // log start of installation
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        "",
                        Localizer.Instance[@"DinkInstaller/StartInstalling"],
                        $"    \"{sourceDirectory.FullName}\"",
                        $"    \"{destinationFile.FullName}\"",
                    });
                    // preparations
                    this.PrepareLocations(sourceDirectory, destinationFile);
                    // pack dmod
                    this.PackDmodToFile_SharpCompress(destinationFile, sourceDirectory);
                    // all done!
                } catch (DinkInstallerCancelledByUserException) {
                    cancelled = true;

                    this.ReportProgress(InstallerReportLevel.Primary,
                        Localizer.Instance[@"DinkInstaller/Heading/CancelledByUser"],
                        "",
                        this.ProgPhaseCurrent++ / this.ProgPhaseTotal);
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        Localizer.Instance[@"DinkInstaller/Heading/CancelledByUser"],
                    }, MyTraceLevel.Warning);
                } catch (Exception ex) {
                    exception = ex;

                    this.ReportProgress(InstallerReportLevel.Primary,
                        Localizer.Instance[@"DinkInstaller/Heading/ErrorOccured"],
                        "",
                        1.0);

                    this.CustomTrace.WriteException(MyTraceCategory.DinkInstaller, ex);
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                } finally {
                    DinkInstallerResult result;
                    
                    this.CleanUp();

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

                    this.IsBusy = false;
                    this.IsDone = true;
                    this.EndTime = DateTime.Now;

                    this.InstallerDone?.Invoke(this, exception == null
                        ? new DmodPackerDoneEventArgs(result, destinationFile, sourceDirectory)
                        : new DmodPackerDoneEventArgs(exception, destinationFile, sourceDirectory));
                }
            });

            task.Start();
        }

        private void PrepareLocations(DirectoryInfo sourceDirectory, FileInfo destinationFile) {
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/Preparing"],
                sourceDirectory.FullName,
                0.0);

            // safety checks for destination...
            if (destinationFile.Directory?.Parent == null) {
                throw new DinkInstallerFileSystemException(Localizer.Instance[@"DinkInstaller/Preparing/DestinationErrorIsRoot"] + $" \"{destinationFile.FullName}\"");
            }
            if (Path.IsPathRooted(destinationFile.Directory.FullName) == false) {
                throw new DinkInstallerFileSystemException(Localizer.Instance[@"DinkInstaller/Preparing/DestinationErrorIsNotRooted"] + $" \"{destinationFile.FullName}\"");
            }
        }

        private void CleanUp() {
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

            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/CleaningUp/Done"],
            });
        }
        

        private void PackDmodToFile_SharpCompress(
            FileInfo destinationFile,
            DirectoryInfo sourceDirectory) {
            
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/PackingDmod"],
                destinationFile.FullName,
                this.ProgPhaseCurrent++/this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/PackingDmod/Start"],
                $"    \"{sourceDirectory.FullName}\""
            });
            
            this.ReportProgress(InstallerReportLevel.Secondary,
                Localizer.Instance[@"DinkInstaller/PackingDmod/Detail"],
                "",
                0.0);

            Queue<DirectoryInfo> processingQueue = new Queue<DirectoryInfo>();
            processingQueue.Enqueue(sourceDirectory);
            List<FileInfo> fileList = new List<FileInfo>();

            
            while (processingQueue.Count > 0) {
                DirectoryInfo dir = processingQueue.Dequeue();
                FileInfo[] files = dir.GetFiles();
                DirectoryInfo[] directories = dir.GetDirectories();
                for (int i = 0; i < files.Length; i++) {
                    fileList.Add(files[i]);
                }
                for (int i = 0; i < directories.Length; i++) {
                    processingQueue.Enqueue(directories[i]);
                }
            }
            
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/PackingDmod/FoundFiles"],
                $"    {fileList.Count}..."
            });
            
            // prepare temp directory...
            DirectoryInfo theTempDir = new DirectoryInfo(Path.GetTempPath());
            DirectoryInfo tempDirInfo = theTempDir.CreateSubdirectory(Path.Combine("dink_temp", Path.GetRandomFileName()));
            this._tempDirs.Add(tempDirInfo);
            string tempTarFile = Path.Combine(tempDirInfo.FullName, sourceDirectory.Name);
            FileInfo tempTarFileInfo = new FileInfo(tempTarFile);
            this._tempFiles.Add(tempTarFileInfo);
            
            // create TAR file...
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/PackingDmod/CreatingTemporaryTar"],
                $"    \"{tempTarFile}\""
            });
            
            using (FileStream fs = new FileStream(tempTarFile, FileMode.Create, FileAccess.Write))
            using (var writer = new TarWriter(fs, new TarWriterOptions(CompressionType.None, true))) {
                for (int index = 0; index < fileList.Count; index++) {
                    if (this.CancelTokenSource.IsCancellationRequested) {
                        throw new DinkInstallerCancelledByUserException();
                    }
                    
                    FileInfo file = fileList[index];
                    string relativeFileName = Path.Combine(
                        sourceDirectory.Name,
                        Path.GetRelativePath(sourceDirectory.FullName, file.FullName));
                    
                    using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read)) {
                        writer.Write(relativeFileName, fileStream, file.LastWriteTime);
                    }
                    
                    this.ReportProgress(InstallerReportLevel.Secondary,
                        Localizer.Instance[@"DinkInstaller/PackingDmod/Detail"],
                        relativeFileName,
                        (double)index / (double)fileList.Count);
                }
            }
            
            // create BZIP2 archive...
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                Localizer.Instance[@"DinkInstaller/PackingDmod/CreatingBzip2Archive"],
                $"    \"{tempTarFile}\""
            });
            this.ReportProgress(InstallerReportLevel.Indeterminate,
                Localizer.Instance[@"DinkInstaller/PackingDmod/Detail"],
                Localizer.Instance[@"DinkInstaller/PackingDmod/CreatingBzip2Archive"],
                0.0);


            int threads = Environment.ProcessorCount;
            using (FileStream fsin = new FileStream(tempTarFile, FileMode.Open, FileAccess.Read))
            using (FileStream fsout = new FileStream(destinationFile.FullName, FileMode.Create, FileAccess.Write)) 
            using (BZip2ParallelOutputStream bzip2 = new BZip2ParallelOutputStream(fsout,threads, true, 9)) {
                fsin.CopyTo(bzip2);
                bzip2.Close();
            }
            
            
            this.ReportProgress(InstallerReportLevel.Secondary,
                Localizer.Instance[@"DinkInstaller/PackingDmod/Detail"],
                "",
                1.0);
            
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/PackingDmod"],
                destinationFile.FullName,
                this.ProgPhaseCurrent++ / this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/PackingDmod/Done"],
                $"    \"{destinationFile.FullName}...\""
            });
        }
    }
}
