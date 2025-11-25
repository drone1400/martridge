using Avalonia.Metadata;
using Martridge.Models.Configuration;
using Martridge.Models.Dmod;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Martridge.Models;
using Martridge.Models.Configuration.AppState;
using Martridge.Models.Configuration.General;
using Martridge.Models.Localization;

namespace Martridge.ViewModels.Dmod {
    
    public class MyDmodComparer : IComparer {
        public int Compare(object? x, object? y) {
            if (x is DmodDefinition x1 && y is DmodDefinition y1)
            {
                return String.CompareOrdinal(x1.DmodParentDirectory, y1.DmodParentDirectory);
            }
            return 0;
        }
    }
    
    public class DmodBrowserViewModel : ViewModelAppPageWithCfg {

        #region  CONSTRUCTOR / Initialization
        
        private readonly Timer _dmodLauncherDelay;
        private readonly Timer _dmodSearchTimer;
        
        // this is so we don't override the app state until the view model has been fully initialized...
        private bool _canSaveSelectedDmodNameToAppState = false;

        /// <summary>
        /// Constructor for Local Dmod Browser / Launcher View Model
        /// </summary>
        public DmodBrowserViewModel() {
            // Launcher Delay Timer
            this._dmodLauncherDelay = new Timer() {
                Interval = 5000,
                AutoReset = true,
            };
            this._dmodLauncherDelay.Elapsed += ( _, _) => {
                this._dmodLauncherDelay.Stop();
                this.DmodLauncherWaitingForDelay = false;
            };

            // Dmod Search Timer
            this._dmodSearchTimer = new Timer() {
                Interval = 200,
                AutoReset = true,
            };
            this._dmodSearchTimer.Elapsed += ( _, _) => {
                this._dmodSearchTimer.Stop();
                this.InitializeFilteredDmods(this._lastusedDmodDefinitions);
            };
            
            // self properties changed
            this.PropertyChanged += this.OnPropertyChanged;


            this.InitializeFromConfig();
        }
        
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            try {
                switch (e.PropertyName) {
                    case nameof(this.DmodSearchString):
                        if (this._dmodSearchTimer.Enabled == false) {
                            this._dmodSearchTimer.Start();
                        }
                        break;
                    case nameof(this.ActiveGameExePath):
                        this.RefreshShowFreedinkLocalizations();
                        break;
                    case nameof(this.SelectedDmodDefinition):
                        this.RefreshShowFreedinkLocalizations();
                        break;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        
        private void InitializeFromConfig() {
            this.LoadFromConfigGeneral();

            // restore selected dmod...
            this.SelectDmodByPath(this.CfgAppState.DmodBrowserSelectedDmodPath);
            
            // reset flags so the values get updated in the UI...
            this.DmodBrowserLeftPanelColumnWidthSet = false;
            this.DmodBrowserRightPanelColumnWidthSet = false;
            // write actual values
            this.DmodBrowserLeftPanelColumnWidth = new GridLength(this.CfgAppState.DmodBrowserLeftPanelColumnWidth, GridUnitType.Star);
            this.DmodBrowserRightPanelColumnWidth = new GridLength(this.CfgAppState.DmodBrowserRightPanelColumnWidth, GridUnitType.Star);
        }

        public void InitializeDmodManager(DmodManager dmodManager, bool restoreSelectedDmod) {
            this.DmodManager = dmodManager;
            
            if (restoreSelectedDmod) {
                this.SelectDmodByPath(this.CfgAppState.DmodBrowserSelectedDmodPath);
            }

            this._canSaveSelectedDmodNameToAppState = true;
        }
        
        protected override void OnCfgGeneralUpdated(object? sender, ConfigUpdateEventArgs e) {
            this.LoadFromConfigGeneral();
        }

        protected override void OnCfgLaunchUpdated(object? sender, ConfigUpdateEventArgs e) {
            this.LoadFromConfigLauncher();
        }
        
        protected override void OnCfgAppStateUpdated(object? sender, ConfigUpdateEventArgs e) {
            // don't care...
        }


        public void SelectDmodByPath(string dmodPath) {
            if (string.IsNullOrWhiteSpace(dmodPath) == false) {
                foreach (var dmod in this._dmodDefinitionsFiltered) {
                    if (LocationHelper.PathIsEqual(dmod.DmodDirectory, dmodPath, LocationHelperPathCompareFlags.IgnoreDirectorySeparator)) {
                        Dispatcher.UIThread.InvokeAsync(() => {
                            this.DmodDefinitionsCollection?.MoveCurrentTo(dmod);
                        });
                        return;
                    }
                }
            }
            
            // as a fallback, make sure the current item is selected
            this.SelectDmodFromCollectionViewCurentItem();
        }

        #endregion
        
        #region CONFIGURATION - remembered layout

        public bool DmodBrowserLeftPanelColumnWidthSet {
            get => this._dmodBrowserLeftPanelColumnWidthSet;
            set => this.RaiseAndSetIfChanged(ref this._dmodBrowserLeftPanelColumnWidthSet, value);
        }
        private bool _dmodBrowserLeftPanelColumnWidthSet = false;

        public GridLength DmodBrowserLeftPanelColumnWidth {
            get => this._dmodBrowserLeftPanelColumnWidth;
            set {
                this.RaiseAndSetIfChanged(ref this._dmodBrowserLeftPanelColumnWidth, value);
                Dictionary<string, object?> values = new Dictionary<string, object?>() {
                    [nameof(ConfigAppState.DmodBrowserLeftPanelColumnWidth)] = value.Value,
                };
                this.CfgAppState.UpdateProperties(values);
            }
        }
        private GridLength _dmodBrowserLeftPanelColumnWidth = new GridLength(1.0, GridUnitType.Star); 
        
        public bool DmodBrowserRightPanelColumnWidthSet {
            get => this._dmodBrowserRightPanelColumnWidthSet;
            set => this.RaiseAndSetIfChanged(ref this._dmodBrowserRightPanelColumnWidthSet, value);
        }
        private bool _dmodBrowserRightPanelColumnWidthSet = false;
        
        public GridLength DmodBrowserRightPanelColumnWidth {
            get => this._dmodBrowserRightPanelColumnWidth;
            set {
                this.RaiseAndSetIfChanged(ref this._dmodBrowserRightPanelColumnWidth, value);
                Dictionary<string, object?> values = new Dictionary<string, object?>() {
                    [nameof(ConfigAppState.DmodBrowserRightPanelColumnWidth)] = value.Value,
                };
                this.CfgAppState.UpdateProperties(values);
            }
        }
        private GridLength _dmodBrowserRightPanelColumnWidth = new GridLength(1.0, GridUnitType.Star);
        
        
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
        
        public bool LaunchUseRefDir {
            get => this._launchUseRefDir;
            set => this.RaiseAndSetIfChanged(ref this._launchUseRefDir, value);
        }
        private bool _launchUseRefDir= true;

        public string LaunchRefDirPath {
            get => this._launchRefDirPath;
            set => this.RaiseAndSetIfChanged(ref this._launchRefDirPath, value);
        }
        private string _launchRefDirPath = string.Empty;
        
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

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        private void LoadFromConfigLauncher() {
            if (this.CfgLaunch is not ConfigLaunch cfg) return;
            
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
            this.LaunchUseRefDir = cfg.UseRefDir;
            this.LaunchRefDirPath = cfg.RefDirPath;
        }

        private void SaveToConfigLauncher() {
            if (this.CfgLaunch is not ConfigLaunch cfg) return;
            
            Dictionary<string, object?> values = new Dictionary<string, object?>() {
                [nameof(ConfigLaunch.TrueColor)] = this.LaunchTrueColor,
                [nameof(ConfigLaunch.Windowed)] = this.LaunchWindowed,
                [nameof(ConfigLaunch.Sound)] = this.LaunchSound,
                [nameof(ConfigLaunch.Joystick)] = this.LaunchJoystick,
                [nameof(ConfigLaunch.Debug)] = this.LaunchDebug,
                [nameof(ConfigLaunch.V107Mode)] = this.LaunchV107Mode,
                [nameof(ConfigLaunch.UsePathQuotationMarks)] = this.LaunchUsePathQuotationMarks,
                [nameof(ConfigLaunch.UsePathRelativeToGame)] = this.LaunchUsePathRelativeToGame,
                [nameof(ConfigLaunch.CustomUserArguments)] = this.LaunchCustomUserArguments,
                [nameof(ConfigLaunch.Skip)] = this.LaunchSkip,
                [nameof(ConfigLaunch.UseRefDir)] = this.LaunchUseRefDir,
                [nameof(ConfigLaunch.RefDirPath)] = this.LaunchRefDirPath,
            };
            cfg.UpdateProperties(values);
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
            private set => this.RaiseAndSetIfChanged(ref this._activeGameExePath, value);
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
            get => this._showDmodDevFeatures;
            private set => this.RaiseAndSetIfChanged(ref this._showDmodDevFeatures, value);
        }
        private bool _showDmodDevFeatures = false;
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        
        private void LoadFromConfigGeneral() {
            if (this.CfgGeneral is not ConfigGeneral cfg) return;
            
            ObservableCollection<DmodLauncherSelectionViewModel> listGameExe = new ObservableCollection<DmodLauncherSelectionViewModel>();
            try {
                bool gamePathsChanged = false;

                // check if game exe paths differ
                if (this.GameExePaths.Count != cfg.GameExePaths.Count) {
                    gamePathsChanged = true;
                } else {
                    for (int i = 0; i < cfg.GameExePaths.Count; i++) {
                        if (cfg.GameExePaths[i] != this.GameExePaths[i].PathRaw) {
                            gamePathsChanged = true;
                            break;
                        }
                    }
                }
                
                if (gamePathsChanged) {
                    // update game exe paths
                    for (int i = 0; i < cfg.GameExePaths.Count; i++) {
                        listGameExe.Add(new DmodLauncherSelectionViewModel(cfg.GameExePaths[i]));
                    }
                    this.GameExePaths = listGameExe;
                    if (cfg.ActiveGameExeIndex >= 0 && cfg.ActiveGameExeIndex < cfg.GameExePaths.Count) {
                        this.ActiveGameExePath = this.GameExePaths[cfg.ActiveGameExeIndex];
                        this.RefreshShowFreedinkLocalizations();
                    } else {
                        this.ActiveGameExePath = null;
                        this.RefreshShowFreedinkLocalizations();
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                this.GameExePaths = listGameExe;
                this.ActiveGameExePath = null;
                this.RefreshShowFreedinkLocalizations();
            }


            ObservableCollection<DmodLauncherSelectionViewModel> listEditorExe = new ObservableCollection<DmodLauncherSelectionViewModel>();
            try {
                bool editorPathsChanged = false;
                
                // check if editor exe paths differ
                if (this.EditorExePaths.Count != cfg.EditorExePaths.Count) {
                    editorPathsChanged = true;
                } else {
                    for (int i = 0; i < cfg.EditorExePaths.Count; i++) {
                        if (cfg.EditorExePaths[i] != this.EditorExePaths[i].PathRaw) {
                            editorPathsChanged = true;
                            break;
                        }
                    }
                }
                if (editorPathsChanged) {
                    // update editor exe paths
                    for (int i = 0; i < cfg.EditorExePaths.Count; i++) {
                        listEditorExe.Add(new DmodLauncherSelectionViewModel(cfg.EditorExePaths[i]));
                    }
                    this.EditorExePaths = listEditorExe;
                    if (cfg.ActiveEditorExeIndex >= 0 && cfg.ActiveEditorExeIndex < cfg.EditorExePaths.Count) {
                        this.ActiveEditorExePath = this.EditorExePaths[cfg.ActiveEditorExeIndex];
                    } else {
                        this.ActiveEditorExePath = null;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                this.EditorExePaths = listGameExe;
                this.ActiveEditorExePath = null;
            }

            this.RaisePropertyChanged(nameof(this.GameExeFound));
            this.RaisePropertyChanged(nameof(this.EditorExeFound));
            
            this.ShowDmodDevFeatures = cfg.ShowDmodDevFeatures;
            this.ShowLaunchRefDirPathInMainWindow = cfg.ShowLaunchRefDirPathInMainWindow;
            this.ShowLaunchCustomArgsInMainWindow = cfg.ShowLaunchCustomArgsInMainWindow;
        }

        private void SaveActiveIndexToConfigGeneral() {
            if (this.CfgGeneral is not ConfigGeneral cfg) return;
            
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
        
        public bool ShowFreeDinkLocalizations {
            get => this._showFreeDinkLocalizations;
            protected set => this.RaiseAndSetIfChanged(ref this._showFreeDinkLocalizations, value);
        }

        private bool _showFreeDinkLocalizations = false;
        
        public bool DmodLauncherWaitingForDelay {
            get => this._dmodLauncherWaitingForDelay;
            set => this.RaiseAndSetIfChanged(ref this._dmodLauncherWaitingForDelay, value);
        }
        private bool _dmodLauncherWaitingForDelay = false;
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------

        private void RefreshShowFreedinkLocalizations() {
            if (this.ActiveGameExePath == null || this.ActiveGameExePath?.PathIsFile != true) {
                this.ShowFreeDinkLocalizations = false;
                return;
            }

            if (this.SelectedDmodDefinition == null) {
                this.ShowFreeDinkLocalizations = false;
                return;
            }

            if (this.SelectedDmodDefinition.Localizations.Count <= 1) {
                this.ShowFreeDinkLocalizations = false;
                return;
            }

            string path = this.ActiveGameExePath.Path;
            FileInfo finfo = new FileInfo(path);
            string nameLower = finfo.Name.ToLowerInvariant();
            if (nameLower.Contains("freedink") == false &&
                nameLower.Contains("yeoldedink") == false) {
                this.ShowFreeDinkLocalizations = false;
                return;
            }
            
            this.ShowFreeDinkLocalizations = true;
        }

        #endregion
        

        #region DMOD List - Properties and methods related to the DMOD list are here
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        public DmodManager? DmodManager {
            get => this._dmodManager;
            private set {
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

        public DataGridCollectionView? DmodDefinitionsCollection {
            get => this._dmodDefinitionsCollection;
            private set => this.RaiseAndSetIfChanged(ref this._dmodDefinitionsCollection, value);
        }
        private DataGridCollectionView? _dmodDefinitionsCollection = null;
        
        public bool DmodDefinitionsFilteredHasItems => this._dmodDefinitionsFiltered.Any();
        private List<DmodDefinition> _lastusedDmodDefinitions = new List<DmodDefinition>();
        private IEnumerable<DmodDefinition> _dmodDefinitionsFiltered = new List<DmodDefinition>();

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

        public void ReinitializeDataGridCollectionView() {
            try {
                string oldSelPath = this.SelectedDmodDefinition?.DmodDirectory ?? "";

                if (this.DmodDefinitionsCollection != null) {
                    this.DmodDefinitionsCollection.PropertyChanged -= this.DmodDefinitionsCollectionOnPropertyChanged;
                }

                DataGridCollectionView collectionView = new DataGridCollectionView(this._dmodDefinitionsFiltered);
                collectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("DmodParentDirectory"));
                collectionView.SortDescriptions.Add(new DataGridComparerSortDescription(new MyDmodComparer(), ListSortDirection.Ascending));
                collectionView.PropertyChanged += this.DmodDefinitionsCollectionOnPropertyChanged;
                this.DmodDefinitionsCollection = collectionView;

                // restore selected dmod!
                this.SelectDmodByPath(oldSelPath);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        /// <summary>
        /// Initializes filtered dmods list for the view using current <see cref="_lastusedDmodDefinitions"/>
        /// </summary>
        private void InitializeFilteredDmods(List<DmodDefinition> newDmodList) {
            this._lastusedDmodDefinitions = newDmodList;
            
            if (this.DmodSearchString != null && this.DmodSearchString.Length >= 2) {
                string searchStr = this.DmodSearchString.ToLowerInvariant();
                
                IEnumerable<DmodDefinition> filtered = newDmodList.Where(definition => 
                    definition.Name?.ToLowerInvariant().Contains(searchStr) == true );

                // filter definitions
                this._dmodDefinitionsFiltered = filtered;
                this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));
                
                this.ReinitializeDataGridCollectionView();
            } else {
                // use all the definitions
                this._dmodDefinitionsFiltered = newDmodList;
                this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));
                
                this.ReinitializeDataGridCollectionView();
            }
        }
        private void DmodDefinitionsCollectionOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof (DataGridCollectionView.CurrentItem)) {
                this.SelectDmodFromCollectionViewCurentItem();
            }
        }

        private void SelectDmodFromCollectionViewCurentItem() {
            if (this.DmodDefinitionsCollection is IDataGridCollectionView dgcv) {
                this.SelectedDmodDefinition = dgcv.CurrentItem as DmodDefinition;

                if (this._canSaveSelectedDmodNameToAppState && this.SelectedDmodDefinition != null) {
                    Dictionary<string, object?> values = new Dictionary<string, object?>() {
                        [nameof(ConfigAppState.DmodBrowserSelectedDmodPath)] = this.SelectedDmodDefinition.DmodDirectory
                    };
                    this.CfgAppState.UpdateProperties(values);
                }
            }
            else {
                this.SelectedDmodDefinition = null;
            }
        }

        #endregion


        #region DMOD Selected - Properties and methods related to the currently selected online DMOD are here
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        public DmodDefinition? SelectedDmodDefinition {
            get => this._selectedDmodDefinition;
            private set {
                this._selectedDmodDefinition?.UnloadThumbnail();
                value?.LoadThumbnail();
                this.RaiseAndSetIfChanged(ref this._selectedDmodDefinition, value);
            }
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
                this.CfgGeneral is not ConfigGeneral cfgGen) { return; }
            this.DmodSearchString = null;

            // reload configuration just in case something was not synchronized previously...
            this.LoadFromConfigGeneral();
            manager.Initialize(cfgGen);
        }

        [DependsOn(nameof(DmodManager))]
        public bool CanCmdRefreshDmods(object? parameter = null) => this.DmodManager != null;
        public void CmdClearSelectedDmod(object? parameter = null) {
            this.SelectedDmodDefinition = null;
            Dispatcher.UIThread.InvokeAsync(() => {
                this.DmodDefinitionsCollection?.MoveCurrentTo(null);
            });
        }

        [DependsOn(nameof(SelectedDmodDefinition))]
        public bool CanCmdClearSelectedDmod(object? parameter = null) => this.SelectedDmodDefinition != null;
        
        public async void CmdLaunchDmod(object? parameter = null) {
            try {
                bool launchEditor = false;
                if (parameter is string str && str == this.LaunchEditorParameter) launchEditor = true;
                
                if (this.DmodManager is not DmodManager dmodMan) return;
                //if (!this.GameExeFound) return;
                if (string.IsNullOrEmpty(this.SelectedDmodDefinition?.DmodDirectory)) return;

                string exePath = launchEditor
                    ? this.ActiveEditorExePath?.Path ?? string.Empty
                    : this.ActiveGameExePath?.Path ?? string.Empty;
                
                if (string.IsNullOrEmpty(exePath)) return;
                
                string dmodPath = this.SelectedDmodDefinition.DmodDirectory;
                
                this.DmodLauncherWaitingForDelay = true;
                
                this.SaveToConfigLauncher();
                this.SaveActiveIndexToConfigGeneral();

                var extension = this.CfgExtension.TryAddOrGetExtension(exePath);

                // launch dmod with separate task to prevent gui lockup
                await Task.Run(() => {
                    DmodLauncher.LaunchDmod(exePath, launchEditor, dmodPath, this.CfgLaunch, extension, this.SelectedLocalization?.CultureInfo?.Name);
                });
                this._dmodLauncherDelay.Start();
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
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
                bool launchEditor = parameter is string str && str == this.LaunchEditorParameter;
                
                if (this.DmodManager == null) return false;
                //if (!this.GameExeFound) return false;
                if (string.IsNullOrEmpty(this.SelectedDmodDefinition?.DmodDirectory)) return false;

                string exePath;
                
                if (launchEditor) {
                    if (string.IsNullOrEmpty(this.ActiveEditorExePath?.Path)) return false;
                    exePath = this.ActiveEditorExePath.Path;
                } else {
                    if (string.IsNullOrEmpty(this.ActiveGameExePath?.Path)) return false;
                    exePath = this.ActiveGameExePath.Path;
                }
                string dmodPath = this.SelectedDmodDefinition.DmodDirectory;
                //FileInfo finfo = new FileInfo(exePath);
                DirectoryInfo dinfo = new DirectoryInfo(dmodPath);
                //if (finfo.Exists == false || dinfo.Exists == false) { return false; }
                if (dinfo.Exists == false) { return false; }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                return false;
            }

            return true;
        }

        #endregion
        
        #region COMMANDS LAUNCH
        
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
        
        public override bool ProcessKeyDown(Key key, KeyModifiers modifiers) {
            switch (key) {
                case Key.D:
                    if (modifiers != KeyModifiers.Control) return false;
                    this.CmdLaunchDmod();
                    return true;
                
                case Key.E:
                    if (modifiers != KeyModifiers.Control) return false;
                    this.CmdLaunchDmod(this.LaunchEditorParameter);
                    return true;
            }
            return false;
        }
        
    }
}
