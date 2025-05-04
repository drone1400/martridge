using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.DmodInstaller;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
namespace Martridge.ViewModels.Dmod {
    
    public class DmodInstallerViewModel : ViewModelAppPageWithCfg {

        /// <summary>
        /// A nice title to show at the top of the UI
        /// </summary>
        public string DmodInstallerTitle {
            get => this._dmodInstallerTitle;
            private set => this.RaiseAndSetIfChanged(ref this._dmodInstallerTitle, value);
        }
        private string _dmodInstallerTitle = "";

        /// <summary>
        /// Indicates overall install phase
        /// </summary>
        public double DmodInstallerPhaseProgressPercent {
            get => this._dmodInstallerPhaseProgressPercent;
            private set => this.RaiseAndSetIfChanged(ref this._dmodInstallerPhaseProgressPercent, value);
        }
        private double _dmodInstallerPhaseProgressPercent = 0;

        /// <summary>
        /// Shows various DMOD installer log info
        /// </summary>
        public string InstallerProgressLog {
            get => this._installerTraceListener?.Text ?? "";
        }
        private MyTraceListenerGui? _installerTraceListener = null;

        /// <summary>
        /// For keeping the DMOD isntaller log textbox scrolled down
        /// </summary>
        public int InstallerProgressLogCaretIndex {
            get => this._installerProgressLogCaretIndex;
            private set => this.RaiseAndSetIfChanged(ref this._installerProgressLogCaretIndex, value);
        }
        private int _installerProgressLogCaretIndex = 0;

        /// <summary>
        /// If true, the DMOD is actively being installed
        /// </summary>
        public bool DmodInstallerInProgress {
            get => this._dmodInstallerInProgress;
            private set => this.RaiseAndSetIfChanged(ref this._dmodInstallerInProgress, value);
        }
        private bool _dmodInstallerInProgress = false;

        /// <summary>
        /// Final DMOD source, this is what is actually used to install the DMOD
        /// </summary>
        public string FinalDmodSource {
            get => this._finalDmodSource;
            private set => this.RaiseAndSetIfChanged(ref this._finalDmodSource, value);
        }
        private string _finalDmodSource = "";
        
        /// <summary>
        /// Temporary DMOD source value holder
        /// </summary>
        public string TemporaryDmodSource {
            get => this._temporaryDmodSource;
            set => this.RaiseAndSetIfChanged( ref this._temporaryDmodSource, value);
        }
        private string _temporaryDmodSource = "";

        /// <summary>
        /// Known DMOD base destination locations
        /// </summary>
        public ObservableCollection<DirectoryInfo> BaseDestinations { get; } = new ObservableCollection<DirectoryInfo>();

        
        /// <summary>
        /// Selected DMOD base destination
        /// </summary>
        public DirectoryInfo? SelectedBaseDestination {
            get => this._selectedBaseDestination;
            set
            {
                this.RaiseAndSetIfChanged(ref this._selectedBaseDestination, value);
                this.RefreshFinalDmodDestination();
            }
        }
        private DirectoryInfo? _selectedBaseDestination = null;

        /// <summary>
        /// The final DMOD folder, overrides whatever the top level DMOD folder in the .dmod archive is
        /// </summary>
        public string DesiredDmodDirectory {
            get => this._desiredDmodDirectory;
            set
            {
                this.RaiseAndSetIfChanged(ref this._desiredDmodDirectory, value);
                this.RefreshFinalDmodDestination();
            }
        }
        private string _desiredDmodDirectory = "";

        /// <summary>
        /// Final DMOD destination path...
        /// </summary>
        public string FinalDmodDestination {
            get => this._finalDmodDestination;
            private set => this.RaiseAndSetIfChanged(ref this._finalDmodDestination, value);
        }
        private string _finalDmodDestination = "";

