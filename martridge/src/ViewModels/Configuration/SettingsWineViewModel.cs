using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Timers;
using Avalonia.Metadata;
using Martridge.Models.Configuration.Generic;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Configuration.LaunchExtension;
using Martridge.Models.Configuration.LaunchExtension.FileData;
using Martridge.Models.Steam;
using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public class SettingsWineViewModel : ViewModelBase {

        public bool UseWine {
            get => this._useWine;
            set => this.RaiseAndSetIfChanged(ref this._useWine, value);
        }
        private bool _useWine = false;

        public bool OverrideDefaultWineEnvVarDefinitions {
            get => this._overrideDefaultWineEnvVarDefinitions;
            set => this.RaiseAndSetIfChanged(ref this._overrideDefaultWineEnvVarDefinitions, value);
        }
        private bool _overrideDefaultWineEnvVarDefinitions = false;

        public ObservableCollection<SettingsEnvironmentVariableViewModel> EnvironmentVariables {
            get => this._environmentVariables;
            private set => this.RaiseAndSetIfChanged(ref this._environmentVariables, value);
        }
        private ObservableCollection<SettingsEnvironmentVariableViewModel> _environmentVariables = new ObservableCollection<SettingsEnvironmentVariableViewModel>();

        public int SelectedEnvrionmentVariableIndex {
            get => this._selectedEnvrionmentVariableIndex;
            set => this.RaiseAndSetIfChanged(ref this._selectedEnvrionmentVariableIndex, value);
        }
        private int _selectedEnvrionmentVariableIndex = -1;

        public string NewEnvVarName {
            get => this._newEnvVarName;
            set => this.RaiseAndSetIfChanged(ref this._newEnvVarName, value);
        }
        private string _newEnvVarName = string.Empty;

        public string NewEnvVarValue {
            get => this._newEnvVarValue;
            set => this.RaiseAndSetIfChanged(ref this._newEnvVarValue, value);
        }
        private string _newEnvVarValue = string.Empty;

        private Dictionary<string, bool> _isDefaultEnvVar = new Dictionary<string, bool>();
        
        private Timer _wineVerChangedTimer = new Timer() {
            Interval = 330,
            AutoReset = false,
        };
        
        public SettingsWineViewModel() {

            Dictionary<string, ConfigDataEnvironmentVariable> dictionary = ConfigWine.GetDefaultWineEnvVarsDictionary();
            foreach (KeyValuePair<string, ConfigDataEnvironmentVariable> kvp in  dictionary) {
                this._isDefaultEnvVar[kvp.Key] = true;
            }
            
            this._wineVerChangedTimer.Elapsed += this.WineVerChangedTimerOnElapsed;
        }

        public void InitializeFromConfig(ConfigWine data) {
            this.UseWine = data.UseWine;
            this.OverrideDefaultWineEnvVarDefinitions = data.OverrideDefaultWineEnvVarDefinitions;
            
            ObservableCollection<SettingsEnvironmentVariableViewModel> envVars = new ObservableCollection<SettingsEnvironmentVariableViewModel>();
            
            foreach (KeyValuePair<string, ConfigEnvironmentVariable> envVar in data.EnvironmentVariables) {
                envVars.Add(new SettingsEnvironmentVariableViewModel(envVar.Value));
            }
            
            this.EnvironmentVariables = envVars;
        }

        public void InitializeFromEnvVars(Dictionary<string, ConfigDataEnvironmentVariable> envVars) {
            Dictionary<string, SettingsEnvironmentVariableViewModel> temp = new Dictionary<string, SettingsEnvironmentVariableViewModel>();
            foreach (var x in this.EnvironmentVariables) {
                temp[x.Key] = x;
            }
            foreach (KeyValuePair<string, ConfigDataEnvironmentVariable> kvp in envVars) {
                if (temp.TryGetValue(kvp.Value.Key ?? string.Empty, out SettingsEnvironmentVariableViewModel? vm)) {
                    // modifying existing env var value only
                    vm.Value = kvp.Value.Value ?? string.Empty;
                } else {
                    // add new env var
                    this.EnvironmentVariables.Add(new SettingsEnvironmentVariableViewModel(kvp.Value));
                }
            }
        }
        
        public void InitializeFromConfig(ConfigDataWine data) {
            this.UseWine = data.UseWine ?? true;
            this.OverrideDefaultWineEnvVarDefinitions = data.OverrideDefaultWineEnvVarDefinitions ?? false;
            
            ObservableCollection<SettingsEnvironmentVariableViewModel> envVars = new ObservableCollection<SettingsEnvironmentVariableViewModel>();

            if (data.EnvironmentVariables != null) {
                foreach (ConfigDataEnvironmentVariable envVar in data.EnvironmentVariables) {
                    envVars.Add(new SettingsEnvironmentVariableViewModel(envVar));
                }
            }
            this.ClearEvents();
            this.EnvironmentVariables = envVars;
            this.InitEvents();
        }

        private void ClearEvents() {
            foreach (var x in this.EnvironmentVariables) {
                if (x.Key ==  EnvironmentVariableHelper.WINEVERPATH) {
                    x.PropertyChanged -= this.WineverpathOnPropertyChanged;
                }
            }
            this.EnvironmentVariables.CollectionChanged -= this.EnvironmentVariablesOnCollectionChanged;
        }
        private void InitEvents() {
            foreach (var x in this.EnvironmentVariables) {
                if (x.Key ==  EnvironmentVariableHelper.WINEVERPATH) {
                    x.PropertyChanged += this.WineverpathOnPropertyChanged;
                }
            }
            this.EnvironmentVariables.CollectionChanged += this.EnvironmentVariablesOnCollectionChanged;
        }
        
        private void WineVerChangedTimerOnElapsed(object? sender, ElapsedEventArgs e) {
            this._wineVerChangedTimer.Stop();
            this.SetWinePathsFromWineVerPath();
        }
        
        private void EnvironmentVariablesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            foreach (var x in e.NewItems) {
                if (x is SettingsEnvironmentVariableViewModel vm && vm.Key ==  EnvironmentVariableHelper.WINEVERPATH) {
                    vm.PropertyChanged += this.WineverpathOnPropertyChanged;
                }
            }
        }

        private void WineverpathOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            this._wineVerChangedTimer.Stop();
            this._wineVerChangedTimer.Start();
        }
        
        private void SetWinePathsFromWineVerPath() {
            SettingsEnvironmentVariableViewModel? vmWineVerPath = null;
            SettingsEnvironmentVariableViewModel? vmWineDllPath = null;
            SettingsEnvironmentVariableViewModel? vmWineServer = null;
            SettingsEnvironmentVariableViewModel? vmWineLoader = null;
            SettingsEnvironmentVariableViewModel? vmLdLibraryPath = null;

            foreach (var x in this.EnvironmentVariables) {
                if (x.Key == EnvironmentVariableHelper.WINEVERPATH) {
                    vmWineVerPath = x;
                } else if (x.Key == EnvironmentVariableHelper.WINEDLLPATH) {
                    vmWineDllPath = x;
                } else if (x.Key == EnvironmentVariableHelper.WINESERVER) {
                    vmWineServer = x;
                } else if (x.Key == EnvironmentVariableHelper.WINELOADER) {
                    vmWineLoader = x;
                } else if (x.Key == EnvironmentVariableHelper.LD_LIBRARY_PATH) {
                    vmLdLibraryPath = x;
                }
            }

            if (vmWineVerPath == null)
                return;
            
            
            SteamHelper.AutoDetectWinePathsFromWineVerPath(vmWineVerPath.Value, out string wineBinPath, out string wineLibPath, out string wineDllPath, out string wineServer, out string wineLoader);

            if (vmWineServer != null && string.IsNullOrWhiteSpace(wineServer) == false) {
                vmWineServer.Value = wineServer;
            }
            
            if (vmWineLoader != null && string.IsNullOrWhiteSpace(wineLoader) == false) {
                vmWineLoader.Value = wineLoader;
            }
            
            if (vmWineDllPath != null && string.IsNullOrWhiteSpace(wineDllPath) == false) {
                vmWineDllPath.Value = wineDllPath;
            }
            
            if (vmLdLibraryPath != null && string.IsNullOrWhiteSpace(wineLibPath) == false &&
                string.IsNullOrWhiteSpace(vmLdLibraryPath.Value)) {
                vmLdLibraryPath.Value = wineLibPath;
            }
        }

        public ConfigDataWine GetConfigData() {
            ConfigDataWine data = new ConfigDataWine();
            data.UseWine = this.UseWine;
            data.OverrideDefaultWineEnvVarDefinitions = this.OverrideDefaultWineEnvVarDefinitions;
            data.EnvironmentVariables = new List<ConfigDataEnvironmentVariable>();
            foreach (SettingsEnvironmentVariableViewModel x in this.EnvironmentVariables) {
                data.EnvironmentVariables.Add(new ConfigDataEnvironmentVariable() {
                    Key = x.Key,
                    Value = x.Value,
                    IsEnabled = x.IsEnabled,
                    IsAppendMode = x.IsAppendMode,
                    IsAppendAtEnd = x.IsAppendAtEnd,
                    AppendSeparator = x.AppendSeparator,
                    Description = x.Description,
                });
            }

            return data;
        }

        public void AddNewEnvironmentVariable(object? parameter) {
            if (string.IsNullOrWhiteSpace(this.NewEnvVarName)) return;

            foreach (SettingsEnvironmentVariableViewModel x in this.EnvironmentVariables) {
                if (x.Key == this.NewEnvVarName) return;
            }
            
            this.EnvironmentVariables.Add(new SettingsEnvironmentVariableViewModel(this.NewEnvVarName, this.NewEnvVarValue));
            
            this.NewEnvVarName = string.Empty;
            this.NewEnvVarValue = string.Empty;
        }
        
        [DependsOn(nameof(this.NewEnvVarName))]
        [DependsOn(nameof(this.NewEnvVarValue))]
        public bool CanAddNewEnvironmentVariable(object? parameter) {
            if (string.IsNullOrWhiteSpace(this.NewEnvVarName)) return false;

            foreach (SettingsEnvironmentVariableViewModel x in this.EnvironmentVariables) {
                if (x.Key == this.NewEnvVarName) return false;
            }

            return true;
        }

        public void RemoveEnvironmentVariable(object? parameter) {
            if (this.SelectedEnvrionmentVariableIndex < 0 ||
                this.SelectedEnvrionmentVariableIndex >= this.EnvironmentVariables.Count) {
                return;
            }
            string key = this.EnvironmentVariables[this.SelectedEnvrionmentVariableIndex].Key;
            if (this.OverrideDefaultWineEnvVarDefinitions == false &&
                this._isDefaultEnvVar.TryGetValue(key, out bool result) && result) {
                return;
            }
            
            
            this.EnvironmentVariables.RemoveAt(this.SelectedEnvrionmentVariableIndex);
        }

        [DependsOn(nameof(this.OverrideDefaultWineEnvVarDefinitions))]
        [DependsOn(nameof(this.SelectedEnvrionmentVariableIndex))]
        public bool CanRemoveEnvironmentVariable(object? parameter) {
            if (this.SelectedEnvrionmentVariableIndex < 0 ||
                this.SelectedEnvrionmentVariableIndex >= this.EnvironmentVariables.Count) {
                return false;
            }
            string key = this.EnvironmentVariables[this.SelectedEnvrionmentVariableIndex].Key;
            if (this.OverrideDefaultWineEnvVarDefinitions == false &&
                this._isDefaultEnvVar.TryGetValue(key, out bool result) && result) {
                return false;
            }
            return true;
        }
    }
}
