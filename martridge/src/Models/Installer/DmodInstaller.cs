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

        #region 7ZIP version
        #if DMOD_INSTALLER_7ZIP

        //
        // NOTE: I used to make use of these before, but now I switched over to SharpCompress since it works on multiple platforms,
        // so they're no longer needed...
        //

        private void DecompressDmodToLocation_7zip(
            FileInfo sourceFile, 
            DirectoryInfo destinationDirectory,
            bool unzipToMemoryStream) {

            // ----------------------------------------------------------------------------------------------------
            // 7 zip - part 1 - extract bzip2 to temporary stream
            // NOTE: the original DMOD format uses a broken .tar format, corrections might be needed...


            this.ReportProgress(InstallerReportLevel.Primary,
                    Localizer.Instance[@"DinkInstaller/Heading/Temporary"],
                    sourceFile.FullName,
                    this._progPhaseCurrent++ / this._progPhaseTotal);

            this.ReportProgress(InstallerReportLevel.Indeterminate,
                    Localizer.Instance[@"DinkInstaller/InstallingDmod/TemporaryUnzip/Start"],
                    sourceFile.FullName,
                    0.0);

            this.UnzipDmodToTemp_7zip(sourceFile, unzipToMemoryStream);

            this.ReportProgress(InstallerReportLevel.Indeterminate,
                    Localizer.Instance[@"DinkInstaller/InstallingDmod/TemporaryUnzip/Done"],
                    sourceFile.FullName,
                    1.0);

            long length = this._tempStream.Length;
            if (length % 512 == 0) {
                // it's an olden badly formatted dmod tar file! needs some 0s added...
                long desiredLength = ((length / 512) * 512) + 512 * 3;
                int bytesToWrite = (int)(desiredLength - length);

                this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                    "",
                    Localizer.Instance[@"DinkInstaller/InstallingDmod/TemporaryUnzip/PaddingFile"],
                    $"    TAR Size (bytes)     = {length}",
                    $"    Padding Size (bytes) = {bytesToWrite}",
                });

                byte[] buffer = new byte[bytesToWrite];
                for (int i = 0; i < bytesToWrite; i++) {
                    buffer[i] = 0x00;
                }
                this._tempStream.Position = this._tempStream.Length;
                this._tempStream.Write(buffer, 0, buffer.Length);
            }

            // reset temp stream position...
            this._tempStream.Position = 0;

            // ----------------------------------------------------------------------------------------------------

            // ----------------------------------------------------------------------------------------------------
            // 7zip part 2 - install dmod
            //

            this.ReportProgress(InstallerReportLevel.Primary,
                    Localizer.Instance[@"DinkInstaller/Heading/InstallingDmod"],
                    sourceFile.FullName,
                    this._progPhaseCurrent++/this._progPhaseTotal);
            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/InstallingDmod/FinalUnzip/Start"],
                $"    \"{destinationDirectory.FullName}\""
            });

            this.ReportProgress(InstallerReportLevel.Indeterminate,
                Localizer.Instance[@"DinkInstaller/InstallingDmod/FinalUnzip/Detail"],
                sourceFile.FullName,
                0.0);
            
            this.UntarDmodToLocation_7zip(this._tempStream, destinationDirectory);
            
            this.ReportProgress(InstallerReportLevel.Indeterminate,
                Localizer.Instance[@"DinkInstaller/InstallingDmod/FinalUnzip/Detail"],
                sourceFile.FullName,
                1.0);
        }

        private void UntarDmodToLocation_7zip(Stream tempStream, DirectoryInfo destinationDirectory) {
            using (SevenZipExtractor.ArchiveFile archive = new SevenZipExtractor.ArchiveFile(tempStream, SevenZipExtractor.SevenZipFormat.Tar)) {
                archive.Extract(destinationDirectory.FullName, true);
            }
        }
        
        private void UnzipDmodToTemp_7zip(FileInfo dmodFile, bool unzipToStream = false) {
            string tempFileName = "<Memory>";

            if (unzipToStream == false) {
                this._tempFile = new FileInfo(Path.GetTempFileName());
                tempFileName = this._tempFile.FullName;
                this._tempStream = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite);
            } else {
                this._tempStream = new MemoryStream();
            }

            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/InstallingDmod/TemporaryUnzip/Start"],
                $"    Archive  = \"{ SevenZipExtractor.SevenZipFormat.BZip2 }\"",
                $"    Source   = \"{ dmodFile.FullName }\"",
                $"    Dest     = \"{ tempFileName }\"",
            });

            using (FileStream fs = new FileStream(dmodFile.FullName, FileMode.Open, FileAccess.Read))
                // archive should be a bzip2 archive
            using (SevenZipExtractor.ArchiveFile archive = new SevenZipExtractor.ArchiveFile(fs, SevenZipExtractor.SevenZipFormat.BZip2)) {
                if (archive.Entries.Count != 1) {
                    string msg = Localizer.Instance[@"DinkInstaller/InstallingDmod/InvalidFormatTooManyFiles"];
                    this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                        msg,
                    });

                    throw new DinkInstallerInvalidDmodFormatException(msg);
                }

                archive.Entries[0].Extract(this._tempStream);
            }


            this.CustomTrace.WriteMessage(MyTraceCategory.DinkInstaller, new List<string>() {
                "",
                Localizer.Instance[@"DinkInstaller/InstallingDmod/TemporaryUnzip/Done"],
            });
        }
        
        #endif
        #endregion
    }
}
