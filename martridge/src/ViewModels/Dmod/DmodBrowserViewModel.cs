using Avalonia.Metadata;
using Martridge.ViewModels.DinkyGraphics;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Dmod;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using static Martridge.Models.AppLogic;

namespace Martridge.ViewModels.Dmod {
    public class DmodBrowserViewModel : ViewModelBase {
        //
        // Config stuff
        //

        public ConfigGeneral? ConfigurationGeneral {
            get => this._cfgGeneral;
            set {
                if (this._cfgGeneral != null) {
                    this._cfgGeneral.Updated -= this.ConfigurationGeneralUpdated;
                    this._cfgGeneral = null;
                }
                this.RaiseAndSetIfChanged(ref this._cfgGeneral, value);

                if (this._cfgGeneral != null) {
                    this._cfgGeneral.Updated += this.ConfigurationGeneralUpdated;
                    this.LoadFromConfigGeneral();
                }
            }
        }

        private ConfigGeneral? _cfgGeneral = null;

        public ConfigLaunch? ConfigurationLauncher {
            get => this._cfgLauncher;
            set {
                if (this._cfgLauncher != null) {
                    this._cfgLauncher.Updated -= this.ConfigurationLauncherUpdated;
                    this._cfgLauncher = null;
                }
                this._cfgLauncher = value;
                this.RaisePropertyChanged(nameof(this.ConfigurationLauncher));

                if (this._cfgLauncher != null) {
                    this._cfgLauncher.Updated += this.ConfigurationLauncherUpdated;
                    this.LoadFromConfigLauncher();
                }
            }
        }
        private ConfigLaunch? _cfgLauncher = null;

        public AppLogic.LaunchDmodDelegate? DmodLauncher {
            get => this._dmodLauncher;
            set => this.RaiseAndSetIfChanged(ref this._dmodLauncher, value);
        }
        private AppLogic.LaunchDmodDelegate? _dmodLauncher = null;

        public DmodManager? DmodManager {
            get => this._dmodManager;
            set {
                if (this._dmodManager != null) {
                    this._dmodManager.DmodListInitialized -= this.DmodManager_DmodListInitialized;
                    this._dmodManager = null;
                }
                this._dmodManager = value;
                this.RaisePropertyChanged(nameof(this.DmodManager));

                if (this._dmodManager != null) {
                    this._dmodManager.DmodListInitialized += this.DmodManager_DmodListInitialized;
                    this.InitializeDmods();
                }
            }
        }
        private DmodManager? _dmodManager = null;


        //
        // Config properties launcher
        //

        public bool LaunchTrueColor {
            get => this._launchTrueColor;
            set => this.RaiseAndSetIfChanged(ref this._launchTrueColor, value);
        }
        private bool _launchTrueColor = false;

        public bool LaunchWindowed {
            get => this._launchWindowed;
            set => this.RaiseAndSetIfChanged(ref this._launchWindowed, value);
        }
        private bool _launchWindowed = true;

        public bool LaunchSound {
            get => this._launchSound;
            set => this.RaiseAndSetIfChanged(ref this._launchSound, value);
        }
        private bool _launchSound = true;

        public bool LaunchJoystick {
            get => this._launchJoystick;
            set => this.RaiseAndSetIfChanged(ref this._launchJoystick, value);
        }
        private bool _launchJoystick = true;

        public bool LaunchDebug {
            get => this._launchDebug;
            set => this.RaiseAndSetIfChanged(ref this._launchDebug, value);
        }
        private bool _launchDebug = false;

        public bool LaunchV107Mode {
            get => this._launchV107Mode;
            set => this.RaiseAndSetIfChanged(ref this._launchV107Mode, value);
        }
        private bool _launchV107Mode = false;

        //
        // Other stuff
        //
        
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



        public string? DmodSearchString {
            get => this._dmodSearchString;
            set {
                if (value == "") {
                    this.RaiseAndSetIfChanged(ref this._dmodSearchString, null);
                } else {
                    this.RaiseAndSetIfChanged(ref this._dmodSearchString, value);
                }
            }
        }
        private string? _dmodSearchString = null;
        private Timer _dmodSearchTimer;

