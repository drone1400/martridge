using Avalonia.Controls;
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
using Martridge.Models.Configuration.Save;

namespace Martridge.ViewModels.Configuration {
    public class SettingsGeneralViewModel : ViewModelBase {

        public event EventHandler? SettingsDone;

        //
        // General Configuration object...
        //
        public ConfigGeneral? Configuration { 
            get => this._cfg;
            set {
                if (this._cfg != null) {
                    this._cfg.Updated -= this.CfgOnUpdated;
                }
                this.RaiseAndSetIfChanged(ref this._cfg, value);
                
                if (this._cfg != null) {
                    this._cfg.Updated += this.CfgOnUpdated;
                    this.LoadFromConfig();
                }
            }
        }
        private ConfigGeneral? _cfg = null;
        private void CfgOnUpdated(object? sender, EventArgs e) {
            this.LoadFromConfig();
        }
        
        //
        // General Configuration properties
        //

        public ApplicationTheme ThemeName {
            get => this._themeName;
            set => this.RaiseAndSetIfChanged(ref this._themeName, value);
        }
        private ApplicationTheme _themeName;
        
        public bool AutoUpdateInstallerList {
            get => this._autoUpdateInstallerList;
            set => this.RaiseAndSetIfChanged(ref this._autoUpdateInstallerList, value);
        }
        private bool _autoUpdateInstallerList = false;
        
        public bool ShowAdvancedFeatures {
            get => this._showAdvancedFeatures;
            set => this.RaiseAndSetIfChanged(ref this._showAdvancedFeatures, value);
        }
        private bool _showAdvancedFeatures = false;

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
        
        public int ActiveGameExeIndex {
            get => this._activeGameExeIndex;
            set => this.RaiseAndSetIfChanged(ref this._activeGameExeIndex, value);
        }
        private int _activeGameExeIndex = 0;

        public ObservableCollection<string> GameExePaths {
            get => this._gameExePaths;
            set => this.RaiseAndSetIfChanged(ref this._gameExePaths, value);
        }
        private ObservableCollection<string> _gameExePaths = new ObservableCollection<string>();
        
        public int ActiveEditorExeIndex {
            get => this._activeEditorExeIndex;
            set => this.RaiseAndSetIfChanged(ref this._activeEditorExeIndex, value);
        }
        private int _activeEditorExeIndex = 0;

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

        public int AdditionalDmodLocationsIndex {
            get => this._additionalDmodLocationsIndex;
            set => this.RaiseAndSetIfChanged(ref this._additionalDmodLocationsIndex, value);
        }
        private int _additionalDmodLocationsIndex = 0;
        public ObservableCollection<string> AdditionalDmodLocations {
            get => this._additionalDmodLocations;
            set => this.RaiseAndSetIfChanged(ref this._additionalDmodLocations, value);
        }
        private ObservableCollection<string> _additionalDmodLocations = new ObservableCollection<string>();

        //
        // Internal logic
        //
        public bool IsBusy {
            get { lock (this._isBusyLock) { return this._isBusy; } }
            private set { lock (this._isBusyLock)  { this.RaiseAndSetIfChanged(ref this._isBusy, value); } }
        }
        private bool _isBusy = false;
        private readonly object _isBusyLock = new object();

        public SettingsGeneralViewModel() {
            // set current theme...
            this._themeName = StyleManager.Instance.CurrentApplicationTheme;
            StyleManager.Instance.ThemeChanged += InstanceOnThemeChanged;
            
            try {
                this._localizations.Clear();
                List<string> languages = Localizer.Instance.GetAvailableLanguages();
                foreach (string langId in languages) {
                    try {
                        CultureInfo ci = CultureInfo.GetCultureInfo(langId);
                        this._localizations.Add(ci);
                    } catch (Exception ex) {
                        MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Could not initialize application localization for \"{langId}\"", MyTraceLevel.Error);
                        MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, "Error initializing application localizations in settings view...", MyTraceLevel.Error);
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
            
            this.PropertyChanged += OnPropertyChanged;

            
        }
        private void InstanceOnThemeChanged(object? sender, EventArgs e) {
            if (sender is not StyleManager styleMan) return;
            this.ThemeName = styleMan.CurrentApplicationTheme;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.SelectedLocalization)) {
                if (this.SelectedLocalization != null &&
                    Localizer.Instance.Language != this.SelectedLocalization.Name) {
                    Localizer.Instance.LoadLanguage(this.SelectedLocalization.Name);
                }
            } else if (e.PropertyName == nameof(this.ThemeName)) {
                // update in configuration...
                this.Configuration?.UpdateProperties(new Dictionary<string, object?>() {
                    [nameof(ConfigGeneral.ThemeName)] = this.ThemeName.ToString(),
                });
                // update in style manager...
                StyleManager.Instance.UseTheme(this.ThemeName);
            }
        }

        #region LOAD / SAVE Config

