using Avalonia.Metadata;
using Martridge.Models.Configuration;
using Martridge.Models.Installer;
using Martridge.Models.Localization;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Martridge.ViewModels.DinkyAlerts;

namespace Martridge.ViewModels.Installer {
    
    public class DmodInstallerViewModel : InstallerViewModelBase {

        public string SelectedDmodSource {
            get => this._selectedDmodSource;
            set => this.RaiseAndSetIfChanged( ref this._selectedDmodSource, value);
        }
        private string _selectedDmodSource = "";

        public ObservableCollection<DirectoryInfo> InstallableDestinations { get; } = new ObservableCollection<DirectoryInfo>();

        public DirectoryInfo? SelectedInstallableDestination {
            get => this._selectedInstallableDestination;
            set => this.RaiseAndSetIfChanged(ref this._selectedInstallableDestination, value);
        }
        private DirectoryInfo? _selectedInstallableDestination = null;

        public string DmodDirectoryOverride {
            get => this._dmodDirectoryOverride;
            set => this.RaiseAndSetIfChanged(ref this._dmodDirectoryOverride, value);
        }
        private string _dmodDirectoryOverride = "";

        public DmodInstallPhase InstallPhase {
            get => this._installPhase;
            private set => this.RaiseAndSetIfChanged( ref this._installPhase, value);
        }
        private DmodInstallPhase _installPhase = DmodInstallPhase.Inactive;

        public bool IsFileBrowserActive {
            get => this._isFileBrowserActive;
            private set => this.RaiseAndSetIfChanged(ref this._isFileBrowserActive, value);
        }
        private bool _isFileBrowserActive = false;

        // ------------------------------------------------------------------------------------------
        //      Installer logic 
        //
        
        public event EventHandler<DmodInstallerDoneEventArgs>? InstallerDone;
        private DmodInstallerDoneEventArgs _installerDoneEventArgs = new DmodInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
        
        private ConfigGeneral? _configGeneral = null;
        private DmodInstaller? _installerLogic = null;


        // ------------------------------------------------------------------------------------------
        //      Constructor
        //
        
        public DmodInstallerViewModel() {
            this.ResetInstallerStateAndFireDoneEvent(false);
        }

        public void InitializeConfiguration(ConfigGeneral cfg) {
            if (this._configGeneral != null) {
                this._configGeneral.Updated -= this.GeneralUpdated;
            }

            this._configGeneral = cfg;
            if (this._configGeneral != null) {
                this._configGeneral.Updated += this.GeneralUpdated;
                this.InitializeDmodLocations();
            }
        }

        private void GeneralUpdated(object? sender, EventArgs e) {
            this.InitializeDmodLocations();
        }

        private void InitializeDmodLocations() {
            if (this._configGeneral == null) return;
            this.InstallableDestinations.Clear();
            List<DirectoryInfo> dmodPlaces = this._configGeneral.GetRealDmodDirectories();

            foreach (DirectoryInfo dirInfo in dmodPlaces) {
                this.InstallableDestinations.Add(dirInfo);
            }
            
            this.SelectedInstallableDestination = dmodPlaces.First();
        }

        // ------------------------------------------------------------------------------------------
        //      Commands
        //

        #region Commands

        public async void CmdInitializeDmod(object? parameter = null)
        {
            if (this.InstallPhase != DmodInstallPhase.Inactive) return;
            if (parameter is not string dmodPath) return;
            
            FileInfo fileInfo = new FileInfo(dmodPath);
            if (fileInfo.Exists == false) return;
            
            this.SelectedDmodSource = fileInfo.FullName;
            await this.StartInitializingDmod(fileInfo);
        }
        [DependsOn(nameof(InstallPhase))]
        public bool CanCmdInitializeDmod(object? parameter = null)
        {
            return this.InstallPhase == DmodInstallPhase.Inactive && parameter is string path && File.Exists(path);
        }

