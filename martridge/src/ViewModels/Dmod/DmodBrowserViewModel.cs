using Avalonia.Metadata;
using Martridge.Models.Configuration;
using Martridge.Models.Configuration.Save;
using Martridge.Models.Dmod;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

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
                this.InitializeFilteredDmods(this._lastusedDmodDefinitions);
            };
            
            // self properties changed
            this.PropertyChanged += ( sender,  args) => {
                if (args.PropertyName == nameof(this.DmodSearchString)) {
                    if (this._dmodSearchTimer.Enabled == false) {
                        this._dmodSearchTimer.Start();
                    }
                }

                if (args.PropertyName == nameof(this.DmodOrderBy)) {
                    this.InitializeFilteredDmods(this._lastusedDmodDefinitions);
                }

                if (args.PropertyName == nameof(this.ActiveGameExePath)) {
                    this.RefreshIsLauncherFreeDink();
                }
            };
        }
        
        #endregion

        #region CONFIGURATION

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        public Config? Configuration {
            get => this._configuration;
            set {
                if (this._configuration != null) {
                    this._configuration.General.Updated -= this.ConfigurationGeneralUpdated;
                    this._configuration.Launch.Updated -= this.ConfigurationLauncherUpdated;
                    this._configuration = null;
                }
                this.RaiseAndSetIfChanged(ref this._configuration, value);

                if (this._configuration != null) {
                    this._configuration.General.Updated += this.ConfigurationGeneralUpdated;
                    this._configuration.Launch.Updated += this.ConfigurationLauncherUpdated;
                    this.LoadFromConfigGeneral(this._configuration.General);
                    this.LoadFromConfigLauncher(this._configuration.Launch);
                }
            }
        }
        private Config? _configuration = null;

        #endregion
        

        #region CONFIGURATION - Launch

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
        
        public bool LaunchSkip {
            get => this._launchSkip;
            set => this.RaiseAndSetIfChanged(ref this._launchSkip, value);
        }
        private bool _launchSkip= true;

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        private void ConfigurationLauncherUpdated(object? sender, EventArgs e) {
            if (sender is not ConfigLaunch cfg) return;
            this.LoadFromConfigLauncher(cfg);
        }
        
        private void LoadFromConfigLauncher(ConfigLaunch cfg) {
            this.LaunchTrueColor = cfg.TrueColor;
            this.LaunchWindowed = cfg.Windowed;
            this.LaunchSound = cfg.Sound;
            this.LaunchJoystick = cfg.Joystick;
            this.LaunchDebug = cfg.Debug;
            this.LaunchV107Mode = cfg.V107Mode;
            this.LaunchUsePathQuotationMarks = cfg.UsePathQuotationMarks;
            this.LaunchUsePathRelativeToGame = cfg.UsePathRelativeToGame;
            this.LaunchCustomUserArguments = cfg.CustomUserArguments;
            this.LaunchSkip = cfg.Skip;
        }

        private void SaveToConfigLauncher(ConfigLaunch cfg) {
            cfg.UpdateFromData( new ConfigDataLaunch() {
                TrueColor = this.LaunchTrueColor,
                Windowed = this.LaunchWindowed,
                Sound = this.LaunchSound,
                Joystick = this.LaunchJoystick,
                Debug = this.LaunchDebug,
                V107Mode = this.LaunchV107Mode,
                UsePathQuotationMarks = this.LaunchUsePathQuotationMarks,
                UsePathRelativeToGame = this.LaunchUsePathRelativeToGame,
                CustomUserArguments = this.LaunchCustomUserArguments,
                Skip = this.LaunchSkip,
            });
        }
        
        #endregion

        #region CONFIGURATION - General

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Mirrored Main Configuration Properties
        // -----------------------------------------------------------------------------------------------------------------------------------

        public ObservableCollection<DmodLauncherSelectionViewModel> GameExePaths {
            get => this._gameExePaths;
            set => this.RaiseAndSetIfChanged(ref this._gameExePaths, value);
        }
        private ObservableCollection<DmodLauncherSelectionViewModel> _gameExePaths = new ObservableCollection<DmodLauncherSelectionViewModel>();
        
        public DmodLauncherSelectionViewModel? ActiveGameExePath {
            get => this._activeGameExePath;
            set => this.RaiseAndSetIfChanged(ref this._activeGameExePath, value);
        }
        private DmodLauncherSelectionViewModel? _activeGameExePath = null;

        public bool GameExeFound => this.GameExePaths.Count > 0;
        
        public ObservableCollection<DmodLauncherSelectionViewModel> EditorExePaths {
            get => this._editorExePaths;
            set => this.RaiseAndSetIfChanged(ref this._editorExePaths, value);
        }
        private ObservableCollection<DmodLauncherSelectionViewModel> _editorExePaths = new ObservableCollection<DmodLauncherSelectionViewModel>();
        
        public DmodLauncherSelectionViewModel? ActiveEditorExePath {
            get => this._activeEditorExePath;
            set => this.RaiseAndSetIfChanged(ref this._activeEditorExePath, value);
        }
        private DmodLauncherSelectionViewModel? _activeEditorExePath = null;

        public bool EditorExeFound => this.EditorExePaths.Count > 0;

        public bool ShowDmodDevFeatures {
            get => this._ShowDmodDevFeatures;
            private set => this.RaiseAndSetIfChanged(ref this._ShowDmodDevFeatures, value);
        }
        private bool _ShowDmodDevFeatures = false;
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        private void ConfigurationGeneralUpdated(object? sender, EventArgs e) {
            if (this.Configuration?.General is not ConfigGeneral cfg) return;
            this.LoadFromConfigGeneral(cfg);
        }
        
        private void LoadFromConfigGeneral(ConfigGeneral cfg) {
            ObservableCollection<DmodLauncherSelectionViewModel> listGameExe = new ObservableCollection<DmodLauncherSelectionViewModel>();
            try {
                bool gamePathsChanged = false;

                // check if game exe paths differ
                if (this.GameExePaths.Count != cfg.GameExePaths.Count) {
                    gamePathsChanged = true;
                } else {
                    for (int i = 0; i < cfg.GameExePaths.Count; i++) {
                        if (cfg.GameExePaths[i] != this.GameExePaths[i].Path) {
                            gamePathsChanged = true;
                            break;
                        }
                    }
                }

                if (gamePathsChanged) {
                    // update game exe paths
                    for (int i = 0; i < cfg.GameExePaths.Count; i++) {
                        FileInfo finfo = new FileInfo(cfg.GameExePaths[i]);
                        string? dirName = finfo.Directory?.Name; 
                        listGameExe.Add(new DmodLauncherSelectionViewModel() {
                            Path = cfg.GameExePaths[i],
                            DisplayName = dirName != null ? Path.Combine(dirName,finfo.Name) : finfo.Name,
                        });
                    }
                    this.GameExePaths = listGameExe;
                    if (cfg.ActiveGameExeIndex >= 0 && cfg.ActiveGameExeIndex < cfg.GameExePaths.Count) {
                        this.ActiveGameExePath = this.GameExePaths[cfg.ActiveGameExeIndex];
                    } else {
                        this.ActiveGameExePath = null;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex);
                this.GameExePaths = listGameExe;
                this.ActiveGameExePath = null;
            }



            ObservableCollection<DmodLauncherSelectionViewModel> listEditorExe = new ObservableCollection<DmodLauncherSelectionViewModel>();
            try {
                bool editorPathsChanged = false;
                
                // check if editor exe paths differ
                if (this.EditorExePaths.Count != cfg.EditorExePaths.Count) {
                    editorPathsChanged = true;
                } else {
                    for (int i = 0; i < cfg.EditorExePaths.Count; i++) {
                        if (cfg.EditorExePaths[i] != this.EditorExePaths[i].Path) {
                            editorPathsChanged = true;
                            break;
                        }
                    }
                }
                if (editorPathsChanged) {
                    // update editor exe paths
                    for (int i = 0; i < cfg.EditorExePaths.Count; i++) {
                        FileInfo finfo = new FileInfo(cfg.EditorExePaths[i]);
                        string? dirName = finfo.Directory?.Name; 
                        listEditorExe.Add(new DmodLauncherSelectionViewModel() {
                            Path = cfg.EditorExePaths[i],
                            DisplayName = dirName != null ? Path.Combine(dirName,finfo.Name) : finfo.Name,
                        });
                    }
                    this.EditorExePaths = listEditorExe;
                    if (cfg.ActiveEditorExeIndex >= 0 && cfg.ActiveEditorExeIndex < cfg.EditorExePaths.Count) {
                        this.ActiveEditorExePath = this.EditorExePaths[cfg.ActiveEditorExeIndex];
                    } else {
                        this.ActiveEditorExePath = null;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex);
                this.EditorExePaths = listGameExe;
                this.ActiveEditorExePath = null;
            }

            this.RaisePropertyChanged(nameof(this.GameExeFound));
            this.RaisePropertyChanged(nameof(this.EditorExeFound));
            
            this.ShowDmodDevFeatures = cfg.ShowDmodDevFeatures;
        }

        private void SaveActiveIndexToConfigGeneral(ConfigGeneral cfg) {
            Dictionary<string, object?> updates = new Dictionary<string, object?>();
            
            // update game index
            for (int i = 0; i < cfg.GameExePaths.Count; i++) {
                string path = cfg.GameExePaths[i];
                if (path == this.ActiveGameExePath?.Path) {
                    if (i != cfg.ActiveGameExeIndex) {
                        updates.Add(nameof(ConfigGeneral.ActiveGameExeIndex), i);
                        break;
                    }
                }
            }
            
            // update editor index too
            for (int i = 0; i < cfg.EditorExePaths.Count; i++) {
                string path = cfg.EditorExePaths[i];
                if (path == this.ActiveEditorExePath?.Path) {
                    if (i != cfg.ActiveEditorExeIndex) {
                        updates.Add(nameof(ConfigGeneral.ActiveEditorExeIndex), i);
                        break;
                    }
                }
            }

            cfg.UpdateProperties(updates);
        }

        #endregion

        #region DMOD Launcher

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------

        public string LaunchEditorParameter => "editor";
        
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
            if (this.Configuration?.General == null ||
                this.ActiveGameExePath == null ||
                this.ActiveGameExePath?.PathExists != true) {
                this.IsLauncherFreeDink = false;
                return;
            }

            string path = this.ActiveGameExePath.Path;
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
        private List<DmodDefinition> _lastusedDmodDefinitions = new List<DmodDefinition>();

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
            List<DmodDefinition> newDmodList = new List<DmodDefinition>();
            if (this._dmodManager is DmodManager dMan) {
                foreach (DmodFileDefinition dfd in dMan.DmodList) {
                    newDmodList.Add(new DmodDefinition(dfd));
                }
            }
            
            this.InitializeFilteredDmods(newDmodList);
        }

        /// <summary>
        /// Initializes filtered dmods list for the view using current <see cref="_lastusedDmodDefinitions"/>
        /// </summary>
        private void InitializeFilteredDmods(List<DmodDefinition> newDmodList) {
            void SetFilteredDmods(IEnumerable<DmodDefinition> dmods) {
                string oldSelPath = this.SelectedDmodDefinition?.Path ?? "";
                
                this.DmodDefinitionsFiltered = this.DmodOrderBy switch {
                    DmodOrderBy.NameAsc => dmods.OrderBy(x => x.Name),
                    DmodOrderBy.NameDesc => dmods.OrderByDescending(x => x.Name),
                    DmodOrderBy.PathAsc => dmods.OrderBy(x => x.Path),
                    DmodOrderBy.PathDesc => dmods.OrderBy(x => x.Path),
                    _ => dmods.AsEnumerable(),
                };

                // restore selected dmod!
                foreach (var def in this.DmodDefinitionsFiltered) {
                    if (def.Path == oldSelPath) {
                        this.SelectedDmodDefinition = def;
                        break;
                    }
                }
            }
            
            this._lastusedDmodDefinitions = newDmodList;
            
            if (this.DmodSearchString != null && this.DmodSearchString.Length >= 2) {
                string searchStr = this.DmodSearchString.ToLowerInvariant();
                
                IEnumerable<DmodDefinition> filtered = newDmodList.Where(definition => 
                    definition.Name?.ToLowerInvariant().Contains(searchStr) == true );

                // filter definitions
                SetFilteredDmods(filtered);
            } else {
                // use all the definitions
                SetFilteredDmods(newDmodList);
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
            if (this.DmodManager is not DmodManager manager ||
                this.Configuration?.General is not ConfigGeneral cfgGen) { return; }
            this.DmodSearchString = null;

            // reload configuration just in case something was not synchronized previously...
            this.LoadFromConfigGeneral(cfgGen);
            manager.Initialize(cfgGen);
        }

        [DependsOn(nameof(DmodManager))]
        public bool CanCmdRefreshDmods(object? parameter = null) => this.DmodManager != null;
        public void CmdClearSelectedDmod(object? parameter = null) {
            this.SelectedDmodDefinition = null;
        }

        [DependsOn(nameof(SelectedDmodDefinition))]
        public bool CanCmdClearSelectedDmod(object? parameter = null) => this.SelectedDmodDefinition != null;
        
        public async void CmdLaunchDmod(object? parameter = null) {
            try {
                bool launchEditor = false;
                if (parameter is string str && str == this.LaunchEditorParameter) launchEditor = true;
                
                
                if (this.Configuration is not Config cfg) return;
                if (this.DmodManager is not DmodManager dmodMan) return;
                if (!this.GameExeFound) return;
                if (string.IsNullOrEmpty(this.SelectedDmodDefinition?.Path)) return;

                string exePath;
                
                if (launchEditor) {
                    if (string.IsNullOrEmpty(this.ActiveEditorExePath?.Path)) return;
                    exePath = this.ActiveEditorExePath.Path;
                } else {
                    if (string.IsNullOrEmpty(this.ActiveGameExePath?.Path)) return;
                    exePath = this.ActiveGameExePath.Path;
                }
                
                string dmodPath = this.SelectedDmodDefinition.Path;
                
                this.DmodLauncherWaitingForDelay = true;
                this.SaveToConfigLauncher(cfg.Launch);
                this.SaveActiveIndexToConfigGeneral(cfg.General);

                // launch dmod with separate task to prevent gui lockup
                await Task.Run(() => {
                    DmodLauncher.LaunchDmod(exePath, dmodPath, cfg.Launch, this.SelectedLocalization?.CultureInfo?.Name);
                });
                this._dmodLauncherDelay.Start();
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex);
            }
        }

        [DependsOn(nameof(Configuration))]
        [DependsOn(nameof(DmodManager))]
        [DependsOn(nameof(GameExeFound))]
        [DependsOn(nameof(ActiveGameExePath))]
        [DependsOn(nameof(ActiveEditorExePath))]
        [DependsOn(nameof(SelectedDmodDefinition))]
        public bool CanCmdLaunchDmod(object? parameter = null) {
            try {
                bool launchEditor = false;
                if (parameter is string str && str == this.LaunchEditorParameter) launchEditor = true;
                
                if (this.Configuration == null) return false;
                if (this.DmodManager == null) return false;
                if (!this.GameExeFound) return false;
                if (string.IsNullOrEmpty(this.SelectedDmodDefinition?.Path)) return false;

                string exePath;
                
                if (launchEditor) {
                    if (string.IsNullOrEmpty(this.ActiveEditorExePath?.Path)) return false;
                    exePath = this.ActiveEditorExePath.Path;
                } else {
                    if (string.IsNullOrEmpty(this.ActiveGameExePath?.Path)) return false;
                    exePath = this.ActiveGameExePath.Path;
                }
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