        private void LoadFromConfig() {
            if (this._cfg == null) { return; }


            ObservableCollection<string> listGameExe = new ObservableCollection<string>();
            foreach (string str in this._cfg.GameExePaths) {
                listGameExe.Add(str);
            }
            
            ObservableCollection<string> listEditorExe = new ObservableCollection<string>();
            foreach (string str in this._cfg.EditorExePaths) {
                listEditorExe.Add(str);
            }

            ObservableCollection<string> listDmod = new ObservableCollection<string>();
            foreach (string str in this._cfg.AdditionalDmodLocations) {
                listDmod.Add(str);
            }

            if (Enum.TryParse(this._cfg.ThemeName, out ApplicationTheme theme )) {
                this.ThemeName = theme;
            }
            
            this.ShowLogWindowOnStartup = this._cfg.ShowLogWindowOnStartup;
            this.ShowAdvancedFeatures = this._cfg.ShowAdvancedFeatures;
            this.UseRelativePathForSubfolders = this._cfg.UseRelativePathForSubfolders;
            this.AutoUpdateInstallerList = this._cfg.AutoUpdateInstallerList;
            this.AdditionalDmodLocationsIndex = -1;
            this.AdditionalDmodLocations = listDmod;
            this.AdditionalDmodLocationsIndex = 0;
            this.DefaultDmodLocation = this._cfg.DefaultDmodLocation;
            this.ActiveGameExeIndex = -1;
            this.GameExePaths = listGameExe;
            this.ActiveGameExeIndex = this._cfg.ActiveGameExeIndex;
            this.ActiveEditorExeIndex = -1;
            this.EditorExePaths = listEditorExe;
            this.ActiveEditorExeIndex = this._cfg.ActiveEditorExeIndex;
            
            // find and select the right localization
            foreach (CultureInfo ci in this._localizations) {
                if (ci.Name == this._cfg.LocalizationName) {
                    this.SelectedLocalization = ci;
                }
            }

            this._savedLocalization = Localizer.Instance.Language;
        }

