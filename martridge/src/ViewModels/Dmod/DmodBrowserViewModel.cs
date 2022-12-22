using Avalonia.Metadata;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Dmod;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

                if (args.PropertyName == nameof(this.ActiveGameExeIndex)) {
                    this.RefreshIsLauncherFreeDink();
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
        
        public Config? Configuration {
            get => this._configuration;
            set {
                if (this._configuration != null) {
                    this._configuration.General.Updated -= this.ConfigurationGeneralUpdated;
                    this._configuration = null;
                }
                this.RaiseAndSetIfChanged(ref this._configuration, value);

                if (this._configuration != null) {
                    this._configuration.General.Updated += this.ConfigurationGeneralUpdated;
                    this.LoadFromConfigGeneral();
                }
            }
        }
        private Config? _configuration = null;

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

        public bool GameExeFound => this.GameExePaths.Count > 0;
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        private void ConfigurationGeneralUpdated(object? sender, EventArgs e) {
            this.LoadFromConfigGeneral();
        }
        
        private void LoadFromConfigGeneral() {
            if (this._configuration == null) { return; }

            ObservableCollection<string> list = new ObservableCollection<string>();
            foreach (string str in this._configuration.General.GameExePaths) {
                FileInfo finfo = new FileInfo(str);
                if (finfo.Exists) {
                    list.Add(str);
                }
            }
            
            this.ActiveGameExeIndex = -1;
            this.GameExePaths = list;
            this.ActiveGameExeIndex = this._configuration.General.ActiveGameExeIndex;
            
            this.RaisePropertyChanged(nameof(this.GameExeFound));
        }

        private void SaveToConfigGeneral() {
            if (this._configuration == null) { return; }

            if (this.ActiveGameExeIndex != this._configuration.General.ActiveGameExeIndex) {
                this._configuration.General.ActiveGameExeIndex = this.ActiveGameExeIndex;
            }
        }

        #endregion

        #region DMOD Launcher

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------

        
        public bool IsLauncherFreeDink {
            get => this._isLauncherFreeDink;
            protected set => this.RaiseAndSetIfChanged(ref this._isLauncherFreeDink, value);
        }

        private bool _isLauncherFreeDink = false;
        
        public bool DmodLauncherWaitingForDelay {
            get => this._dmodLauncherWaitingForDelay;
            set => this.RaiseAndSetIfChanged(ref this._dmodLauncherWaitingForDelay, value);
        }
        private bool _dmodLauncherWaitingForDelay = false;
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------

        private void RefreshIsLauncherFreeDink() {
            if (this.Configuration?.General == null) {
                this.IsLauncherFreeDink = false;
                return;
            }
            if (this.ActiveGameExeIndex < 0 || this.ActiveGameExeIndex >= this.Configuration.General.GameExePaths.Count) {
                this.IsLauncherFreeDink = false;
                return;
            }

            string path = this.Configuration.General.GameExePaths[this.ActiveGameExeIndex];
            FileInfo finfo = new FileInfo(path);
            if (finfo.Name.ToLowerInvariant().Contains("freedink") == false) {
                this.IsLauncherFreeDink = false;
                return;
            }
            
            this.IsLauncherFreeDink = true;
        }

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
                this.RaiseAndSetIfChanged(ref this._dmodManager, value);
                if (this._dmodManager != null) {
                    this._dmodManager.DmodListInitialized += this.DmodManager_DmodListInitialized;
                }
                this.InitializeDmodsFromManager();
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
        
        public IEnumerable<DmodDefinition> DmodDefinitionsFiltered {
            get => this._dmodDefinitionsFiltered;
            set {
                this.RaiseAndSetIfChanged(ref this._dmodDefinitionsFiltered, value);
                this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));
            }
        }
        private IEnumerable<DmodDefinition> _dmodDefinitionsFiltered = new List<DmodDefinition>();
        
        public bool DmodDefinitionsFilteredHasItems => this.DmodDefinitionsFiltered.Any();
        private List<DmodDefinition> _dmodDefinitions = new List<DmodDefinition>();

        public List<DmodOrderBy> DmodOrderByList { get; } = new List<DmodOrderBy>() {
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
            if (this._dmodManager == null || this._configuration == null) {
                this._dmodDefinitions = new List<DmodDefinition>();
            } else {
                List<DmodDefinition> dmodDefs  = new List<DmodDefinition>();
                foreach (DmodFileDefinition dfd in this._dmodManager.DmodList) {
                    dmodDefs.Add(new DmodDefinition(dfd));
                }
                this._dmodDefinitions = dmodDefs;
            }

            this.InitializeFilteredDmods();
        }

        /// <summary>
        /// Initializes filtered dmods list for the view using current <see cref="_dmodDefinitions"/>
        /// </summary>
        private void InitializeFilteredDmods() {
            void SetFilteredDmods(IEnumerable<DmodDefinition> dmods) {
                this.DmodDefinitionsFiltered = this.DmodOrderBy switch {
                    DmodOrderBy.NameAsc => dmods.OrderBy(x => x.Name),
                    DmodOrderBy.NameDesc => dmods.OrderByDescending(x => x.Name),
                    DmodOrderBy.PathAsc => dmods.OrderBy(x => x.Path),
                    DmodOrderBy.PathDesc => dmods.OrderBy(x => x.Path),
                    _ => dmods.AsEnumerable(),
                };
            }
            
            if (this.DmodSearchString != null && this.DmodSearchString.Length >= 2) {
                string searchStr = this.DmodSearchString.ToLowerInvariant();
                
                IEnumerable<DmodDefinition> filtered = this._dmodDefinitions.Where(definition => 
                    definition.Name?.ToLowerInvariant().Contains(searchStr) == true );

                // filter definitions
                SetFilteredDmods(filtered);
            } else {
                // use all the definitions
                SetFilteredDmods(this._dmodDefinitions);
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
                this.Configuration?.General == null) { return; }
            this.DmodSearchString = null;

            // reload configuration just in case something was not synchronized previously...
            this.LoadFromConfigGeneral();

            this.DmodManager.Initialize(this.Configuration.General);
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
        
        public async void CmdLaunchDmod(object? parameter = null) {
            try {
                if (CanCmdLaunchDmod() == false) return;
                
                this.DmodLauncherWaitingForDelay = true;
                this.SaveToConfigLauncher();
                this.SaveToConfigGeneral();

                // launch dmod with separate task to prevent gui lockup
                await Task.Run(() => {
                        
                    if (this.Configuration == null ||
                        this.SelectedDmodDefinition?.Path == null) return;
                        
                    DmodLauncher.LaunchDmod(
                        this.Configuration.General.GameExePaths[this.ActiveGameExeIndex],
                        this.SelectedDmodDefinition.Path,
                        this.Configuration.Launch,
                        this.SelectedLocalization?.CultureInfo?.Name);
                });
                this._dmodLauncherDelay.Start();
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex);
            }
        }

        [DependsOn(nameof(GameExeFound))]
        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(ConfigurationLauncher))]
        [DependsOn(nameof(ActiveGameExeIndex))]
        [DependsOn(nameof(DmodManager))]
        [DependsOn(nameof(SelectedDmodDefinition))]
        public bool CanCmdLaunchDmod(object? parameter = null) {
            try {
                if (!this.GameExeFound) return false;
                if (this.Configuration == null) return false;
                if (this.ConfigurationLauncher == null) return false;
                if (this.Configuration.General.GameExePaths.Count == 0) return false;
                if (this.ActiveGameExeIndex < 0 || this.ActiveGameExeIndex >= this.Configuration.General.GameExePaths.Count) return false;
                if (this.DmodManager == null) return false; 
                if (this.SelectedDmodDefinition?.Path == null) return false;
                

                string exePath = this.Configuration.General.GameExePaths[this.ActiveGameExeIndex];
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
