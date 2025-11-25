using Avalonia.Metadata;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Martridge.Models.Configuration.General;
using Martridge.Models.Configuration.Launcher;

namespace Martridge.ViewModels.Configuration {
    public class SettingsGeneralViewModel : ViewModelAppPageWithCfg {

        public event EventHandler? SettingsDone;
        
#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
        public bool ShowDinkInstallerSettings => true;
#else 
        public bool ShowDinkInstallerSettings => false;
#endif
        
#if ENABLE_FEATURE_ONLINE
        public bool ShowOnlineSettings => true;
#else 
        public bool ShowOnlineSettings => false;
#endif

        public SettingsExtensionComponentConfigViewModel? ExeExtensionViewModel {
            get => this._exeExtensionViewModel;
            private set => this.RaiseAndSetIfChanged(ref this._exeExtensionViewModel, value);
        }
        private SettingsExtensionComponentConfigViewModel? _exeExtensionViewModel = null;
        
        //
        // General Configuration properties
        //
        public bool ShowDmodDevFeatures {
            get => this._showDmodDevFeatures;
            set => this.RaiseAndSetIfChanged(ref this._showDmodDevFeatures, value);
        }
        private bool _showDmodDevFeatures = false;
        
        public bool EnableOnlineFeatures {
            get => this._enableOnlineFeatures;
            set => this.RaiseAndSetIfChanged(ref this._enableOnlineFeatures, value);
        }
        private bool _enableOnlineFeatures = false;
        
        public bool ShowLaunchRefDirPathInMainWindow {
            get => this._showLaunchRefDirPathInMainWindow;
            set => this.RaiseAndSetIfChanged(ref this._showLaunchRefDirPathInMainWindow, value);
        }
        private bool _showLaunchRefDirPathInMainWindow = false;
        
        public bool ShowLaunchCustomArgsInMainWindow {
            get => this._showLaunchCustomArgsInMainWindow;
            set => this.RaiseAndSetIfChanged(ref this._showLaunchCustomArgsInMainWindow, value);
        }
        private bool _showLaunchCustomArgsInMainWindow = false;

        public bool ShowLogWindowOnStartup {
            get => this._showLogWindowOnStartup;
            set => this.RaiseAndSetIfChanged(ref this._showLogWindowOnStartup, value);
        }
        private bool _showLogWindowOnStartup = false;

        public bool UseRelativePathForSubfolders {
            get => this._useRelativePathForSubfolders;
            set => this.RaiseAndSetIfChanged(ref this._useRelativePathForSubfolders, value);
        }
        private bool _useRelativePathForSubfolders = false;

        public string DefaultDmodLocation {
            get => this._defaultDmodLocation;
            set => this.RaiseAndSetIfChanged(ref this._defaultDmodLocation, value);
        }
        private string _defaultDmodLocation = "DMODS";

        public ObservableCollection<string> GameExePaths {
            get => this._gameExePaths;
            set => this.RaiseAndSetIfChanged(ref this._gameExePaths, value);
        }
        private ObservableCollection<string> _gameExePaths = new ObservableCollection<string>();

        public ObservableCollection<string> EditorExePaths {
            get => this._editorExePaths;
            set => this.RaiseAndSetIfChanged(ref this._editorExePaths, value);
        }
        private ObservableCollection<string> _editorExePaths = new ObservableCollection<string>();

        public ObservableCollection<CultureInfo> Localizations {
            get => this._localizations;
            set => this.RaiseAndSetIfChanged(ref this._localizations, value);
        }
        private ObservableCollection<CultureInfo> _localizations = new ObservableCollection<CultureInfo>();

        public CultureInfo? SelectedLocalization {
            get => this._selectedLocalization;
            set => this.RaiseAndSetIfChanged(ref this._selectedLocalization, value);
        }
        private CultureInfo? _selectedLocalization = null;
        private string? _savedLocalization = null;
        