        /// <summary>
        /// Show the option to overwrite existing DMODs
        /// </summary>
        public bool ShowDesiredDmodDirectoryOverwrite {
            get => this._showDesiredDmodDirectoryOverwrite;
            set => this.RaiseAndSetIfChanged(ref this._showDesiredDmodDirectoryOverwrite, value);
        }
        private bool _showDesiredDmodDirectoryOverwrite = false;

        /// <summary>
        /// Enable overwriting existing DMODs
        /// </summary>
        public bool IsEnabledDesiredDmodDirectoryOverwrite {
            get => this._isEnabledDesiredDmodDirectoryOverwrite;
            set => this.RaiseAndSetIfChanged(ref this._isEnabledDesiredDmodDirectoryOverwrite, value);
        }
        private bool _isEnabledDesiredDmodDirectoryOverwrite = false;

        /// <summary>
        /// Current DMOD install phase...
        /// </summary>
        public DmodInstallPhase InstallPhase {
            get => this._installPhase;
            private set => this.RaiseAndSetIfChanged( ref this._installPhase, value);
        }
        private DmodInstallPhase _installPhase = DmodInstallPhase.Inactive;

        /// <summary>
        /// Indicates if the file browser was opened and not yet closed
        /// </summary>
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
        
        private DmodInstaller? _installerLogic = null;
        private readonly object _syncRoot_DesiredDmodDirectory = new object();


        // ------------------------------------------------------------------------------------------
        //      Constructor
        //
        
        public DmodInstallerViewModel() {
            this.ResetInstallerState();
            
            // validate source DMOD
            this.ValidationRule(x => x.TemporaryDmodSource,
                dmodSource => {
                    try
                    {
                        return (string.IsNullOrWhiteSpace(dmodSource) == false && File.Exists(dmodSource));
                    } catch (Exception)
                    {
                        return false;
                    }
                },
                Localizer.Instance["DmodInstaller/ViewModel/Validation/MissingSourceDmod"]);
            
            // validate final destination directory...
            this.ValidationRule(x => x.FinalDmodDestination,
                finalDestination => string.IsNullOrWhiteSpace(finalDestination) == false,
                Localizer.Instance["DmodInstaller/ViewModel/Validation/DestinationEmpty"]);
            this.ValidationRule(x => x.FinalDmodDestination,
                finalDestination => {
                    this.SelectedBaseDestination?.Refresh();
                    return this.SelectedBaseDestination?.Exists == true;
                },
                Localizer.Instance["DmodInstaller/ViewModel/Validation/DestinationBaseDoesNotExist"]);
            this.ValidationRule(x => x.FinalDmodDestination,
                finalDestination => {
                    try
                    {
                        // reset the show overwrite checkbox every time we validate...
                        // the user must explicitly confirm the *final* location if they want to overwrite it
                        this.ShowDesiredDmodDirectoryOverwrite = false;
                        this.IsEnabledDesiredDmodDirectoryOverwrite = false;
                        
                        bool exists = Directory.Exists(finalDestination);
                        if (exists) this.ShowDesiredDmodDirectoryOverwrite = true;
                        return exists == false;
                    } catch (Exception)
                    {
                        // if we get an exception, technically the directory does not exist so don't show this validation message..
                        return true;
                    }
                },
                Localizer.Instance["DmodInstaller/ViewModel/Validation/DestinationAlreadyExists"]);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            
            if (this._installerLogic != null && this._installerLogic.InstallPhase != DmodInstallPhase.Finished)
                this._installerLogic.Cancel();
        }

        protected override void OnConfigGeneralChanged() {
            this.InitializeDmodLocations();
        }
        protected override void OnCfgGeneralUpdated(object? sender, ConfigUpdateEventArgs e) {
            this.InitializeDmodLocations();
        }