        public ObservableCollection<DmodDefinition> DmodDefinitionsFiltered {
            get => this._dmodDefinitionsFiltered;
            set => this.RaiseAndSetIfChanged(ref this._dmodDefinitionsFiltered, value);
        }
        private ObservableCollection<DmodDefinition> _dmodDefinitionsFiltered = new ObservableCollection<DmodDefinition>();
        public ObservableCollection<DmodDefinition> DmodDefinitions { get => this._dmodDefinitions; }
        private ObservableCollection<DmodDefinition> _dmodDefinitions = new ObservableCollection<DmodDefinition>();

        public DmodDefinition? SelectedDmodDefinition {
            get => this._selectedDmodDefinition;
            set => this.RaiseAndSetIfChanged(ref this._selectedDmodDefinition, value);
        }
        private DmodDefinition? _selectedDmodDefinition;
        
        public DmodLocalizationDefinition? SelectedLocalization {
            get => this._selectedLocalization;
            set => this.RaiseAndSetIfChanged(ref this._selectedLocalization, value);
        }
        private DmodLocalizationDefinition? _selectedLocalization;

        public bool DmodDefinitionsFilteredHasItems {
            get => this.DmodDefinitionsFiltered.Count > 0;
        }

        public bool DmodLauncherWaitingForDelay {
            get => this._dmodLauncherWaitingForDelay;
            set => this.RaiseAndSetIfChanged(ref this._dmodLauncherWaitingForDelay, value);
        }
        private bool _dmodLauncherWaitingForDelay = false;

        public bool GameExeFound {
            get => this._gameExeFound;
        }
        private bool _gameExeFound = false;
        
        public bool DmodsFound {
            get => this._dmodsFound;
        }
        private bool _dmodsFound = false;

        private Timer _dmodLauncherDelay;
        private double _dmodLauncherDelayInterval = 5000;

        public DmodBrowserViewModel() {
            this._dmodLauncherDelay = new Timer();
            this._dmodLauncherDelay.Interval = this._dmodLauncherDelayInterval;
            this._dmodLauncherDelay.Elapsed += this.DmodLauncherDelay_Elapsed;

            this._dmodSearchTimer = new Timer() {
                Interval = 200,
                AutoReset = true,
            };
            this._dmodSearchTimer.Elapsed += DmodSearchTimerOnElapsed;
            
            this.PropertyChanged += OnPropertyChanged;
        }

        //
        // Configuration stuff
        //
        private void DmodManager_DmodListInitialized(object? sender, EventArgs e) {
            this.InitializeDmods();
        }

        private void ConfigurationGeneralUpdated(object? sender, EventArgs e) {
            this.LoadFromConfigGeneral();
        }

        private void ConfigurationLauncherUpdated(object? sender, EventArgs e) {
            this.LoadFromConfigLauncher();
        }

        private void LoadFromConfigGeneral() {
            if (this._cfgGeneral == null) { return; }

            ObservableCollection<string> list = new ObservableCollection<string>();
            foreach (string str in this._cfgGeneral.GameExePaths) {
                FileInfo finfo = new FileInfo(str);
                if (finfo.Exists) {
                    list.Add(str);
                }
            }

            this.ActiveGameExeIndex = -1;
            this.GameExePaths = list;
            this.ActiveGameExeIndex = this._cfgGeneral.ActiveGameExeIndex;

            this.RaisePropertyChanged(nameof(this.GameExeFound));

            this._gameExeFound = list.Count > 0;

            this.RaisePropertyChanged(nameof(this.GameExeFound));
        }

        private void SaveToConfigGeneral() {
            if (this._cfgGeneral == null) { return; }

            if (this.ActiveGameExeIndex != this._cfgGeneral.ActiveGameExeIndex) {
                this._cfgGeneral.ActiveGameExeIndex = this.ActiveGameExeIndex;
            }
        }

        private void LoadFromConfigLauncher() {
            if (this._cfgLauncher == null) { return; }

            this.LaunchTrueColor = this._cfgLauncher.TrueColor;
            this.LaunchWindowed = this._cfgLauncher.Windowed;
            this.LaunchSound = this._cfgLauncher.Sound;
            this.LaunchJoystick = this._cfgLauncher.Joystick;
            this.LaunchDebug = this._cfgLauncher.Debug;
            this.LaunchV107Mode = this._cfgLauncher.V107Mode;
        }