        public ObservableCollection<string> AdditionalDmodLocations {
            get => this._additionalDmodLocations;
            set => this.RaiseAndSetIfChanged(ref this._additionalDmodLocations, value);
        }
        private ObservableCollection<string> _additionalDmodLocations = new ObservableCollection<string>();
        
        //
        // Launch settings
        //
        
        public string LaunchRefDirPath {
            get => this._launchRefDirPath;
            set => this.RaiseAndSetIfChanged(ref this._launchRefDirPath, value);
        }
        private string _launchRefDirPath = string.Empty;
        
        public string LaunchCustomUserArguments {
            get => this._launchCustomuserArguments;
            set => this.RaiseAndSetIfChanged(ref this._launchCustomuserArguments, value);
        }
        private string _launchCustomuserArguments = string.Empty;

        public bool QuitMartridgeOnGameLaunch {
            get => this._quitMartridgeOnGameLaunch;
            set => this.RaiseAndSetIfChanged(ref this._quitMartridgeOnGameLaunch, value);
        }
        private bool _quitMartridgeOnGameLaunch = false;
        
        public bool QuitMartridgeOnEditorLaunch {
            get => this._quitMartridgeOnEditorLaunch;
            set => this.RaiseAndSetIfChanged(ref this._quitMartridgeOnEditorLaunch, value);
        }
        private bool _quitMartridgeOnEditorLaunch = false;

        //
        // Internal logic
        //
        public bool IsBusy {
            get { lock (this._isBusyLock) { return this._isBusy; } }
            private set { lock (this._isBusyLock)  { this.RaiseAndSetIfChanged(ref this._isBusy, value); } }
        }
        private bool _isBusy = false;
        private readonly object _isBusyLock = new object();

        
        //
        // CONSTRUCTOR
        //
        
        public SettingsGeneralViewModel() {
            try {
                this._localizations.Clear();
                List<string> languages = Localizer.Instance.GetAvailableLanguages();
                foreach (string langId in languages) {
                    try {
                        CultureInfo ci = CultureInfo.GetCultureInfo(langId);
                        this._localizations.Add(ci);
                    } catch (Exception ex) {
                        MyTrace.Global.WriteMessage($"Could not initialize application localization for \"{langId}\"", MyTraceLevel.Error);
                        MyTrace.Global.WriteException(ex);
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage("Error initializing application localizations in settings view...", MyTraceLevel.Error);
                MyTrace.Global.WriteException(ex);
            }
            
            this.PropertyChanged += OnPropertyChanged;

            
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.SelectedLocalization)) {
                if (this.SelectedLocalization != null &&
                    Localizer.Instance.Language != this.SelectedLocalization.Name) {
                    Localizer.Instance.LoadLanguage(this.SelectedLocalization.Name);
                }
            }
        }

        protected override void OnConfigGeneralChanged() {
            this.LoadFromConfig();
        }

        protected override void OnCfgGeneralUpdated(object? sender, ConfigUpdateEventArgs e) {
            this.LoadFromConfig();
        }

        protected override void OnConfigLaunchChanged() {
            this.LoadFromConfigLaunch();
        }

        protected override void OnCfgLaunchUpdated(object? sender, ConfigUpdateEventArgs e) {
            this.LoadFromConfigLaunch();
        }

        #region LOAD / SAVE Config
        
        private void LoadFromConfigLaunch() {
            if (this.CfgLaunch == null) return;

            this.LaunchRefDirPath = this.CfgLaunch.RefDirPath;
            this.LaunchCustomUserArguments = this.CfgLaunch.CustomUserArguments;
            this.QuitMartridgeOnGameLaunch = this.CfgLaunch.QuitMartridgeOnGameLaunch;
            this.QuitMartridgeOnEditorLaunch = this.CfgLaunch.QuitMartridgeOnEditorLaunch;
        }

