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
        
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.SelectedLocalization)) {
                if (this.SelectedLocalization != null &&
                    Localizer.Instance.Language != this.SelectedLocalization.Name) {
                    Localizer.Instance.LoadLanguage(this.SelectedLocalization.Name);
                }
            }
        }

        #region LOAD / SAVE Config

        private void LoadFromConfig() {
            if (this._cfg == null) { return; }


            ObservableCollection<string> listExe = new ObservableCollection<string>();
            foreach (string str in this._cfg.GameExePaths) {
                listExe.Add(str);
            }

            ObservableCollection<string> listDmod = new ObservableCollection<string>();
            foreach (string str in this._cfg.AdditionalDmodLocations) {
                listDmod.Add(str);
            }

            this.ShowLogWindowOnStartup = this._cfg.ShowLogWindowOnStartup;
            this.ShowAdvancedFeatures = this._cfg.ShowAdvancedFeatures;
            this.UseRelativePathForSubfolders = this._cfg.UseRelativePathForSubfolders;
            this.AdditionalDmodLocationsIndex = -1;
            this.ActiveGameExeIndex = -1;
            this.AdditionalDmodLocations = listDmod;
            this.GameExePaths = listExe;
            this.AdditionalDmodLocationsIndex = 0;
            this.ActiveGameExeIndex = this._cfg.ActiveGameExeIndex;
            this.DefaultDmodLocation = this._cfg.DefaultDmodLocation;

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

            List<string> listExe = new List<string>();
            foreach (string str in this.GameExePaths) {
                listExe.Add(str);
            }

            List<string> listDmod = new List<string>();
            foreach (string str in this.AdditionalDmodLocations) {
                listDmod.Add(str);
            }

            this._savedLocalization = Localizer.Instance.Language;

            this.Configuration.UpdateAll(
                this._savedLocalization ?? "en-US",
                this.ShowAdvancedFeatures,
                this.AutoUpdateInstallerList,
                this.ShowLogWindowOnStartup,
                this.UseRelativePathForSubfolders,
                this.ActiveGameExeIndex,
                listExe,
                this.DefaultDmodLocation,
                listDmod
            );
        }
        
        #endregion

        #region COMMANDS

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
    }
}
