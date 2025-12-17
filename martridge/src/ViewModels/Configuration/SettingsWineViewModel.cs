using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Timers;
using System.Windows.Input;
using Avalonia.Metadata;
using Martridge.Models.Configuration.Generic;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Configuration.LaunchExtension;
using Martridge.Models.Configuration.LaunchExtension.FileData;
using Martridge.Models.Localization;
using Martridge.Models.Steam;
using Martridge.ViewModels.DinkyAlerts;
using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public class SettingsWineViewModel : ViewModelBase {

        public bool EnableWine {
            get => this._enableWine;
            set => this.RaiseAndSetIfChanged(ref this._enableWine, value);
        }
        private bool _enableWine = false;
        
        public ObservableCollection<SettingsEnvVarViewModelBase> EnvironmentVariables {
            get => this._environmentVariables;
            private set {
                this.ClearEvents();
                this.RaiseAndSetIfChanged(ref this._environmentVariables, value);
                this.InitEvents();
            }
        }
        private ObservableCollection<SettingsEnvVarViewModelBase> _environmentVariables = new ObservableCollection<SettingsEnvVarViewModelBase>();

        public int SelectedEnvrionmentVariableIndex {
            get => this._selectedEnvrionmentVariableIndex;
            set => this.RaiseAndSetIfChanged(ref this._selectedEnvrionmentVariableIndex, value);
        }
        private int _selectedEnvrionmentVariableIndex = -1;
        
        public string NewEnvVarKey {
            get => this._newEnvVarKey;
            set => this.RaiseAndSetIfChanged(ref this._newEnvVarKey, value);
        }
        private string _newEnvVarKey = string.Empty;

        public string NewEnvVarValue {
            get => this._newEnvVarValue;
            set => this.RaiseAndSetIfChanged(ref this._newEnvVarValue, value);
        }
        private string _newEnvVarValue = string.Empty;

        public List<ConfigEnvVarMode> NewEnvVarModes { get; } = new List<ConfigEnvVarMode>() {
            ConfigEnvVarMode.Normal,
            ConfigEnvVarMode.AppendStart,
            ConfigEnvVarMode.AppendEnd,
        };

        public ConfigEnvVarMode NewEnvVarMode {
            get => this._newEnvVarMode;
            set => this.RaiseAndSetIfChanged(ref this._newEnvVarMode, value);
        }
        private ConfigEnvVarMode _newEnvVarMode = ConfigEnvVarMode.Normal;

        public string NewEnvVarSeparator {
            get => this._newEnvVarSeparator;
            set => this.RaiseAndSetIfChanged(ref this._newEnvVarSeparator, value);
        }
        private string _newEnvVarSeparator = ":";

        public ICommand AutoDetectWineCommand {
            get => this._autoDetectWineCommand;
            set => this.RaiseAndSetIfChanged(ref this._autoDetectWineCommand, value);
        }
        private ICommand _autoDetectWineCommand;
        
        private Timer _wineVerChangedTimer = new Timer() {
            Interval = 330,
            AutoReset = false,
        };
        
        public SettingsWineViewModel() {
            this._wineVerChangedTimer.Elapsed += this.WineVerChangedTimerOnElapsed;

            this._autoDetectWineCommand = ReactiveCommand.Create(this.CmdAutoDetectInternal);
        }

        public void InitializeFromConfig(ConfigWine data) {
            this.EnvironmentVariables.Clear();
            this.EnableWine = data.EnableWine;
            
            ObservableCollection<SettingsEnvVarViewModelBase> envVars = new ObservableCollection<SettingsEnvVarViewModelBase>();
            
            foreach (var envVar in data.EnvVars) {
                switch (envVar.Mode) {
                    case ConfigEnvVarMode.Normal:
                        envVars.Add(new SettingsEnvVarViewModel(envVar.Key, envVar.Value, envVar.IsEnabled));
                        break;
                    case ConfigEnvVarMode.AppendStart:
                        envVars.Add(new SettingsEnvVarAppendStartViewModel(envVar.Key, envVar.Value, envVar.IsEnabled, envVar.AppendSeparator));
                        break;
                    case ConfigEnvVarMode.AppendEnd:
                        envVars.Add(new SettingsEnvVarAppendEndViewModel(envVar.Key, envVar.Value, envVar.IsEnabled, envVar.AppendSeparator));
                        break;
                }
            }
            
            this.EnvironmentVariables = envVars;
        }

        private void InitializeEnvironmentVariables(List<ConfigDataEnvironmentVariable> data) {
            this.EnvironmentVariables.Clear();
            ObservableCollection<SettingsEnvVarViewModelBase> envVars = new ObservableCollection<SettingsEnvVarViewModelBase>();

            foreach (var envVar in data) {
                if (string.IsNullOrWhiteSpace(envVar.Key))
                    continue;
                switch (envVar.Mode) {
                    default:
                    case nameof(ConfigEnvVarMode.Normal):
                        envVars.Add(new SettingsEnvVarViewModel(
                            envVar.Key, envVar.Value ?? string.Empty, envVar.IsEnabled ?? false));
                        break;
                    case nameof(ConfigEnvVarMode.AppendStart):
                        envVars.Add(new SettingsEnvVarAppendStartViewModel(
                            envVar.Key, envVar.Value ?? string.Empty, envVar.IsEnabled ?? false, envVar.AppendSeparator ?? string.Empty));
                        break;
                    case nameof(ConfigEnvVarMode.AppendEnd):
                        envVars.Add(new SettingsEnvVarAppendEndViewModel(
                            envVar.Key, envVar.Value ?? string.Empty, envVar.IsEnabled ?? false, envVar.AppendSeparator ?? string.Empty));
                        break;
                }
            }

            this.EnvironmentVariables = envVars;
        }
        
        public void InitializeFromConfig(ConfigDataWine data) {
            
            this.EnableWine = data.EnableWine ?? true;

            if (data.EnvironmentVariables != null) {
                this.InitializeEnvironmentVariables(data.EnvironmentVariables);
            } else {
                this.EnvironmentVariables = new ObservableCollection<SettingsEnvVarViewModelBase>();
            }
        }

        public void CopyValuesFrom(List<ConfigDataEnvironmentVariable> envVars) {
            List<SettingsEnvVarViewModelBase> pending = new List<SettingsEnvVarViewModelBase>();
            foreach (var envVar in envVars) {
                if (string.IsNullOrWhiteSpace(envVar.Key))
                    continue;
                switch (envVar.Mode) {
                    default:
                    case nameof(ConfigEnvVarMode.Normal):
                        pending.Add(new SettingsEnvVarViewModel(
                            envVar.Key, envVar.Value ?? string.Empty, envVar.IsEnabled ?? false));
                        break;
                    case nameof(ConfigEnvVarMode.AppendStart):
                        pending.Add(new SettingsEnvVarAppendStartViewModel(
                            envVar.Key, envVar.Value ?? string.Empty, envVar.IsEnabled ?? false, envVar.AppendSeparator ?? string.Empty));
                        break;
                    case nameof(ConfigEnvVarMode.AppendEnd):
                        pending.Add(new SettingsEnvVarAppendEndViewModel(
                            envVar.Key, envVar.Value ?? string.Empty, envVar.IsEnabled ?? false, envVar.AppendSeparator ?? string.Empty));
                        break;
                }
            }
            
            Dictionary<string, int> temp = new Dictionary<string, int>();
            for (int i = 0; i < this.EnvironmentVariables.Count; i++) {
                temp[this.EnvironmentVariables[i].Key] = i;
            }

            foreach (var envVar in pending) {
                if (temp.TryGetValue(envVar.Key, out int index)) {
                    // have to replace old view model
                    this.EnvironmentVariables.RemoveAt(index);
                    this.EnvironmentVariables.Insert(index, envVar);
                } else {
                    this.EnvironmentVariables.Add(envVar);
                }
            }
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
                    if (x is not SettingsEnvVarViewModel vm)
                        continue;
                    
                    // remove event for old items
                    if (vm.Key ==  EnvironmentVariableHelper.WINEVERPATH) {
                        vm.PropertyChanged -= this.WineVerPathOnPropertyChanged;
                    } 
                }
            }
            if (e.NewItems != null) {
                foreach (var x in e.NewItems) {
                    if (x is not SettingsEnvVarViewModel vm)
                        continue;
                    
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
            SettingsEnvVarViewModelBase? vmWineVerPath = null;
            SettingsEnvVarViewModelBase? vmWineDllPath = null;
            SettingsEnvVarViewModelBase? vmWineServer = null;
            SettingsEnvVarViewModelBase? vmWineLoader = null;
            SettingsEnvVarViewModelBase? vmLdLibraryPath = null;

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
            data.EnableWine = this.EnableWine;
            data.EnvironmentVariables = new List<ConfigDataEnvironmentVariable>();
            foreach (SettingsEnvVarViewModelBase x in this.EnvironmentVariables) {
                var envVar = new ConfigDataEnvironmentVariable() {
                    Key = x.Key,
                    Value = x.Value,
                    IsEnabled = x.IsEnabled,
                    Mode = nameof(ConfigEnvVarMode.Normal),
                };
                
                if (x is SettingsEnvVarAppendEndViewModel xEnd) {
                    envVar.Mode =  nameof(ConfigEnvVarMode.AppendEnd);
                    envVar.AppendSeparator = xEnd.AppendSeparator;
                } else if (x is SettingsEnvVarAppendStartViewModel xStart) {
                    envVar.Mode =  nameof(ConfigEnvVarMode.AppendStart);
                    envVar.AppendSeparator = xStart.AppendSeparator;
                }
                
                data.EnvironmentVariables.Add(envVar);
            }

            return data;
        }
        
        //
        // CmdResetToDefault
        //
        public async void CmdResetToDefault(object? parameter) {
            string title = Localizer.Instance["SettingsWineView/ResetToDefault/ConfirmTitle"];
            string body = Localizer.Instance["SettingsWineView/ResetToDefault/ConfirmBody"];
            
            var result = await DinkyAlert.ShowDinkyAlert(
                title, body, AlertResults.Yes | AlertResults.Cancel, AlertType.Warning);

            if (result != AlertResults.Yes)
                return;
            
            this.EnvironmentVariables.Clear();

            var defaults = ConfigWine.GetDefaultWineEnvVars();
            this.InitializeEnvironmentVariables(defaults);
        }
        
        //
        // CmdAutoDetect
        //
        private void CmdAutoDetectInternal() {
            ConfigWine.AutoDetectDefaultWine(out List<ConfigDataEnvironmentVariable>? envVars);
            if (envVars is null)
                return;
            
            this.CopyValuesFrom(envVars);
        }
        
        //
        // CmdAddNewEnvironmentVariable
        //
        public void CmdAddNewEnvironmentVariable(object? parameter) {
            if (this.CanCmdAddNewEnvironmentVariable_Internal(parameter, out string key) == false)
                return;

            SettingsEnvVarViewModelBase vm = this.NewEnvVarMode switch {
                ConfigEnvVarMode.AppendStart => new SettingsEnvVarAppendStartViewModel(
                    key,
                    this.NewEnvVarValue,
                    true,
                    this.NewEnvVarSeparator),
                ConfigEnvVarMode.AppendEnd => new SettingsEnvVarAppendEndViewModel(
                    key,
                    this.NewEnvVarValue,
                    true,
                    this.NewEnvVarSeparator),
                _  => new SettingsEnvVarViewModel(
                    key,
                    this.NewEnvVarValue,
                    true),
            };
            
            this.EnvironmentVariables.Add(vm);
            
            this.NewEnvVarKey = string.Empty;
            this.NewEnvVarValue = string.Empty;
        }
        private bool CanCmdAddNewEnvironmentVariable_Internal(object? _, out string key) {
            // make sure we have a valid name
            key = this.NewEnvVarKey.Trim();
            if (string.IsNullOrWhiteSpace(key)) return false;

            // make sure there are no duplicates
            foreach (SettingsEnvVarViewModelBase x in this.EnvironmentVariables) {
                if (x.Key == this.NewEnvVarKey) return false;
            }

            return true;
        }
        [DependsOn(nameof(this.NewEnvVarKey))]
        [DependsOn(nameof(this.NewEnvVarValue))]
        [DependsOn(nameof(this.NewEnvVarMode))]
        [DependsOn(nameof(this.NewEnvVarSeparator))]
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
        private bool CanCmdEnvVarRemoveSelected_Internal(object? _) {
            if (this.SelectedEnvrionmentVariableIndex < 0 ||
                this.SelectedEnvrionmentVariableIndex >= this.EnvironmentVariables.Count) {
                return false;
            }
            return true;
        }

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
        private bool CanCmdEnvVarSelectedMoveUp_Internal(object? _) {
            if (this.SelectedEnvrionmentVariableIndex < 1 ||
                this.SelectedEnvrionmentVariableIndex >= this.EnvironmentVariables.Count) {
                return false;
            }
            return true;
        }

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
        private bool CanCmdEnvVarSelectedMoveDown_Internal(object? _) {
            if (this.SelectedEnvrionmentVariableIndex < 0 ||
                this.SelectedEnvrionmentVariableIndex >= this.EnvironmentVariables.Count - 1) {
                return false;
            }
            return true;
        }
        [DependsOn(nameof(this.SelectedEnvrionmentVariableIndex))]
        public bool CanCmdEnvVarSelectedMoveDown(object? parameter)
            => this.CanCmdEnvVarSelectedMoveDown_Internal(parameter);
        
        
    }
}