        private void SaveToConfigLaunch() {
            if (this.CfgLaunch == null) return;
            
            this.CfgLaunch.UpdateProperties(new Dictionary<string, object?>() {
                [nameof(ConfigLaunch.RefDirPath)] = this.LaunchRefDirPath,
                [nameof(ConfigLaunch.CustomUserArguments)] = this.LaunchCustomUserArguments,
                [nameof(ConfigLaunch.QuitMartridgeOnGameLaunch)] = this.QuitMartridgeOnGameLaunch,
                [nameof(ConfigLaunch.QuitMartridgeOnEditorLaunch)] = this.QuitMartridgeOnEditorLaunch,
            });
        }

        private void LoadFromConfig() {
            if (this.CfgGeneral == null) { return; }


            ObservableCollection<string> listGameExe = new ObservableCollection<string>();
            foreach (string str in this.CfgGeneral.GameExePaths) {
                listGameExe.Add(str);
            }
            
            ObservableCollection<string> listEditorExe = new ObservableCollection<string>();
            foreach (string str in this.CfgGeneral.EditorExePaths) {
                listEditorExe.Add(str);
            }

            ObservableCollection<string> listDmod = new ObservableCollection<string>();
            foreach (string str in this.CfgGeneral.AdditionalDmodLocations) {
                listDmod.Add(str);
            }
            
            this.ShowLogWindowOnStartup = this.CfgGeneral.ShowLogWindowOnStartup;
            this.ShowDmodDevFeatures = this.CfgGeneral.ShowDmodDevFeatures;
            this.EnableOnlineFeatures = this.CfgGeneral.EnableOnlineFeatures;
            this.ShowLaunchRefDirPathInMainWindow = this.CfgGeneral.ShowLaunchRefDirPathInMainWindow;
            this.ShowLaunchCustomArgsInMainWindow = this.CfgGeneral.ShowLaunchCustomArgsInMainWindow;
            this.UseRelativePathForSubfolders = this.CfgGeneral.UseRelativePathForSubfolders;
            this.AdditionalDmodLocations = listDmod;
            this.DefaultDmodLocation = this.CfgGeneral.DefaultDmodLocation;
            this.GameExePaths = listGameExe;
            this.EditorExePaths = listEditorExe;
            
            // find and select the right localization
            foreach (CultureInfo ci in this._localizations) {
                if (ci.Name == this.CfgGeneral.LocalizationName) {
                    this.SelectedLocalization = ci;
                }
            }

            this._savedLocalization = Localizer.Instance.Language;
        }

        private void SaveToConfig() {
            if (this.CfgGeneral == null) { return; }

            List<string> listGameExe = new List<string>();
            foreach (string str in this.GameExePaths) {
                listGameExe.Add(str);
            }
            
            List<string> listEditorExe = new List<string>();
            foreach (string str in this.EditorExePaths) {
                listEditorExe.Add(str);
            }

            List<string> listDmod = new List<string>();
            foreach (string str in this.AdditionalDmodLocations) {
                listDmod.Add(str);
            }

            this._savedLocalization = Localizer.Instance.Language;

            this.CfgGeneral.UpdateProperties(new Dictionary<string, object?>() {
                [nameof(ConfigGeneral.LocalizationName)] = this._savedLocalization ?? "en-US",
                [nameof(ConfigGeneral.ShowDmodDevFeatures)] = this.ShowDmodDevFeatures,
                [nameof(ConfigGeneral.EnableOnlineFeatures)] = this.EnableOnlineFeatures,
                [nameof(ConfigGeneral.ShowLaunchRefDirPathInMainWindow)] = this.ShowLaunchRefDirPathInMainWindow,
                [nameof(ConfigGeneral.ShowLaunchCustomArgsInMainWindow)] = this.ShowLaunchCustomArgsInMainWindow,
                [nameof(ConfigGeneral.ShowLogWindowOnStartup)] = this.ShowLogWindowOnStartup,
                [nameof(ConfigGeneral.UseRelativePathForSubfolders)] = this.UseRelativePathForSubfolders,
                [nameof(ConfigGeneral.GameExePaths)] = listGameExe,
                [nameof(ConfigGeneral.EditorExePaths)] = listEditorExe,
                [nameof(ConfigGeneral.DefaultDmodLocation)] = this.DefaultDmodLocation,
                [nameof(ConfigGeneral.AdditionalDmodLocations)] = listDmod,
            });
        }
        