        private void InitializeDmodLocations() {
            if (this.CfgGeneral == null) return;
            this.BaseDestinations.Clear();
            List<DirectoryInfo> dmodPlaces = this.CfgGeneral.GetRealDmodDirectories();

            foreach (DirectoryInfo dirInfo in dmodPlaces) {
                this.BaseDestinations.Add(dirInfo);
            }
            
            this.SelectedBaseDestination = dmodPlaces.First();
        }

        protected override void OnConfigRememberChanged() {
            if (this.CfgRemember == null) return;
            
            if (this._installerLogic == null || this._installerLogic.InstallPhase != DmodInstallPhase.Inactive) {
                this.TemporaryDmodSource = this.CfgRemember.InstallDmodSourcePath;
                if (string.IsNullOrWhiteSpace(this.CfgRemember.InstallDmodDestinationBaseDirectory) == false) {
                    foreach (DirectoryInfo x in this.BaseDestinations) {
                        if (LocationHelper.PathIsEqual(
                                x.FullName, 
                                this.CfgRemember.InstallDmodDestinationBaseDirectory, 
                                LocationHelperPathCompareFlags.IgnoreDirectorySeparator)) 
                        {
                            this.SelectedBaseDestination = x;
                            break;
                        }
                    }
                }
            }
        }

        // ------------------------------------------------------------------------------------------
        //      Commands
        //

        #region Commands

