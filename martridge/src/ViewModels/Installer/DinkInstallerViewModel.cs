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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Martridge.ViewModels.Installer {
    public class DinkInstallerViewModel : InstallerViewModelBase {

        // ------------------------------------------------------------------------------------------
        //      Installable Selection 
        //

        public ConfigInstallerList InstallableDefinitions {
            get => _installableDefinitions;
            set {
                this.RaiseAndSetIfChanged(ref this._installableDefinitions, value);
                this.RefreshInstallableCategories();
            }
        }
        private ConfigInstallerList _installableDefinitions = new ConfigInstallerList();

        public ObservableCollection<string>  InstallableCategories {
            get => this._installableCategories;
            private set => this.RaiseAndSetIfChanged(ref this._installableCategories, value);
        }
        private ObservableCollection<string>  _installableCategories = new ObservableCollection<string>();

        public string? SelectedInstallableCategory {
            get => this._selectedInstallableCategory;
            set {
                this.RaiseAndSetIfChanged(ref this._selectedInstallableCategory, value);
                this.RefreshInstallableNames();
            }
        }
        private string? _selectedInstallableCategory = null;

        public ObservableCollection<string>  InstallableNames {
            get => this._installableNames;
            private set => this.RaiseAndSetIfChanged(ref this._installableNames, value);
        }
        private ObservableCollection<string>  _installableNames = new ObservableCollection<string>();

        public string? SelectedInstallableName {
            get => this._selectedInstallableName;
            set {
                this.RaiseAndSetIfChanged(ref this._selectedInstallableName, value);
                this.RefreshInstallable();
            }
        }
        private string? _selectedInstallableName = null;

        public ConfigInstaller? SelectedInstallable {
            get => this._selectedInstallable;
            set => this.RaiseAndSetIfChanged(ref this._selectedInstallable, value);
        }

        private ConfigInstaller? _selectedInstallable = null;


        // ------------------------------------------------------------------------------------------
        //      Installer logic
        //

        public event EventHandler<DinkInstallerDoneEventArgs>? InstallerDone;
        private DinkInstallerDoneEventArgs _lastInstallerDoneEventArgs = new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
        
        private DinkInstaller? _installerLogic = null;
        
#if PLATF_WINDOWS
        public bool DinkInstallerNotSupported {
            get => false;
        }
#else
        public bool DinkInstallerNotSupported  {
            get => true;
        }
#endif

        

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
            //this.SelectedInstallableVersionIndex = 0;
            this.PropertyChanged += this.DinkInstallerViewModel_PropertyChanged;
        }

        private void DinkInstallerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.SelectedInstallable)) {
                this._installerDestinationAuto = Path.Combine(LocationHelper.AppBaseDirectory, this.SelectedInstallable?.Name ?? "ERR_InstallerName");
                this.InstallerDestination = this._installerDestinationAuto;
            }
        }

        public void InitializeInstallerList(bool updateDefaultsIfFileExists) {
            string configFileInstaller = Path.Combine(LocationHelper.AppBaseDirectory, "config", "configInstallerList.json");
            ConfigInstallerList? cfgInst = ConfigInstallerList.LoadFromFile(configFileInstaller);
            if (cfgInst == null || cfgInst.Installables.Count == 0) {
                cfgInst = new ConfigInstallerList();
                cfgInst.AddMissingDefaults();
                cfgInst.SaveToFile(configFileInstaller);
            } else if (updateDefaultsIfFileExists) {
                if (cfgInst.AddMissingDefaults()) {
                    cfgInst.SaveToFile(configFileInstaller);
                }
            }

            this.InstallableDefinitions = cfgInst;
        }

        private void RefreshInstallableCategories() {
            this.InstallableCategories.Clear();
            this.SelectedInstallableCategory = null;
            foreach (var kvp in this.InstallableDefinitions.Installables) {
                this.InstallableCategories.Add(kvp.Key);
            }
            if (this.InstallableCategories.Count > 0) {
                this.SelectedInstallableCategory = this.InstallableCategories.First();
            }
        }

        private void RefreshInstallableNames() {
            this.InstallableNames.Clear();
            this.SelectedInstallableName = null;
            if (this.SelectedInstallableCategory == null) return;

            if (this.InstallableDefinitions.Installables.ContainsKey(this.SelectedInstallableCategory)) {
                foreach (var kvp in this.InstallableDefinitions.Installables[this.SelectedInstallableCategory]) {
                    this.InstallableNames.Add(kvp.Key);
                }
                
                if (this.InstallableNames.Count > 0) {
                    this.SelectedInstallableName = this.InstallableNames.First();
                }
            }
        }

        private void RefreshInstallable() {
            this.SelectedInstallable = null;
            this.InstallerProgressTitle = this.SelectedInstallable?.Name ?? "";
            if (this.SelectedInstallableCategory != null &&
                this.SelectedInstallableName != null &&
                this.InstallableDefinitions.Installables.ContainsKey(this.SelectedInstallableCategory) &&
                this.InstallableDefinitions.Installables[this.SelectedInstallableCategory].ContainsKey(this.SelectedInstallableName)
                ) {
                this.SelectedInstallable = this.InstallableDefinitions.Installables[this.SelectedInstallableCategory][this.SelectedInstallableName];
                this.InstallerProgressTitle = this.SelectedInstallable.Name;
            }
            
        }


        // ------------------------------------------------------------------------------------------
        //      Commands
        //

        #region Commands

        public override void CmdExit(object? parameter = null) {
            if (this.IsInstallerStarted == false || this.DinkInstallerNotSupported) {
                this._lastInstallerDoneEventArgs = new DinkInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
                this.ResetInstallerStateAndFireDoneEvent();
            }
            if (this.IsInstallerFinished) {
                this.ResetInstallerStateAndFireDoneEvent();
            }
        }
        
        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(IsInstallerCancelled))]
        [DependsOn(nameof(IsInstallerFinished))]
        public bool CanCmdExit(object? parameter = null) {
            if (this.IsInstallerCancelled) return false;
            if (this.IsInstallerStarted) return this.IsInstallerFinished;
            
            return true;
        }

        public override void CmdCancel(object? parameter = null) {
            if (this.CanCmdCancel() == false) return;
            
            this.IsInstallerCancelled = true;
            // null check done in CanCmdCancel
            this._installerLogic!.CancelTokenSource.Cancel();
        }
        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(IsInstallerCancelled))]
        [DependsOn(nameof(IsInstallerFinished))]
        public bool CanCmdCancel(object? parameter = null) {
            if (this._installerLogic == null) return false;
            // can't cancel if the installer is not started
            if (this.IsInstallerStarted == false) return false;
            // can't cancel if already done
            if (this.IsInstallerFinished) return false;
            // can only cancel once
            return !this.IsInstallerCancelled;
        }
        
        public async override void CmdStartInstall(object? parameter = null) {
            if (this.CanCmdStartInstall() == false) return;
            
            await this.StartInstallation();
        }
        
        [DependsOn(nameof(DinkInstallerNotSupported))]
        [DependsOn(nameof(IsInstallerStarted))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(SelectedInstallable))]
        public bool CanCmdStartInstall(object? parameter = null) {
            if (this.DinkInstallerNotSupported) return false;
            if (this.IsInstallerStarted) return false;
            if (this.ParentWindow == null) return false;
            if (this.SelectedInstallable == null) return false;
            if (this._installerLogic != null) return false;

            return true;
        }

        public void CmdBrowseDestination(object? parameter = null) {
            this.BrowseDestination_Internal();
        }

        [DependsOn(nameof(IsFileBrowserActive))]
        [DependsOn(nameof(ParentWindow))]
        public bool CanCmdBrowseDestination(object? parameter = null) {
            return !this.IsFileBrowserActive && this.ParentWindow != null;
        }

        private async void BrowseDestination_Internal() {
            if (this.ParentWindow == null || this.SelectedInstallable == null || this.IsFileBrowserActive) return;
            
            try {
                this.IsFileBrowserActive = true;

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
                        this._installerDestinationAuto = Path.Combine(dirInfo.FullName, this.SelectedInstallable!.Name);
                        this.InstallerDestination = this._installerDestinationAuto;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            } finally {
                this.IsFileBrowserActive = false;
            }
        }
        
        #endregion
        
        // ------------------------------------------------------------------------------------------
        //      Installer logic
        //

        private async Task StartInstallation() {
            if (this.CanCmdStartInstall() == false) return;

            try {
                this.IsInstallerStarted = true;
                
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
                    if (result == AlertResults.Cancel) {
                        return;
                    }
                    if (result == AlertResults.Yes) {
                        title = Localizer.Instance[@"DinkInstallerView/MessageBox_CleanInstallConfirm_DeleteDoubleConfirm_Title"];
                        body = Localizer.Instance[@"DinkInstallerView/MessageBox_CleanInstallConfirm_DeleteDoubleConfirm_Body"];
                        body += Environment.NewLine;
                        body += destination.FullName;
                        result = await DinkyAlert.ShowDialog(title, body, AlertResults.Yes | AlertResults.Cancel, AlertType.Warning, this.ParentWindow);
                        if (result == AlertResults.Yes) {
                            removeOldFiles = true;
                        } else {
                            return;
                        }
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

                // start installation
                this._installerLogic.StartInstallingDink(destination, removeOldFiles,  this.SelectedInstallable);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
        private void ResetInstallerStateAndFireDoneEvent() {
            this._installerLogic?.CustomTrace.Flush();
            this._installerLogic?.CustomTrace.Close();
            this._installerLogic = null;

            this.InstallerTraceListener?.Close();
            this.InstallerTraceListener = null;
            
            this.IsInstallerStarted = false;
            this.IsInstallerFinished = false;
            this.IsInstallerCancelled = false;

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
