using Avalonia.Controls;
using Avalonia.Metadata;
using Martridge.Models;
using Martridge.Models.Dmod;
using Martridge.Models.Installer;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Martridge.ViewModels.Installer {
    public class DmodPackerViewModel : InstallerViewModelBase {

        // ------------------------------------------------------------------------------------------
        //      Source and Destination Selection 
        //

        public string? SelectedDmodSourceDirectory {
            get => this._selectedDmodSourceDirectory;
            set => this.RaiseAndSetIfChanged(ref this._selectedDmodSourceDirectory, value);
        }
        private string? _selectedDmodSourceDirectory = "";

        public string? SelectedDmodDestination {
            get => this._selectedDmodDestination;
            set => this.RaiseAndSetIfChanged(ref this._selectedDmodDestination, value);
        }
        private string? _selectedDmodDestination = "";


        // ------------------------------------------------------------------------------------------
        //      Installer logic 
        //
        
        public event EventHandler<DmodPackerDoneEventArgs>? InstallerDone;
        private DmodPackerDoneEventArgs _lastInstallerDoneEventArgs = new DmodPackerDoneEventArgs(DinkInstallerResult.Cancelled);

        public InstallerViewPage ActiveUserPage {
            get => this._activeUserPage;
            private set {
                this.RaiseAndSetIfChanged(ref this._activeUserPage, value);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Switched DmodInstallerView active page to {this.ActiveUserPage}");
            }
        }
        private InstallerViewPage _activeUserPage = InstallerViewPage.InstallSettings;

        public bool IsBusy {
            get => this._installerLogic != null;
        }   
        private DmodPacker? _installerLogic = null;


        // ------------------------------------------------------------------------------------------
        //      Constructor
        //

        public DmodPackerViewModel() {
            this.PropertyChanged += this.DmodPackerViewModel_PropertyChanged;
        }

        private void DmodPackerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            // TODO hmmm, did i need to do something here?...
        }
        

        // ------------------------------------------------------------------------------------------
        //      Commands
        //

        #region Commands

        public bool IsInstallerFinished {
            get => this._isInstallerFinished;
            private set => this.RaiseAndSetIfChanged(ref this._isInstallerFinished, value);
        }
        private bool _isInstallerFinished = false;

        public bool IsInstallerCancelled {
            get => this._isInstallerCancelled;
            private set => this.RaiseAndSetIfChanged(ref this._isInstallerCancelled, value);
        }
        private bool _isInstallerCancelled = false;

        public bool IsDmodBrowserActive {
            get => this._isDmodBrowserActive;
            private set => this.RaiseAndSetIfChanged(ref this._isDmodBrowserActive, value);
        }
        private bool _isDmodBrowserActive = false;


        public void CmdFinish(object? parameter = null) {
            this.ResetInstallerStateAndFireDoneEvent();
        }
        [DependsOn(nameof(IsInstallerFinished))]
        public bool CanCmdFinish(object? parameter = null) {

            return this.IsInstallerFinished;
        }

        public void CmdCancel(object? parameter = null) {
            if (this._installerLogic == null) return;
            this.IsInstallerCancelled = true;
            this._installerLogic.CancelTokenSource.Cancel();
        }
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(IsInstallerCancelled))]
        [DependsOn(nameof(IsInstallerFinished))]
        public bool CanCmdCancel(object? parameter = null) {
            // can't cancel if the installer is null...
            if (this.IsBusy == false) { return false; }
            // can't cancel if already done
            if (this.IsInstallerFinished) { return false; }
            // can only cancel once
            return !this.IsInstallerCancelled;
        }


        public void CmdGoBack(object? parameter = null) {
            if (this.ActiveUserPage == InstallerViewPage.InstallSettings) {
                this._lastInstallerDoneEventArgs = new DmodPackerDoneEventArgs(DinkInstallerResult.Cancelled);
                this.ResetInstallerStateAndFireDoneEvent();
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        public bool CanCmdGoBack(object? parameter = null) {
            switch (this.ActiveUserPage) {
                case InstallerViewPage.InstallSettings: return true;
                case InstallerViewPage.InstallProgress: return false;
                default: return false;
            }
        }

        public void CmdGoNext(object? parameter = null) {
            switch (this.ActiveUserPage) {
                case InstallerViewPage.InstallSettings:
                    this.StartInstallation();
                    break;
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(SelectedDmodDestination))]
        [DependsOn(nameof(SelectedDmodSourceDirectory))]
        public bool CanCmdGoNext(object? parameter = null) {
            switch (this.ActiveUserPage) {
                case InstallerViewPage.InstallSettings:
                    if (this.ParentWindow == null) return false;
                    if (string.IsNullOrEmpty(this.SelectedDmodDestination)) return false;
                    if (string.IsNullOrEmpty(this.SelectedDmodSourceDirectory)) return false;
                    if (Directory.Exists(this.SelectedDmodSourceDirectory) == false) return false;
                    return true;
                case InstallerViewPage.InstallProgress: return false;
                default: return false;
            }
        }

        public void CmdBrowseDmodSource(object? parameter = null) {
            this.BrowseDmodSource_Internal();
        }

        [DependsOn(nameof(IsDmodBrowserActive))]
        [DependsOn(nameof(ParentWindow))]
        public bool CanCmdBrowseDmodSource(object? parameter = null) {
            return !this.IsDmodBrowserActive && this.ParentWindow != null;
        }
        
        public void CmdBrowseDmodDestination(object? parameter = null) {
            this.BrowseDmodDestination_Internal();
        }

        [DependsOn(nameof(IsDmodBrowserActive))]
        [DependsOn(nameof(ParentWindow))]
        public bool CanCmdBrowseDmodDestination(object? parameter = null) {
            return !this.IsDmodBrowserActive && this.ParentWindow != null;
        }

        #endregion


        private async void BrowseDmodSource_Internal() {
            if (this.ParentWindow == null) return;

            try {
                this.IsDmodBrowserActive = true;

                OpenFolderDialog ofd = new OpenFolderDialog();
                ofd.Directory = LocationHelper.AppBaseDirectory;

                string? result = await ofd.ShowAsync(this.ParentWindow);
                if (result != null) {
                    // check if this is a valid DMOD directory
                    DmodFileDefinition dfd = new DmodFileDefinition(result);
                    if (dfd.IsCorrectlyDefined) {
                        this.SelectedDmodSourceDirectory = result;
                    } else {
                        string title = Localizer.Instance[@"DinkInstallerView/MessageBox_DmodPackerError_BadFolder_Title"];
                        string message = Localizer.Instance[@"DinkInstallerView/MessageBox_DmodPackerError_BadFolder_Body"];
                        await DinkyAlert.ShowDialog(title, message, AlertResults.Ok, AlertType.Error, this.ParentWindow);
                    }
                }

            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            } finally {
                this.IsDmodBrowserActive = false;                
            }
        }
        
        private async void BrowseDmodDestination_Internal() {
            if (this.ParentWindow == null) return;

            try {
                this.IsDmodBrowserActive = true;

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Directory = LocationHelper.AppBaseDirectory;
                sfd.Filters = new List<FileDialogFilter>() {
                    new FileDialogFilter() {
                        Name = Localizer.Instance[@"Generic/FileTypeDmod"],
                        Extensions = new List<string>() {
                            "dmod",
                        },
                    },
                };

                string? result = await sfd.ShowAsync(this.ParentWindow);
                if (result != null) {
                    this.SelectedDmodDestination = result;
                }

            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            } finally
            {
                this.IsDmodBrowserActive = false;                
            }
        }

        private void StartInstallation() {
            if (this.SelectedDmodDestination == null ||
                this.SelectedDmodSourceDirectory == null) {
                // TODO throw some exception or something
                return;
            }

            if (this._installerLogic == null) {
                try {
                    DirectoryInfo source = new DirectoryInfo(this.SelectedDmodSourceDirectory);
                    FileInfo destination = new FileInfo(this.SelectedDmodDestination);

                    // create installer trace listener
                    this.InstallerTraceListener = new MyTraceListenerGui("Installer Trace Listener");
                    this.InstallerTraceListener.ShowLevels = false;
                    this.InstallerTraceListener.Levels = MyTraceLevel.Critical | MyTraceLevel.Error | MyTraceLevel.Warning | MyTraceLevel.Information;
                    this.InstallerTraceListener.PropertyChanged += ( sender,  args) =>  {
                        this.RaisePropertyChanged(nameof(this.InstallerProgressLog));
                        this.InstallerProgressLogCaretIndex = int.MaxValue;
                    };

                    // create installer logic
                    this._installerLogic = new DmodPacker();
                    this._installerLogic.CustomTrace.Listeners.Add(this.InstallerTraceListener);
                    this._installerLogic.ProgressReport += this.InstallerOnProgressReport;
                    this._installerLogic.InstallerDone += this.InstallerOnDone;

                    this.RaisePropertyChanged(nameof(this.IsBusy));

                    this.InstallerProgressTitle = source.Name;
                    this.ActiveUserPage = InstallerViewPage.InstallProgress;

                    //this._InstallerLogic.IsDebugMode = true;
                    this._installerLogic.StartPackingDmod(destination, source);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            }
        }

        private void ResetInstallerStateAndFireDoneEvent() {
            this.ActiveUserPage = InstallerViewPage.InstallSettings;
            this.IsInstallerFinished = false;
            this.IsInstallerCancelled = false;

            this._installerLogic?.CustomTrace.Flush();
            this._installerLogic?.CustomTrace.Close();
            this._installerLogic = null;
            this.RaisePropertyChanged(nameof(this.IsBusy));

            this.InstallerTraceListener?.Close();
            this.InstallerTraceListener = null;

            this.InstallerDone?.Invoke(this, this._lastInstallerDoneEventArgs);
        }
        
        private void InstallerOnDone(object? sender, DmodPackerDoneEventArgs args) {
            this.IsInstallerFinished = true;
            this._lastInstallerDoneEventArgs = args;
            if (args.Result == DinkInstallerResult.Cancelled) this.ShowInstallerCancelledMessageBox();
            if (args.Result == DinkInstallerResult.Error) this.ShowInstallerErrorMessageBox(args.Exception);
        }

        private void InstallerOnProgressReport(object? sender, InstallerProgressEventArgs args) {
            try {
                if (args.ProgressLevel == InstallerReportLevel.Primary) {
                    this.InstallerProgressLevel0MainTitle = args.HeadingMain;
                    this.InstallerProgressLevel0SubTitle = args.HeadingSecondary;
                    this.InstallerProgressLevel0Progress = args.ProgressPercent;
                }
                if (args.ProgressLevel == InstallerReportLevel.Secondary) {
                    if (Math.Abs(args.ProgressPercent - 1.0) < 0.00001) {
                        this.InstallerProgressLevel1IsVisibile = false;
                    } else {
                        this.InstallerProgressLevel1IsVisibile = true;
                    }

                    this.InstallerProgressLevel1Indeterminate = false;
                    this.InstallerProgressLevel1MainTitle = args.HeadingMain;
                    this.InstallerProgressLevel1SubTitle = args.HeadingSecondary;
                    this.InstallerProgressLevel1Progress = args.ProgressPercent;
                }
                if (args.ProgressLevel == InstallerReportLevel.Indeterminate) {
                    if (Math.Abs(args.ProgressPercent - 1.0) < 0.00001) {
                        this.InstallerProgressLevel1IsVisibile = false;
                    } else {
                        this.InstallerProgressLevel1IsVisibile = true;
                    }
                    this.InstallerProgressLevel1Indeterminate = true;
                    this.InstallerProgressLevel1MainTitle = args.HeadingMain;
                    this.InstallerProgressLevel1SubTitle = args.HeadingSecondary;
                    this.InstallerProgressLevel1Progress = args.ProgressPercent;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        
    }
}

