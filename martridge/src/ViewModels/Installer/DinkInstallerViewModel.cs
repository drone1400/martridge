using System;
using System.Collections.ObjectModel;
using System.IO;
using ReactiveUI;
using Avalonia.Metadata;
using Avalonia.Controls;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Installer;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using System.Threading;

namespace Martridge.ViewModels.Installer {
    public class DinkInstallerViewModel : InstallerViewModelBase {

        // ------------------------------------------------------------------------------------------
        //      Version Selection 
        //

        public ObservableCollection<ConfigInstaller> InstallableVersions { get; } = new ObservableCollection<ConfigInstaller>();

        public int SelectedInstallableVersionIndex {
            get => this._selectedInstallableVersionIndex;
            set => this.RaiseAndSetIfChanged(ref this._selectedInstallableVersionIndex, value);
        }
        private int _selectedInstallableVersionIndex = 0;

        public ConfigInstaller? SelectedInstallableVersion {
            get => this._selectedInstallableVersion;
            set => this.RaiseAndSetIfChanged(ref this._selectedInstallableVersion, value);
        }
        private ConfigInstaller? _selectedInstallableVersion = null;


        // ------------------------------------------------------------------------------------------
        //      Installer logic
        //

        public event EventHandler<DinkInstallerDoneEventArgs>? InstallerDone;
        private DinkInstallerDoneEventArgs _lastInstallerDoneEventArgs = new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
        
        public InstallerViewPage ActiveUserPage {
            get => this._activeUserPage;
            private set {
                this.RaiseAndSetIfChanged(ref this._activeUserPage, value);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Switched DinkInstallerView active page to {this.ActiveUserPage}");
            }
        }
        private InstallerViewPage _activeUserPage = InstallerViewPage.VersionSelect;

        
#if PLATF_WINDOWS
        public bool DinkInstallerNotSupported {
            get => false;
        }
#else
        public bool DinkInstallerNotSupported  {
            get => true;
        }
#endif
        

        public bool IsBusy {
            get => this._installerLogic != null;
        }
        
        private DinkInstaller? _installerLogic = null;

        // ------------------------------------------------------------------------------------------
        //      Pre-Install Info... 
        //
        public string InstallerDestination {
            get => this._installerDestination;
            private set =>
                // TODO... validate path...
                this.RaiseAndSetIfChanged(ref this._installerDestination, value);
        }
        private string _installerDestination = "";
        private string _installerDestinationAuto = "";

        // ------------------------------------------------------------------------------------------
        //      Constructor 
        //
        public DinkInstallerViewModel() {
            string configFileInstaller = Path.Combine(LocationHelper.AppBaseDirectory, "config", "configInstallerList.json");
            ConfigInstallerList? cfgInst = ConfigInstallerList.LoadFromFile(configFileInstaller);
            if (cfgInst == null) {
                cfgInst = ConfigInstallerList.GetDefaultInstallers();

                cfgInst.SaveToFile(configFileInstaller);
            }

            foreach (ConfigInstaller inst in cfgInst.InstallableVersions) {
                this.InstallableVersions.Add(inst);
            }

            //this.SelectedInstallableVersionIndex = 0;
            this.PropertyChanged += this.DinkInstallerViewModel_PropertyChanged;
            this.SelectedInstallableVersion = this.InstallableVersions[0];
        }