        public async void CmdInitializeDmod(object? parameter = null)
        {
            if (this.InstallPhase != DmodInstallPhase.Inactive) return;
            if (parameter is not string dmodPath) return;
            
            if (File.Exists(dmodPath) == false) return;

            this.FinalDmodSource = dmodPath;
            await this.StartInitializingDmod();
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
            
            this.RememberSelections();
            this.ResetInstallerState();
            this.FireInstallerDoneEvent();
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
                this.RememberSelections();
                this.ResetInstallerState();
                this.FireInstallerDoneEvent();
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
        [DependsOn(nameof(SelectedBaseDestination))]
        [DependsOn(nameof(DesiredDmodDirectory))]
        [DependsOn(nameof(FinalDmodDestination))]
        [DependsOn(nameof(IsEnabledDesiredDmodDirectoryOverwrite))]
        public bool CanCmdStartInstall(object? parameter = null)
        {
            if (this.InstallPhase != DmodInstallPhase.AwaitingUserInput) return false;
            if (this.SelectedBaseDestination == null) return false;
            if (string.IsNullOrWhiteSpace(this.DesiredDmodDirectory)) return false;
            
            // base destination must exist, final destination must not or overwrite must be enabled...
            return
                Directory.Exists(this.SelectedBaseDestination.FullName) &&
                (Directory.Exists(this.FinalDmodDestination) == false || this.IsEnabledDesiredDmodDirectoryOverwrite);
        }

        public async void CmdBrowseDmod(object? parameter = null) {
            await this.BrowseDmodSource();
        }

        [DependsOn(nameof(IsFileBrowserActive))]
        public bool CanCmdBrowseDmod(object? parameter = null) {
            return !this.IsFileBrowserActive;
        }
        
        #endregion
        
        
        // ------------------------------------------------------------------------------------------
        //      Command logic
        //

        private void RefreshFinalDmodDestination()
        {
            if (this._selectedBaseDestination != null)
            {
                this.FinalDmodDestination = Path.Combine(this._selectedBaseDestination.FullName, this.DesiredDmodDirectory);
            }
            else
            {
                this.FinalDmodDestination = "";
            }
        }
        
        private Task BrowseDmodSource() {
            if (this.IsFileBrowserActive)
                return Task.CompletedTask;
            
            this.IsFileBrowserActive = true;

            return Task.Run(() => {
                try
                {
                    IStorageFile? storageFile = LocationHelper.BrowseFileOpen(
                        Localizer.Instance["DmodInstaller/ViewModel/BrowseDmodSource"],
                        new [] {
                            new FilePickerFileType("DMOD") {
                                Patterns = new [] { "*.dmod" },
                            },
                        }, this.TemporaryDmodSource);

                    if (storageFile != null)
                    {
                        this.TemporaryDmodSource = storageFile.Path.LocalPath;
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

        private Task StartInitializingDmod()
        {
            return Task.Run(() => {
                try
                {
                    FileInfo fileInfo = new FileInfo(this.FinalDmodSource);
                    if (fileInfo.Exists == false) return;
                    
                    // preemptively update phase...
                    this.InstallPhase = DmodInstallPhase.Initializing;
                    
                    // create installer trace listener
                    this._installerTraceListener = new MyTraceListenerGui("Installer Trace Listener");
                    this._installerTraceListener.ShowLevels = false;
                    this._installerTraceListener.Levels = MyTraceLevel.Critical | MyTraceLevel.Error | MyTraceLevel.Warning | MyTraceLevel.Information;
                    this._installerTraceListener.PropertyChanged += ( _,  _) => {
                        this.RaisePropertyChanged(nameof(this.InstallerProgressLog));
                        this.InstallerProgressLogCaretIndex = int.MaxValue;
                    };
                    this.RaisePropertyChanged(nameof(this.InstallerProgressLog));

                    // create installer logic
                    this._installerLogic = new DmodInstaller();
                    this._installerLogic.CustomTrace.Listeners.Add(this._installerTraceListener);
                    this._installerLogic.ProgressReport += this.InstallerOnProgressReport;
                    this._installerLogic.DmodInstallerActivityStarted += this.InstallerLogicOnDmodInstallerActivityStarted;
                    this._installerLogic.DmodInstallerActivityEnded += this.InstallerLogicOnDmodInstallerActivityEnded;
                    this._installerLogic.Initialize(fileInfo, DmodInstallPreprocessingMode.QuickPeek);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }

        private Task StartInstallation() {
            if (this.CanCmdStartInstall() == false) return Task.CompletedTask;

            return Task.Run(() => {
                try {
                    if (this.SelectedBaseDestination == null) return;
                    if (this._installerLogic == null) return;
                    
                    this._installerLogic.InstallDmod(this.SelectedBaseDestination, this.DesiredDmodDirectory, this.IsEnabledDesiredDmodDirectoryOverwrite);
                    
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });

        }

        private void ResetInstallerState() {
            if (this._installerLogic != null)
            {
                this._installerLogic.CustomTrace.Flush();
                this._installerLogic.CustomTrace.Close();
                this._installerLogic.ProgressReport -= this.InstallerOnProgressReport;
                this._installerLogic = null;
            }

            this._installerTraceListener?.Close();
            this._installerTraceListener = null;
            
            this.RaisePropertyChanged(nameof(this.InstallerProgressLog));
            this.RaisePropertyChanged(nameof(this.InstallerProgressLogCaretIndex));
            
            this.InstallPhase = DmodInstallPhase.Inactive;
            
            this.SelectedBaseDestination = null;
            this.TemporaryDmodSource = "";
            this.DesiredDmodDirectory = "";

            this.DmodInstallerTitle = Localizer.Instance[@"DmodInstaller/ViewModel/Title"];
            this.DmodInstallerPhaseProgressPercent = 0.0;
            this.DmodInstallerInProgress = false;
        }

        private void RememberSelections() {
            try {
                if (this.CfgRemember != null) {
                    Dictionary<string, object?> values = new Dictionary<string, object?>() {
                        [nameof(ConfigRemember.InstallDmodSourcePath)] = this.TemporaryDmodSource,
                        [nameof(ConfigRemember.InstallDmodDestinationBaseDirectory)] = this.SelectedBaseDestination?.FullName ?? "",
                    };
                    this.CfgRemember.UpdateProperties(values);
                }
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        private void FireInstallerDoneEvent() {
            try {
                this.InstallerDone?.Invoke(this, this._installerDoneEventArgs);
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }
        
        private void InstallerLogicOnDmodInstallerActivityEnded(object? sender, EventArgs e)
        {
            this.DmodInstallerInProgress = false;
        }
        private void InstallerLogicOnDmodInstallerActivityStarted(object? sender, EventArgs e)
        {
            this.DmodInstallerInProgress = true;
        }

        private void InstallerOnProgressReport(object? sender, DmodInstallerProgressEventArgs args) {
            try
            {
                if (sender is not DmodInstaller installer) return;
                
                // set current install phase
                this.InstallPhase = args.Phase;
                
                string title = Localizer.Instance[@"DmodInstaller/ViewModel/Title"];
                if (string.IsNullOrWhiteSpace(installer.DmodSourceNameNoExt) == false)
                {
                    title += " - " + installer.DmodSourceNameNoExt;
                }
                this.DmodInstallerTitle = title;
                
                this.DmodInstallerPhaseProgressPercent = args.ProgressPercent;
                
                // just in case this wasn't cleared?...
                if (args.Phase == DmodInstallPhase.Finished)
                {
                    this.DmodInstallerInProgress = false;
                }

                switch (args.Phase)
                {
                    case DmodInstallPhase.AwaitingUserInput:
                    {
                        // just finished Initializing...
                        lock (this._syncRoot_DesiredDmodDirectory)
                        {
                            this.DesiredDmodDirectory = installer.DmodRootName ?? "";
                        }
                        break;
                    }
                    case DmodInstallPhase.Finished:
                    {
                        // just finished overall...
                        this._installerDoneEventArgs = new DmodInstallerDoneEventArgs(installer.InstallResult, installer.SourceFile, installer.InstallationFinalDestination ?? installer.InstallDestination);
                        if (installer.InstallResult == DinkInstallerResult.Cancelled) this.ShowInstallerCancelledMessageBox();
                        if (installer.InstallResult == DinkInstallerResult.Error) this.ShowInstallerErrorMessageBox(installer.InstallException);
                        break;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }
        }

        // ------------------------------------------------------------------------------------------
        //      Message box stuff
        //

        private void ShowInstallerCancelledMessageBox() {
            Dispatcher.UIThread.InvokeAsync(async () => {
                try {
                    string title = Localizer.Instance["DmodInstaller/ViewModel/MessageBox_Cancel_Title"];
                    string body = Localizer.Instance["DmodInstaller/ViewModel/MessageBox_Cancel_Body"];
                    if (Directory.Exists(this.FinalDmodDestination))
                    {
                        body += Environment.NewLine;
                        body += Environment.NewLine;
                        body += Localizer.Instance["DmodInstaller/ViewModel/MessageBox_Extra_ManualCleanup"];
                        body += Environment.NewLine;
                        body += this.FinalDmodDestination;
                    }
                    await DinkyAlert.ShowDialog(title, body, AlertResults.Ok, AlertType.Info);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }

        private void ShowInstallerErrorMessageBox(Exception? exception) {
            Dispatcher.UIThread.InvokeAsync(async () => {
                try {
                    string title = Localizer.Instance["DmodInstaller/ViewModel/MessageBox_Error_Title"];
                    string body = Localizer.Instance["DmodInstaller/ViewModel/MessageBox_Error_Body"] + Environment.NewLine + MyTrace.GetExceptionMessages(exception);
                    if (Directory.Exists(this.FinalDmodDestination))
                    {
                        body += Environment.NewLine;
                        body += Environment.NewLine;
                        body += Localizer.Instance["DmodInstaller/ViewModel/MessageBox_Extra_ManualCleanup"];
                        body += Environment.NewLine;
                        body += this.FinalDmodDestination;
                    }
                    await DinkyAlert.ShowDialog(title, body, AlertResults.Ok, AlertType.Error);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }
        
        public override bool ProcessKeyDown(Key key, KeyModifiers modifiers) {
            switch (key) {
                case Key.Escape:
                    this.CmdCancel();
                    return true;
            }
            return false;
        }
    }
}

