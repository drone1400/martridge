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
            set {
                if (this.RaiseAndSetIfChanged(ref this._overrideDefaultWineEnvVarDefinitions, value)) {
                    this.RefreshCanFullyEditProperties();
                }
            }
        }
        private bool _overrideDefaultWineEnvVarDefinitions = false;

        public ObservableCollection<SettingsEnvironmentVariableViewModel> EnvironmentVariables {
            get => this._environmentVariables;
            private set {
                this.ClearEvents();
                this.RaiseAndSetIfChanged(ref this._environmentVariables, value);
                this.InitEvents();
            }
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

            List<ConfigDataEnvironmentVariable> dictionary = ConfigWine.GetDefaultWineEnvVars();
            foreach (ConfigDataEnvironmentVariable envVar in dictionary) {
                this._isDefaultEnvVar[envVar.Key ?? string.Empty] = true;
            }
            
            this._wineVerChangedTimer.Elapsed += this.WineVerChangedTimerOnElapsed;
        }

        public void InitializeFromConfig(ConfigWine data) {
            this.EnvironmentVariables.Clear();
            this.UseWine = data.UseWine;
            this.OverrideDefaultWineEnvVarDefinitions = data.OverrideDefaultWineEnvVarDefinitions;
            
            ObservableCollection<SettingsEnvironmentVariableViewModel> envVars = new ObservableCollection<SettingsEnvironmentVariableViewModel>();
            
            foreach (var envVar in data.EnvVars) {
                var newVm = new SettingsEnvironmentVariableViewModel(envVar);
                envVars.Add(newVm);
                newVm.CanFullyEdit = 
                    this._isDefaultEnvVar.TryGetValue(newVm.Key, out bool isDefault) == false 
                    || isDefault == false
                    || this._overrideDefaultWineEnvVarDefinitions;
            }
            
            this.EnvironmentVariables = envVars;
        }
        
        public void InitializeFromConfig(ConfigDataWine data) {
            this.EnvironmentVariables.Clear();
            this.UseWine = data.UseWine ?? true;
            this.OverrideDefaultWineEnvVarDefinitions = data.OverrideDefaultWineEnvVarDefinitions ?? false;
            
            ObservableCollection<SettingsEnvironmentVariableViewModel> envVars = new ObservableCollection<SettingsEnvironmentVariableViewModel>();

            if (data.EnvironmentVariables != null) {
                foreach (var envVar in data.EnvironmentVariables) {
                    var newVm = new SettingsEnvironmentVariableViewModel(envVar);
                    envVars.Add(newVm);
                    newVm.CanFullyEdit =
                        this._isDefaultEnvVar.TryGetValue(newVm.Key, out bool isDefault) == false
                        || isDefault == false
                        || this._overrideDefaultWineEnvVarDefinitions;
                }
            }

            this.EnvironmentVariables = envVars;
        }

        private void RefreshCanFullyEditProperties() {
            foreach (var vm in this.EnvironmentVariables) {
                vm.CanFullyEdit = 
                    this._isDefaultEnvVar.TryGetValue(vm.Key, out bool isDefault) == false 
                    || isDefault == false
                    || this._overrideDefaultWineEnvVarDefinitions;
            }
        }

        public void CopyValuesFrom(List<ConfigDataEnvironmentVariable> envVars) {
            Dictionary<string, SettingsEnvironmentVariableViewModel> temp = new Dictionary<string, SettingsEnvironmentVariableViewModel>();
            foreach (SettingsEnvironmentVariableViewModel x in this.EnvironmentVariables) {
                temp[x.Key] = x;
            }
            foreach (var x in envVars) {
                if (temp.TryGetValue(x.Key ?? string.Empty, out SettingsEnvironmentVariableViewModel? vm)) {
                    // modifying existing env var value only
                    vm.Value = x.Value ?? string.Empty;
                } else {
                    // add new env var
                    var newVm = new SettingsEnvironmentVariableViewModel(x);
                    temp[newVm.Key] = newVm;
                    this.EnvironmentVariables.Add(newVm);
                    
                }
            }

            this.RefreshCanFullyEditProperties();
        }
        


        private void ClearEvents() {
            foreach (var x in this.EnvironmentVariables) {
                if (x.Key ==  EnvironmentVariableHelper.WINEVERPATH) {
                    x.PropertyChanged -= this.WineVerPathOnPropertyChanged;
                }
            }
            this.EnvironmentVariables.CollectionChanged -= this.EnvironmentVariablesOnCollectionChanged;
        }
        private void InitEvents() {
            foreach (var x in this.EnvironmentVariables) {
                if (x.Key ==  EnvironmentVariableHelper.WINEVERPATH) {
                    x.PropertyChanged += this.WineVerPathOnPropertyChanged;
                }
            }
            this.EnvironmentVariables.CollectionChanged += this.EnvironmentVariablesOnCollectionChanged;
        }
        
        private void WineVerChangedTimerOnElapsed(object? sender, ElapsedEventArgs e) {
            this._wineVerChangedTimer.Stop();
            this.SetWinePathsFromWineVerPath();
        }
        
        private void EnvironmentVariablesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            if (e.OldItems != null) {
                foreach (var x in e.OldItems) {
                    if (x is not SettingsEnvironmentVariableViewModel vm)
                        continue;
                    
                    // remove event for old items
                    if (vm.Key ==  EnvironmentVariableHelper.WINEVERPATH) {
                        vm.PropertyChanged -= this.WineVerPathOnPropertyChanged;
                    } 
                }
            }
            if (e.NewItems != null) {
                foreach (var x in e.NewItems) {
                    if (x is not SettingsEnvironmentVariableViewModel vm)
                        continue;
                    
                    // make sure the property is set properly :P
                    vm.CanFullyEdit = 
                        this._isDefaultEnvVar.TryGetValue(vm.Key, out bool isDefault) == false 
                        || isDefault == false
                        || this._overrideDefaultWineEnvVarDefinitions;
                    
                    if (vm.Key ==  EnvironmentVariableHelper.WINEVERPATH) {
                        // add event for new items
                        vm.PropertyChanged += this.WineVerPathOnPropertyChanged;
                        // trigger an auto-detect based on WINEVERPATH
                        this._wineVerChangedTimer.Stop();
                        this._wineVerChangedTimer.Start();
                    } 
                }
            }
        }

        private void WineVerPathOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            this._wineVerChangedTimer.Stop();
            this._wineVerChangedTimer.Start();
        }
        
        private void SetWinePathsFromWineVerPath() {
            SettingsEnvironmentVariableViewModel? vmWineVerPath = null;
            SettingsEnvironmentVariableViewModel? vmWineDllPath = null;
            SettingsEnvironmentVariableViewModel? vmWineServer = null;
            SettingsEnvironmentVariableViewModel? vmWineLoader = null;
            SettingsEnvironmentVariableViewModel? vmLdLibraryPath = null;

            // detect view models
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
            
            // detect paths
            SteamHelper.AutoDetectWinePathsFromWineVerPath(vmWineVerPath.Value, out string wineBinPath, out string wineLibPath, out string wineDllPath, out string wineServer, out string wineLoader);

            // copy path data where possible
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
                });
            }

            return data;
        }
        
        //
        // CmdAddNewEnvironmentVariable
        //
        public void CmdAddNewEnvironmentVariable(object? parameter) {
            if (this.CanCmdAddNewEnvironmentVariable_Internal(parameter, out string key) == false)
                return;
            
            this.EnvironmentVariables.Add(new SettingsEnvironmentVariableViewModel(key, this.NewEnvVarValue));
            
            this.NewEnvVarName = string.Empty;
            this.NewEnvVarValue = string.Empty;
        }
        private bool CanCmdAddNewEnvironmentVariable_Internal(object? parameter, out string key) {
            // make sure we have a valid name
            key = this.NewEnvVarName.Trim();
            if (string.IsNullOrWhiteSpace(key)) return false;

            // make sure there are no duplicates
            foreach (SettingsEnvironmentVariableViewModel x in this.EnvironmentVariables) {
                if (x.Key == this.NewEnvVarName) return false;
            }

            return true;
        }
        [DependsOn(nameof(this.NewEnvVarName))]
        [DependsOn(nameof(this.NewEnvVarValue))]
        public bool CanCmdAddNewEnvironmentVariable(object? parameter) 
            => this.CanCmdAddNewEnvironmentVariable_Internal(parameter, out string _);
        
        //
        // CmdEnvVarRemoveSelected
        //
        public void CmdEnvVarRemoveSelected(object? parameter) {
            if (this.CanCmdEnvVarRemoveSelected_Internal(parameter) == false)
                return;
            
            this.EnvironmentVariables.RemoveAt(this.SelectedEnvrionmentVariableIndex);
        }
        private bool CanCmdEnvVarRemoveSelected_Internal(object? parameter) {
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
        [DependsOn(nameof(this.OverrideDefaultWineEnvVarDefinitions))]
        [DependsOn(nameof(this.SelectedEnvrionmentVariableIndex))]
        public bool CanCmdEnvVarRemoveSelected(object? parameter)
            => this.CanCmdEnvVarRemoveSelected_Internal(parameter);
        
        //
        // CmdEnvVarSelectedMoveUp
        //
        public void CmdEnvVarSelectedMoveUp(object? parameter) {
            if (this.CanCmdEnvVarSelectedMoveUp_Internal(parameter) == false)
                return;
            
            int oldIndex = this.SelectedEnvrionmentVariableIndex;
            this.EnvironmentVariables.Move(oldIndex, this.SelectedEnvrionmentVariableIndex - 1);
            this.SelectedEnvrionmentVariableIndex = oldIndex - 1;
        }
        private bool CanCmdEnvVarSelectedMoveUp_Internal(object? parameter) {
            if (this.SelectedEnvrionmentVariableIndex < 1 ||
                this.SelectedEnvrionmentVariableIndex >= this.EnvironmentVariables.Count) {
                return false;
            }
            return true;
        }
        [DependsOn(nameof(this.OverrideDefaultWineEnvVarDefinitions))]
        [DependsOn(nameof(this.SelectedEnvrionmentVariableIndex))]
        public bool CanCmdEnvVarSelectedMoveUp(object? parameter)
            => this.CanCmdEnvVarSelectedMoveUp_Internal(parameter);
        
        //
        // CmdEnvVarSelectedMoveDown
        //
        public void CmdEnvVarSelectedMoveDown(object? parameter) {
            if (this.CanCmdEnvVarSelectedMoveDown_Internal(parameter) == false)
                return;
            
            int oldIndex = this.SelectedEnvrionmentVariableIndex;
            this.EnvironmentVariables.Move(oldIndex, this.SelectedEnvrionmentVariableIndex + 1);
            this.SelectedEnvrionmentVariableIndex = oldIndex + 1;
        }
        private bool CanCmdEnvVarSelectedMoveDown_Internal(object? parameter) {
            if (this.SelectedEnvrionmentVariableIndex < 0 ||
                this.SelectedEnvrionmentVariableIndex >= this.EnvironmentVariables.Count - 1) {
                return false;
            }
            return true;
        }
        [DependsOn(nameof(this.OverrideDefaultWineEnvVarDefinitions))]
        [DependsOn(nameof(this.SelectedEnvrionmentVariableIndex))]
        public bool CanCmdEnvVarSelectedMoveDown(object? parameter)
            => this.CanCmdEnvVarSelectedMoveDown_Internal(parameter);
        
        
    }
}