        private void DinkInstallerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.SelectedInstallableVersion)) {
                this._installerDestinationAuto = Path.Combine(LocationHelper.AppBaseDirectory, this.SelectedInstallableVersion.InstallerName);
                this.InstallerDestination = this._installerDestinationAuto;
            }
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

        public bool IsDestinationBrowserActive {
            get => this._isDestinationBrowserActive;
            private set => this.RaiseAndSetIfChanged(ref this._isDestinationBrowserActive, value);
        }
        private bool _isDestinationBrowserActive = false;


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
            if (this.ActiveUserPage == InstallerViewPage.VersionSelect || 
                this.DinkInstallerNotSupported) {
                this._lastInstallerDoneEventArgs = new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
                this.ResetInstallerStateAndFireDoneEvent();
            } else if (this.ActiveUserPage == InstallerViewPage.InstallSettings) {
                this.ActiveUserPage = InstallerViewPage.VersionSelect;
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        [DependsOn(nameof(SelectedInstallableVersion))]
        public bool CanCmdGoBack(object? parameter = null) {
            if (this.DinkInstallerNotSupported) {
                return true;
            }
            switch (this.ActiveUserPage) {
                case InstallerViewPage.VersionSelect: return true;
                case InstallerViewPage.InstallSettings: return true;
                case InstallerViewPage.InstallProgress: return false;
                default: return false;
            }
        }

        public void CmdGoNext(object? parameter = null) {
            switch (this.ActiveUserPage) {
                case InstallerViewPage.VersionSelect:
                    this.ActiveUserPage = InstallerViewPage.InstallSettings;
                    this.InstallerProgressTitle = this.SelectedInstallableVersion.InstallerName;
                    break;
                case InstallerViewPage.InstallSettings:
                    this.StartInstallation();
                    break;
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(SelectedInstallableVersion))]
        public bool CanCmdGoNext(object? parameter = null) {
            if (this.DinkInstallerNotSupported) return false;
            switch (this.ActiveUserPage) {
                case InstallerViewPage.VersionSelect: return this.SelectedInstallableVersion != null && this.ParentWindow != null;
                case InstallerViewPage.InstallSettings: return this.SelectedInstallableVersion != null && this.ParentWindow != null;
                case InstallerViewPage.InstallProgress: return false;
                default: return false;
            }
        }

        public void CmdBrowseDestination(object? parameter = null) {
            this.BrowseDestination_Internal();
        }

        [DependsOn(nameof(IsDestinationBrowserActive))]
        [DependsOn(nameof(ParentWindow))]
        public bool CanCmdBrowseDestination(object? parameter = null) {
            return !this.IsDestinationBrowserActive && this.ParentWindow != null;
        }

        private async void BrowseDestination_Internal() {
            if (this.ParentWindow == null || this.SelectedInstallableVersion == null) return;
            
            try {
                this.IsDestinationBrowserActive = true;

                OpenFolderDialog ofd = new OpenFolderDialog();
                ofd.Directory = LocationHelper.AppBaseDirectory;

                if (this._installerDestinationAuto == this._installerDestination) {
                    DirectoryInfo dirInfo = new DirectoryInfo(this._installerDestination);
                    if (dirInfo.Parent?.Exists == true) {
                        ofd.Directory = dirInfo.Parent.FullName;
                    }
                } else {
                    DirectoryInfo dirInfo = new DirectoryInfo(this._installerDestination);
                    if (dirInfo.Exists) {
                        ofd.Directory = dirInfo.FullName;
                    }
                }


                string? result = await ofd.ShowAsync(this.ParentWindow);
                if (result != null) {
                    DirectoryInfo dirInfo = new DirectoryInfo(result);
                    if (dirInfo.Exists && dirInfo.Parent != null) {
                        this._installerDestinationAuto = Path.Combine(dirInfo.FullName, this.SelectedInstallableVersion.InstallerName);
                        this.InstallerDestination = this._installerDestinationAuto;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            } finally {
                this.IsDestinationBrowserActive = false;
            }
        }
        
        #endregion
        
        // ------------------------------------------------------------------------------------------
        //      Installer logic
        //

        private async void StartInstallation() {
            if (this.SelectedInstallableVersion == null) {
                // TODO throw some exception or something
                return;
            }

            if (this._installerLogic == null) {
                try {
                    DirectoryInfo destination = new DirectoryInfo(this.InstallerDestination);
                    bool removeOldFiles = false;

                    // Check if destination already exists
                    if (destination.Exists && (destination.GetDirectories().Length > 0 ||
                            destination.GetFiles().Length > 0)) {

                        string title = Localizer.Instance[@"DinkInstallerView/MessageBox_CleanInstallConfirm_Title"];
                        string body = Localizer.Instance[@"DinkInstallerView/MessageBox_CleanInstallConfirm_Body"];

                        // make sure the correct new line characters are used
                        body = body.Replace("\n\r", Environment.NewLine);
                        
                        var result = await DinkyAlert.ShowDialog(title, body, AlertResults.Yes | AlertResults.No | AlertResults.Cancel, AlertType.Warning, this.ParentWindow);
                        if (result == AlertResults.Cancel ||
                            result == AlertResults.Abort) {
                            return;
                        }
                        if (result == AlertResults.Yes) {
                            removeOldFiles = true;
                        }
                    }

                    // create installer trace listener
                    this.InstallerTraceListener = new MyTraceListenerGui("Installer Trace Listener");
                    this.InstallerTraceListener.ShowLevels = false;
                    this.InstallerTraceListener.Levels = MyTraceLevel.Critical | MyTraceLevel.Error | MyTraceLevel.Warning | MyTraceLevel.Information;
                    this.InstallerTraceListener.PropertyChanged += ( sender,  args) => {
                        this.RaisePropertyChanged(nameof(this.InstallerProgressLog));
                        this.InstallerProgressLogCaretIndex = int.MaxValue;
                    };

                    // create installer logic
                    this._installerLogic = new DinkInstaller();
                    this._installerLogic.CustomTrace.Listeners.Add(this.InstallerTraceListener);
                    this._installerLogic.ProgressReport += this.InstallerOnProgressReport;
                    this._installerLogic.InstallerDone += this.InstallerOnDone;
                    
                    // gui updates
                    this.ActiveUserPage = InstallerViewPage.InstallProgress;
                    this.RaisePropertyChanged(nameof(this.IsBusy));
                    
                    // start installation
                    this._installerLogic.StartInstallingDink(destination, removeOldFiles,  this.SelectedInstallableVersion);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            }
        }
        private void ResetInstallerStateAndFireDoneEvent() {
            this.ActiveUserPage = InstallerViewPage.VersionSelect;
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

        private void InstallerOnDone(object? sender, DinkInstallerDoneEventArgs args) {
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
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
    }
}