        public void CmdFinish(object? parameter = null)
        {
            if (this.InstallPhase != DmodInstallPhase.Finished)
                return;
            
            this.ResetInstallerStateAndFireDoneEvent(true);
        }
        [DependsOn(nameof(InstallPhase))]
        public bool CanCmdFinish(object? parameter = null)
        {
            return this.InstallPhase == DmodInstallPhase.Finished;
        }

        public void CmdCancel(object? parameter = null) {
            if (this.CanCmdCancel() == false) return;

            if (this._installerLogic == null)
            {
                this._installerDoneEventArgs = new DmodInstallerDoneEventArgs(DinkInstallerResult.Cancelled);
                this.ResetInstallerStateAndFireDoneEvent(true);
            }
            else
            {
                this._installerLogic.Cancel();
            }
        }
        [DependsOn(nameof(InstallPhase))]
        public bool CanCmdCancel(object? parameter = null)
        {
            if (this.InstallPhase == DmodInstallPhase.Inactive) return true;
            if (this.InstallPhase == DmodInstallPhase.Initializing) return true;
            if (this.InstallPhase == DmodInstallPhase.AwaitingUserInput) return true;
            if (this.InstallPhase == DmodInstallPhase.Installing) return true;

            return false;
        }
        
        public async void CmdStartInstall(object? parameter = null) {
            if (this.CanCmdStartInstall() == false) return;

            await this.StartInstallation();
        }
        [DependsOn(nameof(InstallPhase))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(SelectedInstallableDestination))]
        public bool CanCmdStartInstall(object? parameter = null)
        {
            if (this.InstallPhase != DmodInstallPhase.AwaitingUserInput) return false;
            if (this.ParentWindow == null) return false;
            if (this.SelectedInstallableDestination == null) return false;
            return true;
        }

        public void CmdBrowseDmod(object? parameter = null) {
            this.BrowseDmod_Internal();
        }

        [DependsOn(nameof(IsFileBrowserActive))]
        [DependsOn(nameof(ParentWindow))]
        public bool CanCmdBrowseDmod(object? parameter = null) {
            return !this.IsFileBrowserActive && this.ParentWindow?.StorageProvider.CanOpen == true;
        }
        
        #endregion
        
        
        // ------------------------------------------------------------------------------------------
        //      Command logic
        //
        
