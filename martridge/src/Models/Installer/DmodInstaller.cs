using Martridge.Models.Localization;
using Martridge.Trace;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Martridge.Models.Installer {
    public class DmodInstaller : InstallerBase {
        public event EventHandler<DmodInstallerDoneEventArgs>? InstallerDone;

        private readonly Stream? _tempStream = null;
        private readonly FileInfo? _tempFile = null;

        public void StartInstallingDmod(FileInfo sourceFile, DirectoryInfo destinationDirectory) {
            // only allow installer to run once
            if (this.IsBusy || this.IsDone) return;
            
            this.IsBusy = true;

            Task task = new Task( () => {
                bool cancelled = false;
                Exception? exception = null;

                try {
                    // starting...
                    this.StartTime = DateTime.Now;
                    this.ProgPhaseCurrent = 0;
                    this.ProgPhaseTotal = 5;
                    // log start of installation
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        "",
                        Localizer.Instance[@"DinkInstaller/StartInstalling"],
                        $"    \"{sourceFile.FullName}\"",
                        $"    \"{destinationDirectory.FullName}\"",
                    });
                    // preparations
                    this.PrepareLocations(sourceFile, destinationDirectory);
                    // decompress dmod
                    this.DecompressDmodToLocation_SharpCompress(sourceFile, destinationDirectory);
                    // all done!
                } catch (DinkInstallerCancelledByUserException) {
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

                    this.IsDone = true;
                    this.IsBusy = false;
                    this.EndTime = DateTime.Now;

                    this.InstallerDone?.Invoke(this, exception == null
                        ? new DmodInstallerDoneEventArgs(result, sourceFile, destinationDirectory)
                        : new DmodInstallerDoneEventArgs(exception, sourceFile, destinationDirectory));
                }
            });

            task.Start();
        }
        
        private void PrepareLocations(FileInfo sourceFile, DirectoryInfo destinationDirectory) {
            // start of phase 1
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/InstallingDmod/Preparing"],
                sourceFile.FullName,
                0.0);

            // safety checks for destination...
            if (destinationDirectory.Parent == null) {
                throw new DinkInstallerFileSystemException(Localizer.Instance[@"DinkInstaller/Preparing/DestinationErrorIsRoot"] + $" \"{destinationDirectory.FullName}\"");
            }
            if (Path.IsPathRooted(destinationDirectory.FullName) == false) {
                throw new DinkInstallerFileSystemException(Localizer.Instance[@"DinkInstaller/Preparing/DestinationErrorIsNotRooted"] + $" \"{destinationDirectory.FullName}\"");
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

            this._tempStream?.Close();
            this._tempStream?.Dispose();

            if (this._tempFile != null) {
                this._tempFile.Refresh();
                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    $"    {this._tempFile.FullName}",
                });
                this._tempFile.Delete();
            }

            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/CleaningUp/Done"],
            });
        }
        
        private void DecompressDmodToLocation_SharpCompress(
            FileInfo sourceFile,
            DirectoryInfo destinationDirectory) {
            
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/InstallingDmod"],
                sourceFile.FullName,
                this.ProgPhaseCurrent++/this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/InstallingDmod/FinalUnzip/Start"],
                $"    \"{destinationDirectory.FullName}\""
            });
            
            this.ReportProgress(InstallerReportLevel.Indeterminate,
                Localizer.Instance[@"DinkInstaller/InstallingDmod/FinalUnzip/Detail"],
                "",
                0.0);
            using (FileStream fs = new FileStream(sourceFile.FullName, FileMode.Open, FileAccess.Read))
            using (var reader = ReaderFactory.Open(fs)) {
                while (reader.MoveToNextEntry()) {
                    if (this.CancelTokenSource.IsCancellationRequested) {
                        throw new DinkInstallerCancelledByUserException();
                    }
                    
                    // archive entries are split using '/' 
                    string[] split = reader.Entry.Key.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    // recreate path using system specific path separator
                    string relativePath = Path.Combine(split);
                    string fullPath = Path.Combine(destinationDirectory.FullName, relativePath);
                    
                    // NOTE: reporting progress here slows down extraction greatly! there's no point in doing it...
                    // this.ReportProgress(InstallerReportLevel.Indeterminate,
                    //     Localizer.Instance[@"DinkInstaller/InstallingDmod/FinalUnzip/Detail"],
                    //     relativePath,
                    //     0.0);
                    
                    FileInfo finfo = new FileInfo(fullPath);
                    if (reader.Entry.IsDirectory == false) {
                        if (finfo.Directory?.Exists == false) {
                            finfo.Directory.Create();
                        }
                        reader.WriteEntryToFile(fullPath);
                    }
                }
            }
            this.ReportProgress(InstallerReportLevel.Indeterminate,
                Localizer.Instance[@"DinkInstaller/InstallingDmod/FinalUnzip/Detail"],
                "",
                1.0);
            
            this.ReportProgress(InstallerReportLevel.Primary,
                Localizer.Instance[@"DinkInstaller/Heading/InstallingDmod"],
                sourceFile.FullName,
                this.ProgPhaseCurrent++ / this.ProgPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/InstallingDmod/FinalUnzip/Done"],
                $"    \"{destinationDirectory.FullName}...\""
            });
        }
    }
}