        private void SaveToConfig() {
            if (this.Configuration == null) { return; }

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

            if (this.ActiveGameExeIndex < 0 && this.GameExePaths.Count > 0) {
                this.ActiveGameExeIndex = 0;
            }
            
            if (this.ActiveEditorExeIndex < 0 && this.EditorExePaths.Count > 0) {
                this.ActiveEditorExeIndex = 0;
            }

            this.Configuration.UpdateProperties(new Dictionary<string, object?>() {
                [nameof(ConfigGeneral.ThemeName)] = this.ThemeName.ToString(),
                [nameof(ConfigGeneral.LocalizationName)] = this._savedLocalization ?? "en-US",
                [nameof(ConfigGeneral.AutoUpdateInstallerList)] = this.AutoUpdateInstallerList,
                [nameof(ConfigGeneral.ShowAdvancedFeatures)] = this.ShowAdvancedFeatures,
                [nameof(ConfigGeneral.ShowLogWindowOnStartup)] = this.ShowLogWindowOnStartup,
                [nameof(ConfigGeneral.UseRelativePathForSubfolders)] = this.UseRelativePathForSubfolders,
                [nameof(ConfigGeneral.ActiveGameExeIndex)] = this.ActiveGameExeIndex,
                [nameof(ConfigGeneral.ActiveEditorExeIndex)] = this.ActiveEditorExeIndex,
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
            // signal that settings are done...
            this.SettingsDone?.Invoke(this, EventArgs.Empty);
        }

        [DependsOn(nameof(Configuration))]
        public bool CanCmdSettingsOk(object? parameter = null) {
            if (this.Configuration == null) { return false; }
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
        [DependsOn(nameof(Configuration))]
        public bool CanCmdSettingsCancel(object? parameter = null) {
            //if (this.Configuration == null) { return false; }
            return true;
        }
        
        #endregion
        
        #region COMMANDS - DMODs
        
        //
        // Default dmods
        //
        
        public async void CmdDefaultDmodsSet(object? parameter = null) {
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return;

            try {
                this.IsBusy = true;
                
                OpenFolderDialog ofd = new OpenFolderDialog {
                    Directory = LocationHelper.AppBaseDirectory,
                };

                string? result = await ofd.ShowAsync(this.ParentWindow);
                if (result != null) {
                    this.DefaultDmodLocation = result;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            } finally {
                this.IsBusy = false;
            }
        }

        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdDefaultDmodsSet(object? parameter = null) {
            // general conditions
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return false;
            // specific conditions
            return true;
        }


        //
        // Additional dmods
        //

        public void CmdAdditionalDmodsRemoveSelected(object? parameter = null) {
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return;

            if (this.AdditionalDmodLocationsIndex >= 0 && this.AdditionalDmodLocationsIndex < this.AdditionalDmodLocations.Count) {
                this.AdditionalDmodLocations.RemoveAt(this.AdditionalDmodLocationsIndex);
            }
        }

        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(AdditionalDmodLocationsIndex))]
        [DependsOn(nameof(AdditionalDmodLocations))]
        public bool CanCmdAdditionalDmodsRemoveSelected(object? parameter = null) {
            // general conditions
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return false;
            // specific conditions
            return this.AdditionalDmodLocationsIndex >= 0 && this.AdditionalDmodLocationsIndex < this.AdditionalDmodLocations.Count;
        }

        public async void CmdAdditionalDmodsAddNew(object? parameter = null) {
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return;

            try {
                this.IsBusy = true;
                
                OpenFolderDialog ofd = new OpenFolderDialog {
                    Directory = LocationHelper.AppBaseDirectory,
                };

                string? result = await ofd.ShowAsync(this.ParentWindow);
                if (result != null) {
                    this.AdditionalDmodLocations.Add(result);
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            } finally {
                this.IsBusy = false;
            }
        }

        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdAdditionalDmodsAddNew(object? parameter = null) {
            // general conditions
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return false;
            // specific conditions
            return true;
        }
        
        #endregion
        
        #region COMMANDS - THEME

        public void CmdSetApplicationTheme(object? parameter = null) {
            if (parameter is string themeName) {
                if (Enum.TryParse(themeName, out ApplicationTheme themeValue )) {
                    this.ThemeName = themeValue;
                }
            } else if (parameter is ApplicationTheme themeValue) {
                this.ThemeName = themeValue;
            }
        }
        
        public bool CanCmdSetApplicationTheme(object? parameter = null) {
            if (parameter is string themeName) {
                if (Enum.TryParse(themeName, out ApplicationTheme themeValue )) {
                    return true;
                }
            } else if (parameter is ApplicationTheme) {
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region COMMANDS - GAME EXE

        //
        // Game exe paths
        //
        public void CmdGameExeRemoveSelected(object? parameter = null) {
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return;

            if (this.ActiveGameExeIndex >= 0 && this.ActiveGameExeIndex < this.GameExePaths.Count) {
                this.GameExePaths.RemoveAt(this.ActiveGameExeIndex);
            }
        }
        
        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(ActiveGameExeIndex))]
        [DependsOn(nameof(GameExePaths))]
        public bool CanCmdGameExeRemoveSelected(object? parameter = null) {
            // general conditions
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return false;
            // specific conditions
            return this.ActiveGameExeIndex >= 0 && this.ActiveGameExeIndex < this.GameExePaths.Count;
        }


        public async void CmdGameExeAddNew(object? parameter = null) {
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return;

            try {
                this.IsBusy = true;

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Directory = LocationHelper.AppBaseDirectory;
                ofd.AllowMultiple = false;

#if PLATF_WINDOWS
                // on windows, Dink executables need to have .exe extension...
                ofd.Filters = new List<FileDialogFilter>() {
                    new FileDialogFilter() {
                        Name = Localizer.Instance[@"Generic/FileTypeExecutable"],
                        Extensions = new List<string>() { "exe" },
                    },
                };
#endif

                string[]? result = await ofd.ShowAsync(this.ParentWindow);
                if (result == null) return;
                
                // result is ok, add it
                string file = result[0];
                this.GameExePaths.Add(file);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            } finally {
                this.IsBusy = false;
            }
        }

        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdGameExeAddNew(object? parameter = null) {
            // general conditions
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return false;
            // specific conditions
            return true;
        }
        
        #endregion
        
        #region COMMANDS - EDITOR EXE

        //
        // Editor exe paths
        //
        public void CmdEditorExeRemoveSelected(object? parameter = null) {
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return;

            if (this.ActiveEditorExeIndex >= 0 && this.ActiveEditorExeIndex < this.EditorExePaths.Count) {
                this.EditorExePaths.RemoveAt(this.ActiveEditorExeIndex);
            }
        }
        
        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(IsBusy))]
        [DependsOn(nameof(ActiveEditorExeIndex))]
        [DependsOn(nameof(EditorExePaths))]
        public bool CanCmdEditorExeRemoveSelected(object? parameter = null) {
            // general conditions
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return false;
            // specific conditions
            return this.ActiveEditorExeIndex >= 0 && this.ActiveEditorExeIndex < this.EditorExePaths.Count;
        }


        public async void CmdEditorExeAddNew(object? parameter = null) {
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return;

            try {
                this.IsBusy = true;

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Directory = LocationHelper.AppBaseDirectory;
                ofd.AllowMultiple = false;

#if PLATF_WINDOWS
                // on windows, Dink executables need to have .exe extension...
                ofd.Filters = new List<FileDialogFilter>() {
                    new FileDialogFilter() {
                        Name = Localizer.Instance[@"Generic/FileTypeExecutable"],
                        Extensions = new List<string>() { "exe" },
                    },
                };
#endif

                string[]? result = await ofd.ShowAsync(this.ParentWindow);
                if (result == null) return;
                
                // result is ok, add it
                string file = result[0];
                this.EditorExePaths.Add(file);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            } finally {
                this.IsBusy = false;
            }
        }

        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(ParentWindow))]
        [DependsOn(nameof(IsBusy))]
        public bool CanCmdEditorExeAddNew(object? parameter = null) {
            // general conditions
            if (this.Configuration == null ||
                this.ParentWindow == null ||
                this.IsBusy ) return false;
            // specific conditions
            return true;
        }
        
        #endregion
    }
}
