using Avalonia.Metadata;
using Martridge.Models.Configuration;
using Martridge.Models.Dmod;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using static Martridge.Models.AppLogic;

namespace Martridge.ViewModels.Dmod {
    public class DmodBrowserViewModel : ViewModelBase {

        #region  CONSTRUCTOR / Initialization
        
        private readonly Timer _dmodLauncherDelay;
        private readonly Timer _dmodSearchTimer;

        /// <summary>
        /// Constructor for Local Dmod Browser / Launcher View Model
        /// </summary>
        public DmodBrowserViewModel() {
            // Launcher Delay Timer
            this._dmodLauncherDelay = new Timer() {
                Interval = 5000,
                AutoReset = true,
            };
            this._dmodLauncherDelay.Elapsed += ( sender,  args) => {
                this._dmodLauncherDelay.Stop();
                this.DmodLauncherWaitingForDelay = false;
            };

            // Dmod Search Timer
            this._dmodSearchTimer = new Timer() {
                Interval = 200,
                AutoReset = true,
            };
            this._dmodSearchTimer.Elapsed += ( sender,  args) => {
                this._dmodSearchTimer.Stop();
                this.InitializeFilteredDmods();
            };
            
            // self properties changed
            this.PropertyChanged += ( sender,  args) => {
                if (args.PropertyName == nameof(this.DmodSearchString)) {
                    if (this._dmodSearchTimer.Enabled == false) {
                        this._dmodSearchTimer.Start();
                    }
                }

                if (args.PropertyName == nameof(this.DmodOrderBy)) {
                    this.InitializeFilteredDmods();
                }
            };
        }
        
        #endregion
        

        #region CONFIGURATION - Launch
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------

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


        // -----------------------------------------------------------------------------------------------------------------------------------
        // Mirrored Launch Configuration Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
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
        
        public bool LaunchUsePathRelativeToGame {
            get => this._launchUsePathRelativeToGame;
            set => this.RaiseAndSetIfChanged(ref this._launchUsePathRelativeToGame, value);
        }
        private bool _launchUsePathRelativeToGame = false;
        
        public bool LaunchUsePathQuotationMarks {
            get => this._launchUsePathQuotationMarks;
            set => this.RaiseAndSetIfChanged(ref this._launchUsePathQuotationMarks, value);
        }
        private bool _launchUsePathQuotationMarks = false;
        
        public string LaunchCustomUserArguments {
            get => this._launchCustomUserArguments;
            set => this.RaiseAndSetIfChanged(ref this._launchCustomUserArguments, value);
        }
        private string _launchCustomUserArguments= "";

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        private void ConfigurationLauncherUpdated(object? sender, EventArgs e) {
            this.LoadFromConfigLauncher();
        }
        
        private void LoadFromConfigLauncher() {
            if (this._cfgLauncher == null) { return; }

            this.LaunchTrueColor = this._cfgLauncher.TrueColor;
            this.LaunchWindowed = this._cfgLauncher.Windowed;
            this.LaunchSound = this._cfgLauncher.Sound;
            this.LaunchJoystick = this._cfgLauncher.Joystick;
            this.LaunchDebug = this._cfgLauncher.Debug;
            this.LaunchV107Mode = this._cfgLauncher.V107Mode;
            this.LaunchUsePathQuotationMarks = this._cfgLauncher.UsePathQuotationMarks;
            this.LaunchUsePathRelativeToGame = this._cfgLauncher.UsePathRelativeToGame;
            this.LaunchCustomUserArguments = this._cfgLauncher.CustomUserArguments;
        }

        private void SaveToConfigLauncher() {
            this._cfgLauncher?.UpdateAll(
                this.LaunchTrueColor,
                this.LaunchWindowed,
                this.LaunchSound,
                this.LaunchJoystick,
                this.LaunchDebug,
                this.LaunchV107Mode,
                this.LaunchUsePathQuotationMarks,
                this.LaunchUsePathRelativeToGame,
                this.LaunchCustomUserArguments);
        }
        
        #endregion

        #region CONFIGURATION - General

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
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

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Mirrored Main Configuration Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
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
        