        private Task BrowseDmod_Internal() {
            if (this.ParentWindow?.StorageProvider.CanOpen != true || this.IsFileBrowserActive) return Task.CompletedTask;

            return Task.Run(() => {
                try
                {
                    if (this.IsFileBrowserActive) return;

                    this.IsFileBrowserActive = true;

                    FilePickerOpenOptions fpo = new FilePickerOpenOptions() {
                        Title = Localizer.Instance[@"Generic/FileTypeDmod"],
                        AllowMultiple = false,
                        FileTypeFilter = new [] {
                            new FilePickerFileType("DMOD") {
                                Patterns = new [] { "*.dmod" },
                            }
                        }
                    };

                    Task<IReadOnlyList<IStorageFile>> fpoTask = this.ParentWindow.StorageProvider.OpenFilePickerAsync(fpo);
                    fpoTask.Wait();
                    IReadOnlyList<IStorageFile> results = fpoTask.Result;

                    if (results.Count > 0)
                    {
                        this.SelectedDmodSource = results[0].Path.AbsolutePath;
                    }

                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
                finally
                {
                    this.IsFileBrowserActive = false;
                }
            });
        }

        private Task StartInitializingDmod(FileInfo dmodPath)
        {
            dmodPath.Refresh();
            if (dmodPath.Exists == false) return Task.CompletedTask;

            return Task.Run(() => {
                try
                {
                    // preemptively update phase...
                    this.InstallPhase = DmodInstallPhase.Initializing;
                    
                    // create installer trace listener
                    this.InstallerTraceListener = new MyTraceListenerGui("Installer Trace Listener");
                    this.InstallerTraceListener.ShowLevels = false;
                    this.InstallerTraceListener.Levels = MyTraceLevel.Critical | MyTraceLevel.Error | MyTraceLevel.Warning | MyTraceLevel.Information;
                    this.InstallerTraceListener.PropertyChanged += ( sender,  args) => {
                        this.RaisePropertyChanged(nameof(this.InstallerProgressLog));
                        this.InstallerProgressLogCaretIndex = int.MaxValue;
                    };
                    this.RaisePropertyChanged(nameof(this.InstallerProgressLog));

                    // create installer logic
                    this._installerLogic = new DmodInstaller();
                    this._installerLogic.CustomTrace.Listeners.Add(this.InstallerTraceListener);
                    this._installerLogic.ProgressReport += this.InstallerOnProgressReport;
                    this._installerLogic.Initialize(dmodPath, DmodInstallPreprocessingMode.QuickPeek);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }

        private Task StartInstallation() {
            if (this.CanCmdStartInstall() == false) return Task.CompletedTask;

            return Task.Run(() => {
                try {
                    if (this.SelectedInstallableDestination == null) return;
                    if (this._installerLogic == null) return;
                    
                    this._installerLogic.InstallDmod(this.SelectedInstallableDestination, this.DmodDirectoryOverride);
                    
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });

        }

        private void ResetInstallerStateAndFireDoneEvent(bool fireDone) {
            if (this._installerLogic != null)
            {
                this._installerLogic.CustomTrace.Flush();
                this._installerLogic.CustomTrace.Close();
                this._installerLogic.ProgressReport -= this.InstallerOnProgressReport;
                this._installerLogic = null;
            }

            this.InstallerTraceListener?.Close();
            this.InstallerTraceListener = null;
            
            this.RaisePropertyChanged(nameof(this.InstallerProgressLog));
            this.RaisePropertyChanged(nameof(this.InstallerProgressLogCaretIndex));
            this.InstallPhase = DmodInstallPhase.Inactive;
            this.SelectedInstallableDestination = null;
            this.SelectedDmodSource = "";
            this.DmodDirectoryOverride = "";

            this.InstallerProgressTitle = Localizer.Instance[@"DmodInstallerView/Title"];

            this.InstallerProgressLevel0IsVisibile = false;
            this.InstallerProgressLevel1IsVisibile = false;

            if (fireDone)
            {
                this.InstallerDone?.Invoke(this, this._installerDoneEventArgs);
            }
        }

        private void InstallerOnProgressReport(object? sender, DmodInstallerProgressEventArgs args) {
            try
            {
                if (sender is not DmodInstaller installer) return;
                
                // set current install phase
                this.InstallPhase = args.Phase;
                
                string title = Localizer.Instance[@"DmodInstallerView/Title"];
                if (string.IsNullOrWhiteSpace(installer.DmodSourceNameNoExt) == false)
                {
                    title += " - " + installer.DmodSourceNameNoExt;
                }
                this.InstallerProgressTitle = title;
                
                if (args.ProgressLevel == InstallerReportLevel.Primary) {
                    this.InstallerProgressLevel0IsVisibile = true;
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

                switch (args.Phase)
                {
                    case DmodInstallPhase.AwaitingUserInput:
                    {
                        // just finished Initializing...
                        this.DmodDirectoryOverride = installer.DmodRootName ?? "";
                        break;
                    }
                    case DmodInstallPhase.Finished:
                    {
                        // just finished overall...
                        this._installerDoneEventArgs = new DmodInstallerDoneEventArgs(installer.InstallResult, installer.SourceFile, installer.InstallDestination);
                        if (installer.InstallResult == DinkInstallerResult.Cancelled) this.ShowInstallerCancelledMessageBox();
                        if (installer.InstallResult == DinkInstallerResult.Error) this.ShowInstallerErrorMessageBox(installer.InstallException);
                        break;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        
    }
}