        #endregion

        #region COMMANDS - OK / CANCEL

        public void CmdSettingsOk(object? parameter = null) {
            this.SaveToConfig();
            this.SaveToConfigLaunch();
            // signal that settings are done...
            this.SettingsDone?.Invoke(this, EventArgs.Empty);
        }

        [DependsOn(nameof(CfgGeneral))]
        [DependsOn(nameof(CfgLaunch))]
        public bool CanCmdSettingsOk(object? parameter = null) {
            if (this.CfgGeneral == null) { return false; }
            if (this.CfgLaunch == null) { return false; }
            return true;
        }

        public void CmdSettingsCancel(object? parameter = null) {
            // restore saved localization...
            if (this._savedLocalization != null) {
                Localizer.Instance.LoadLanguage(this._savedLocalization);
            }

            this.LoadFromConfig();

            // signal that settings are done...
            this.SettingsDone?.Invoke(this, EventArgs.Empty);
        }
        [DependsOn(nameof(CfgGeneral))]
        public bool CanCmdSettingsCancel(object? parameter = null) {
            //if (this.Configuration == null) { return false; }
            return true;
        }
        
        #endregion

        #region COMMANDS LAUNCH

        //
        // Default dmods
        //
        
        public async void CmdLaunchRefDirBrowse(object? parameter = null) {
            if (this.IsBusy ) return;
            
            this.IsBusy = true;

            await Task.Run(() => {
                try
                {
                    IStorageFolder? storageFolder = LocationHelper.BrowseFolderPicker(
                        Localizer.Instance["SettingsGeneral/BrowseRefDirDirectory"],
                        string.IsNullOrWhiteSpace(this.LaunchRefDirPath) 
                            ? LocationHelper.GetPathDefaultFileBrowser()
                            : this.LaunchRefDirPath );
                    
                    if (storageFolder != null)
                    {
                        this.LaunchRefDirPath = storageFolder.Path.LocalPath;
                    }
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(ex);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }
        
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdLaunchRefDirBrowse(object? parameter = null) {
            // general conditions
            if (this.IsBusy ) return false;
            // specific conditions
            return true;
        }

        #endregion
        
        #region COMMANDS - DMODs

        //
        // Default dmods
        //
        
        public async void CmdDefaultDmodsBrowse(object? parameter = null) {
            if (this.IsBusy ) return;
            
            this.IsBusy = true;

            await Task.Run(() => {
                try
                {
                    IStorageFolder? storageFolder = LocationHelper.BrowseFolderPicker(
                        Localizer.Instance["SettingsGeneral/BrowseDefaultDmodDirectory"],
                        string.IsNullOrWhiteSpace(this.DefaultDmodLocation) 
                            ? LocationHelper.GetPathDefaultFileBrowser()
                            : this.DefaultDmodLocation );
                    
                    if (storageFolder != null)
                    {
                        this.DefaultDmodLocation = storageFolder.Path.LocalPath;
                    }
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(ex);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }
        
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdDefaultDmodsBrowse(object? parameter = null) {
            // general conditions
            if (this.IsBusy ) return false;
            // specific conditions
            return true;
        }


        //
        // Additional dmods
        //

        public void CmdAdditionalDmodsRemoveSelected(object? parameter = null) {
            if (parameter is not string target) return;
            if (this.IsBusy ) return;
            this.AdditionalDmodLocations.Remove(target);

        }
        
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(AdditionalDmodLocations))]
        public bool CanCmdAdditionalDmodsRemoveSelected(object? parameter = null) {
            if (parameter is not string) return false;
            if (this.IsBusy ) return false;
            return true;
        }

        
        public string AdditionalDmodLocationsAddNewManualValue {
            get => this._additionalDmodLocationsAddNewManualValue;
            set => this.RaiseAndSetIfChanged(ref  this._additionalDmodLocationsAddNewManualValue, value);
        }
        private string _additionalDmodLocationsAddNewManualValue = string.Empty;
        
        
        public void CmdAdditionalDmodLocationsAddNewManual(object? parameter = null) {
            if (this.IsBusy) return;
            if (string.IsNullOrWhiteSpace(this.AdditionalDmodLocationsAddNewManualValue)) return;
            
            try
            {
                this.IsBusy = true;
                
                if (LocationHelper.PathIsDuplicate(this.AdditionalDmodLocations, this.AdditionalDmodLocationsAddNewManualValue) == false)
                {
                    this.AdditionalDmodLocations.Add(this.AdditionalDmodLocationsAddNewManualValue);
                }
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }
        
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(AdditionalDmodLocationsAddNewManualValue))]
        public bool CanCmdAdditionalDmodLocationsAddNewManual(object? parameter = null) {
            if (this.IsBusy) return false;
            if (string.IsNullOrWhiteSpace(this.AdditionalDmodLocationsAddNewManualValue)) return false;
            return true;
        }
        
        public async void CmdAdditionalDmodsAddNew(object? parameter = null) {
            if (this.IsBusy ) return;

            this.IsBusy = true;
            
            await Task.Run(() => {
                try
                {
                    IStorageFolder? storageFolder = LocationHelper.BrowseFolderPicker(
                        Localizer.Instance["SettingsGeneral/BrowseAddDmodDirectory"],
                        string.IsNullOrWhiteSpace(this.DefaultDmodLocation) 
                            ? LocationHelper.GetPathDefaultFileBrowser()
                            : this.DefaultDmodLocation );

                    if (storageFolder != null && LocationHelper.PathIsDuplicate(this.AdditionalDmodLocations, storageFolder.Path.LocalPath) == false)
                    {
                        this.AdditionalDmodLocations.Add(storageFolder.Path.LocalPath);
                    }
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(ex);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }
        
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdAdditionalDmodsAddNew(object? parameter = null) {
            // general conditions
            if (this.IsBusy ) return false;
            // specific conditions
            return true;
        }
        
        #endregion
        
        #region COMMANDS - GAME EXE

        //
        // Game exe paths
        //
        public void CmdGameExeRemove(object? parameter = null) {
            if (parameter is not string target) return;
            if (this.IsBusy ) return;
            this.GameExePaths.Remove(target);
        }
        
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdGameExeRemove(object? parameter = null) {
            if (parameter is not string) return false;
            if (this.IsBusy ) return false;
            return true;
        }

        public string GameExeAddNewManualValue {
            get => this._gameExeAddNewManualValue;
            set => this.RaiseAndSetIfChanged(ref  this._gameExeAddNewManualValue, value);
        }
        private string _gameExeAddNewManualValue = string.Empty;
        
        
        public void CmdGameExeAddNewManual(object? parameter = null) {
            if (this.IsBusy) return;
            if (string.IsNullOrWhiteSpace(this.GameExeAddNewManualValue)) return;
            
            try
            {
                this.IsBusy = true;
                
                if (LocationHelper.PathIsDuplicate(this.GameExePaths, this.GameExeAddNewManualValue) == false)
                {
                    this.GameExePaths.Add(this.GameExeAddNewManualValue);
                }
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }
        
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(GameExeAddNewManualValue))]
        public bool CanCmdGameExeAddNewManual(object? parameter = null) {
            if (this.IsBusy) return false;
            if (string.IsNullOrWhiteSpace(this.GameExeAddNewManualValue)) return false;
            return true;
        }

        public async void CmdGameExeAddNew(object? parameter = null) {
            if (this.IsBusy ) return;

            this.IsBusy = true;

            await Task.Run(() => {
                try
                {
                    IStorageFile? storageFile = LocationHelper.BrowseFileOpen(
                        Localizer.Instance["SettingsGeneral/BrowseAddGameExe"],
#if PLATF_WINDOWS
                        // on windows, browse for exe files
                        new [] {
                            new FilePickerFileType(Localizer.Instance["SettingsGeneral/BrowseFileTypeExe"]) {
                                Patterns = new [] { "*.exe" },
                            },
                        },
#else   
                        // on other platforms, browse for everything?
                        null,
#endif
                        LocationHelper.GetPathDefaultFileBrowser());

                    if (storageFile != null && LocationHelper.PathIsDuplicate(this.GameExePaths, storageFile.Path.LocalPath) == false)
                    {
                        this.GameExePaths.Add(storageFile.Path.LocalPath);
                    }
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(ex);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }

        [DependsOn(nameof(IsBusy))]
        public bool CanCmdGameExeAddNew(object? parameter = null) {
            // general conditions
            if (this.IsBusy ) return false;
            // specific conditions
            return true;
        }
        
        #endregion
        
        #region COMMANDS - EDITOR EXE

        //
        // Editor exe paths
        //
        public void CmdEditorExeRemove(object? parameter = null) {
            if (parameter is not string target) return;
            if (this.IsBusy ) return;
            this.EditorExePaths.Remove(target);
        }
        
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdEditorExeRemove(object? parameter = null) {
            if (parameter is not string) return false;
            if (this.IsBusy ) return false;
            return true;
        }

        public string EditorExeAddNewManualValue {
            get => this._editorExeAddNewManualValue;
            set => this.RaiseAndSetIfChanged(ref  this._editorExeAddNewManualValue, value);
        }
        private string _editorExeAddNewManualValue = string.Empty;
        
        
        public void CmdEditorExeAddNewManual(object? parameter = null) {
            if (this.IsBusy) return;
            if (string.IsNullOrWhiteSpace(this.EditorExeAddNewManualValue)) return;
            
            try
            {
                this.IsBusy = true;
                
                if (LocationHelper.PathIsDuplicate(this.EditorExePaths, this.EditorExeAddNewManualValue) == false)
                {
                    this.EditorExePaths.Add(this.EditorExeAddNewManualValue);
                }
            } catch (Exception ex)
            {
                MyTrace.Global.WriteException(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }
        
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(EditorExeAddNewManualValue))]
        public bool CanCmdEditorExeAddNewManual(object? parameter = null) {
            if (this.IsBusy) return false;
            if (string.IsNullOrWhiteSpace(this.EditorExeAddNewManualValue)) return false;
            return true;
        }

        public async void CmdEditorExeAddNew(object? parameter = null) {
            if (this.IsBusy ) return;

            this.IsBusy = true;

            await Task.Run(() => {

                try
                {
                    IStorageFile? storageFile = LocationHelper.BrowseFileOpen(
                        Localizer.Instance["SettingsGeneral/BrowseAddEditorExe"],
#if PLATF_WINDOWS
                        // on windows, browse for exe files
                        new [] {
                            new FilePickerFileType(Localizer.Instance["SettingsGeneral/BrowseFileTypeExe"]) {
                                Patterns = new [] { "*.exe" },
                            },
                        },
#else   
                        // on other platforms, browse for everything?
                        null,
#endif
                        LocationHelper.GetPathDefaultFileBrowser());

                    if (storageFile != null && LocationHelper.PathIsDuplicate(this.EditorExePaths, storageFile.Path.LocalPath) == false) {
                        this.EditorExePaths.Add(storageFile.Path.LocalPath);
                    }
                } catch (Exception ex)
                {
                    MyTrace.Global.WriteException(ex);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }

        [DependsOn(nameof(IsBusy))]
        public bool CanCmdEditorExeAddNew(object? parameter = null) {
            // general conditions
            if (this.IsBusy ) return false;
            // specific conditions
            return true;
        }
        
        #endregion
        
        #region COMMANDS - EXE EXTENSION

        public void CmdExeExtensionEdit(object? parameter = null) {
            if (parameter is not string target) return;
            if (this.IsBusy ) return;
            if (this.ExeExtensionViewModel != null) return;
            if (this.CfgExtension == null) return;
            
            this.ExeExtensionViewModel = new SettingsExtensionComponentConfigViewModel();

            ConfigExtensionComponent? component = this.CfgExtension.TryAddOrGetExtension(target);
            if (component == null) return;
            
            this.ExeExtensionViewModel.Initialize(target, component.WineData, component.SteamData);
        }
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(ExeExtensionViewModel))]
        [DependsOn(nameof(CfgExtension))]
        public bool CanCmdExeExtensionEdit(object? parameter = null) {
            if (parameter is not string) return false;
            if (this.IsBusy ) return false;
            if (this.ExeExtensionViewModel != null) return false;
            if (this.CfgExtension == null) return false;
            return true;
        }
        
        public void CmdExeExtensionOk(object? parameter = null) {
            if (this.ExeExtensionViewModel == null) return;
            if (this.CfgExtension == null) return;

            try {
                try {
                    string exeOriginal = this.ExeExtensionViewModel.ExePathOriginal;
                    string exeCurrent = this.ExeExtensionViewModel.ExePath;
                    var cfgWine = this.ExeExtensionViewModel.GetWineData();
                    var cfgSteam = this.ExeExtensionViewModel.GetSteamData();



                    if (exeOriginal != exeCurrent) {
                        // have to update exe path...
                        int foundGameExeIndex = this.GameExePaths.IndexOf(exeOriginal);
                        if (foundGameExeIndex != -1) {
                            this.GameExePaths[foundGameExeIndex] = exeCurrent;
                        }

                        int foundEditorExeIndex = this.EditorExePaths.IndexOf(exeOriginal);
                        if (foundEditorExeIndex != -1) {
                            this.EditorExePaths[foundEditorExeIndex] = exeCurrent;
                        }

                        this.CfgExtension.TryChangeTargetPath(exeOriginal, exeCurrent);
                    }

                    // update data
                    ConfigExtensionComponent? component = this.CfgExtension.TryAddOrGetExtension(exeCurrent);
                    if (component == null) return;
                    component.SteamData = cfgSteam;
                    component.WineData = cfgWine;
                }
                finally {
                    // TODO... this is rather inconsistent, as in other places the config is passed to the view model by instance, but here i just save it directly in the current app
                    // i think i'll have to reorganize how i handle the config objects later...
                    // save changes...
                    if (Application.Current is App app) {
                        app.SaveConfigExtension();
                    }
                    
                    this.ExeExtensionViewModel = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            } 
        }
        [DependsOn(nameof(ExeExtensionViewModel))]
        [DependsOn(nameof(CfgExtension))]
        public bool CanCmdExeExtensionOk(object? parameter = null) {
            return this.ExeExtensionViewModel != null;
        }
        
        public void CmdExeExtensionCancel(object? parameter = null) {
            this.ExeExtensionViewModel = null;
        }
        [DependsOn(nameof(ExeExtensionViewModel))]
        [DependsOn(nameof(CfgExtension))]
        public bool CanCmdExeExtensionCancel(object? parameter = null) {
            return this.ExeExtensionViewModel != null;
        }
        
        #endregion
        
        #region COMMANDS - OTHER
        
        public void CmdShowLogWindow(object? parameter = null) {
            App.Instance?.ShowLogWindow();
        }
        
        #endregion
        
        public override bool ProcessKeyDown(Key key, KeyModifiers modifiers) {
            switch (key) {
                case Key.Escape:
                    this.CmdSettingsCancel();
                    return true;
            }
            return false;
        }
    }
}