        public bool GameExeFound {
            get => this._gameExeFound;
            private set => this.RaiseAndSetIfChanged(ref this._gameExeFound, value);
        }
        private bool _gameExeFound = false;
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        private void ConfigurationGeneralUpdated(object? sender, EventArgs e) {
            this.LoadFromConfigGeneral();
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

        #endregion

        #region DMOD Launcher

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------

        public LaunchDmodDelegate? DmodLauncher {
            get => this._dmodLauncher;
            set => this.RaiseAndSetIfChanged(ref this._dmodLauncher, value);
        }
        private LaunchDmodDelegate? _dmodLauncher = null;
        
        public bool DmodLauncherWaitingForDelay {
            get => this._dmodLauncherWaitingForDelay;
            set => this.RaiseAndSetIfChanged(ref this._dmodLauncherWaitingForDelay, value);
        }
        private bool _dmodLauncherWaitingForDelay = false;
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------


        #endregion
        

        #region DMOD List - Properties and methods related to the DMOD list are here
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
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
                    this.InitializeDmodsFromManager();
                }
            }
        }
        private DmodManager? _dmodManager = null;
        
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
        
        public ObservableCollection<DmodDefinition> DmodDefinitions { get => this._dmodDefinitions; }
        private ObservableCollection<DmodDefinition> _dmodDefinitions = new ObservableCollection<DmodDefinition>();
        
        public ObservableCollection<DmodDefinition> DmodDefinitionsFiltered {
            get => this._dmodDefinitionsFiltered;
            set => this.RaiseAndSetIfChanged(ref this._dmodDefinitionsFiltered, value);
        }
        private ObservableCollection<DmodDefinition> _dmodDefinitionsFiltered = new ObservableCollection<DmodDefinition>();
        
        public bool DmodDefinitionsFilteredHasItems {
            get => this.DmodDefinitionsFiltered.Count > 0;
        }
        
        public ObservableCollection<DmodOrderBy> DmodOrderByList { get => this._dmodOrderByList; }
        private ObservableCollection<DmodOrderBy> _dmodOrderByList = new ObservableCollection<DmodOrderBy>() {
            DmodOrderBy.NameAsc,
            DmodOrderBy.NameDesc,
            DmodOrderBy.PathAsc,
            DmodOrderBy.PathDesc,
        };

        public DmodOrderBy DmodOrderBy {
            get => this._dmodOrderBy;
            set => this.RaiseAndSetIfChanged(ref this._dmodOrderBy, value);
        }
        private DmodOrderBy _dmodOrderBy = DmodOrderBy.NameAsc;


        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------

        private void DmodManager_DmodListInitialized(object? sender, EventArgs e) {
            this.InitializeDmodsFromManager();
        }
        
        private void InitializeDmodsFromManager() {
            if (this._dmodManager == null ||
                this._cfgGeneral == null) { return; }
            
            this.DmodDefinitions.Clear();

            foreach (DmodFileDefinition dfd in this._dmodManager.DmodList) {
                this.DmodDefinitions.Add(new DmodDefinition(dfd));
            }

            this.InitializeFilteredDmods();
        }
        
        /// <summary>
        /// Initializes filtered dmods list for the view using current <see cref="DmodDefinitions"/>
        /// </summary>
        private void InitializeFilteredDmods() {
            if (this.DmodSearchString != null && this.DmodSearchString.Length >= 2) {
                string searchStr = this.DmodSearchString.ToLowerInvariant();
                
                var filtered = this._dmodDefinitions.Where(definition => 
                    definition.Name?.ToLowerInvariant().Contains(searchStr) == true );
                
                // filter definitions
                this.InitializeOrderedFilteredDmods(filtered);
            } else {
                // use all the definitions
                this.InitializeOrderedFilteredDmods(this.DmodDefinitions);
            }
        }
        
        private void InitializeOrderedFilteredDmods(IEnumerable<DmodDefinition> dmods) {
            switch (this.DmodOrderBy) {
                default: 
                    this.DmodDefinitionsFiltered = new ObservableCollection<DmodDefinition>(dmods);
                    break;
                
                case DmodOrderBy.NameAsc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<DmodDefinition>(
                        dmods.OrderBy(x => x.Name));
                    break;
                case DmodOrderBy.NameDesc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<DmodDefinition>(
                        dmods.OrderByDescending(x => x.Name));
                    break;
                
                case DmodOrderBy.PathAsc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<DmodDefinition>(
                        dmods.OrderBy(x => x.Path));
                    break;
                case DmodOrderBy.PathDesc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<DmodDefinition>(
                        dmods.OrderBy(x => x.Path));
                    break;
            }
        }
        
        #endregion
        
        
                
        #region DMOD Selected - Properties and methods related to the currently selected online DMOD are here
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
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
        
        
        #endregion



        #region COMMANDS

        public void CmdRefreshDmods(object? parameter = null) {
            if (this.DmodManager == null || 
                this.ConfigurationGeneral == null) { return; }
            this.DmodSearchString = null;

            this.DmodManager.Initialize(this.ConfigurationGeneral);
        }

        [DependsOn(nameof(DmodManager))]
        public bool CanCmdRefreshDmods(object? parameter = null) {
            if (this.DmodManager == null) { return false; }
            return true;
        }
        
        public void CmdClearSelectedDmod(object? parameter = null) {
            if (this.SelectedDmodDefinition == null) { return; }
            this.SelectedDmodDefinition = null;
        }

        [DependsOn(nameof(SelectedDmodDefinition))]
        public bool CanCmdClearSelectedDmod(object? parameter = null) {
            if (this.SelectedDmodDefinition == null) { return false; }
            return true;
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
                if (this.ConfigurationLauncher == null) { return false; }
                if (this.DmodLauncher == null) { return false; }
                if (this.DmodManager == null) { return false; }
                if (this.SelectedDmodDefinition?.Path == null) { return false; }
                if (this.ConfigurationGeneral?.GameExePaths.Count == 0) { return false; }
                if (this.ConfigurationGeneral == null) { return false; }

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

        #endregion
        
    }
}