        private void SaveToConfigLauncher() {
            this._cfgLauncher?.UpdateAll(
                this.LaunchTrueColor,
                this.LaunchWindowed,
                this.LaunchSound,
                this.LaunchJoystick,
                this.LaunchDebug,
                this.LaunchV107Mode);
        }

        //
        // Other stuff
        //

        private void InitializeDmods() {
            this.DmodDefinitions.Clear();
            if (this._dmodManager == null) { return; }

            foreach (DmodFileDefinition dfd in this._dmodManager.DmodList) {
                this.DmodDefinitions.Add(new DmodDefinition(dfd));
            }

            this.DmodDefinitionsFiltered = this.DmodDefinitions;
            this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));

            if (this.DmodDefinitions.Count > 0) {
                this.SelectedDmodDefinition = null;
                this._dmodsFound = true;
                this.RaisePropertyChanged(nameof(this.GameExeFound));
            } else {
                this._dmodsFound = false;
            }
        }

        private void DmodSearchTimerOnElapsed(object? sender, ElapsedEventArgs e) {
            this._dmodSearchTimer.Stop();
            
            if (this.DmodSearchString == null) {
                this.DmodDefinitionsFiltered = this.DmodDefinitions;
                this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));
                return;
            }
            
            if (this.DmodSearchString.Length >= 2) {
                string searchStr = this.DmodSearchString.ToLowerInvariant();
                
                var filtered = this._dmodDefinitions.Where(definition => definition.Name?.ToLowerInvariant().Contains(searchStr) == true );
                this.DmodDefinitionsFiltered = new ObservableCollection<DmodDefinition>(filtered);
                this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));
            }
        }
        
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.DmodSearchString)) {
                if (this._dmodSearchTimer.Enabled == false) {
                    this._dmodSearchTimer.Start();
                }
            }
        }

        public void CmdRefreshDmods(object? parameter = null) {
            if (this.DmodManager == null) { return; }
            this.DmodSearchString = null;
            this.InitializeDmods();
        }

        [DependsOn(nameof(DmodManager))]
        public bool CanCmdRefreshDmods(object? parameter = null) {
            if (this.DmodManager == null) { return false; }
            return true;
        }
     
        private void DmodLauncherDelay_Elapsed(object? sender, ElapsedEventArgs e) {
            this._dmodLauncherDelay.Stop();
            this.DmodLauncherWaitingForDelay = false;
        }

        public void CmdLaunchDmod(object? parameter = null) {
            if (!this.GameExeFound) { return; }
            if (this.ConfigurationGeneral == null) { return; }
            if (this.ConfigurationLauncher == null) { return; }
            if (this.DmodLauncher == null) { return; }
            if (this.DmodManager == null) { return; }
            if (this.SelectedDmodDefinition == null) { return; }

            this.DmodLauncherWaitingForDelay = true;
            this.SaveToConfigLauncher();
            this.SaveToConfigGeneral();

            this.DmodLauncher(this.SelectedDmodDefinition.Path, this.SelectedLocalization?.CultureInfo?.Name);
            
            this._dmodLauncherDelay.Start();
        }

        [DependsOn(nameof(GameExeFound))]
        [DependsOn(nameof(ConfigurationGeneral))]
        [DependsOn(nameof(ConfigurationLauncher))]
        [DependsOn(nameof(DmodLauncher))]
        [DependsOn(nameof(DmodManager))]
        [DependsOn(nameof(SelectedDmodDefinition))]
        public bool CanCmdLaunchDmod(object? parameter = null) {
            try {
                if (!this.GameExeFound) { return false; }
                if (this.ConfigurationGeneral == null) { return false; }
                if (this.ConfigurationLauncher == null) { return false; }
                if (this.DmodLauncher == null) { return false; }
                if (this.DmodManager == null) { return false; }
                if (this.SelectedDmodDefinition == null) { return false; }

                if (this.ConfigurationGeneral?.GameExePaths.Count == 0) { return false; }

                string exePath = this.ConfigurationGeneral.GameExePaths[this.ActiveGameExeIndex];
                string dmodPath = this.SelectedDmodDefinition.Path;
                FileInfo finfo = new FileInfo(exePath);
                DirectoryInfo dinfo = new DirectoryInfo(dmodPath);
                if (finfo.Exists == false || dinfo.Exists == false) { return false; }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex);
                return false;
            }

            return true;
        }
    }
}
