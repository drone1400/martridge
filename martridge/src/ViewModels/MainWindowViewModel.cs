using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Metadata;
using Martridge.Models.Dmod;
using Martridge.Trace;
using Martridge.ViewModels.About;
using Martridge.ViewModels.Configuration;
using Martridge.ViewModels.Dmod;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Configuration.General;
using Martridge.Models.DmodInstaller;
using Martridge.Models.DmodPacker;
using Martridge.ViewModels.DinkyAlerts;

#if ENABLE_FEATURE_ONLINE
using Martridge.Models.OnlineDmods;
using Martridge.ViewModels.OnlineDmod;
#endif

#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
using Martridge.ViewModels.DinkInstaller;
#endif

namespace Martridge.ViewModels {
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel() {
            if (Application.Current is App app) {
                app.OnThemePaletteChange += this.AppOnThemePaletteChanged;
            }

            this.RefreshSidePanelImage();
        }
        
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                if (Application.Current is not App app) return;

                app.OnThemePaletteChange -= this.AppOnThemePaletteChanged;
            }
        }

        private void AppOnThemePaletteChanged(object? sender, EventArgs e) {
            this.RefreshSidePanelImage();
        }


        private Config? _config = null;
        private DmodManager? _dmodManager = null;
        
#if ENABLE_FEATURE_ONLINE
        private DmodCrawler? _dmodCrawler = null;

        public bool EnableOnlineFeatures {
            get => this._enableOnlineFeatures;
            private set { 
                this.RaiseAndSetIfChanged(ref this._enableOnlineFeatures, value);
                this.RaisePropertyChanged(nameof(EnableDinkInstallerMenu));
            }
        }
        private bool _enableOnlineFeatures = false;
        private OnlineDmodBrowserViewModel? _onlineDmodBrowserViewModel = null;
#else
        public bool EnableOnlineFeatures => false;
#endif
        
#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
        public bool EnableDinkInstallerMenu => this.EnableOnlineFeatures;
#else 
        public bool EnableDinkInstallerMenu => false;
#endif

        public bool EnableDmodDeveloperFeatures {
            get => this._enableDmodDeveloperFeatures;
            private set => this.RaiseAndSetIfChanged(ref this._enableDmodDeveloperFeatures, value);
        }
        private bool _enableDmodDeveloperFeatures = false;
        
        public bool IsInitialized {
            get => this._isInitialized;
            private set {
                this.RaiseAndSetIfChanged(ref this._isInitialized, value);
                this.RefreshIsViewModelSwitchable();
            }
        }
        private bool _isInitialized = false;

        public ViewModelAppPage? CurrentViewModel {
            get => this._currentViewModel;
            private set {
                this.RaiseAndSetIfChanged(ref this._currentViewModel, value);
                this.RefreshIsViewModelSwitchable();
            }
        }
        private ViewModelAppPage? _currentViewModel = null;

        public bool IsViewModelSwitchable {
            get => this._isViewModelSwitchable;
            private set => this.RaiseAndSetIfChanged(ref this._isViewModelSwitchable, value);
        }
        private bool _isViewModelSwitchable = false;
        
        
        private DmodBrowserViewModel? _dmodBrowserViewModel = null;

        public DinkyAlertViewModel? AlertViewModel {
            get => this._alertViewModel;
            set => this.RaiseAndSetIfChanged(ref this._alertViewModel, value);
        }
        private DinkyAlertViewModel? _alertViewModel = null;

        
        // ------------------------------------------------------------------------------------------
        //      Internal logic 
        //


        public void Initialize(Config appConfig)
        {
            // sanity check if already initialized.. should never happen...
            if (this._config != null)
                return;
            
            this._config = appConfig;
            this._config.General.Updated += this.GeneralOnUpdated;

            this.EnableDmodDeveloperFeatures = this._config.General.ShowDmodDevFeatures;
            
            this._dmodManager = new DmodManager();
            this._dmodManager.Initialize(this._config.General).ContinueWith((_) => {
                try {
                    // initialize view models after dmod manager is done initializing so that the selected DMOD remember feature works properly...
                    
                    // NOTE: unlike the other app page view models, always keep the DMOD browser VM around... 
                    this._dmodBrowserViewModel = new DmodBrowserViewModel();
                    this._dmodBrowserViewModel.InitializeDmodManager(this._dmodManager, true);

                    this.SwapToDefaultViewModel();
            
                    this.IsInitialized = true;
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(ex);
                }
            });
            
                        
#if ENABLE_FEATURE_ONLINE
            this.EnableOnlineFeatures = this._config.General.EnableOnlineFeatures;
            if (this.EnableOnlineFeatures) {
                this.EnsureInitializedOnlineDmodBrowser();
            }
#endif
            
            this.PropertyChanged += this.OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
#if ENABLE_FEATURE_ONLINE
            if (e.PropertyName == nameof(this.EnableOnlineFeatures)) {
                if (this.EnableOnlineFeatures == false) {
                    if (this.CurrentViewModel is OnlineDmodBrowserViewModel) {
                        this.SwapToDefaultViewModel();
                    }

                    if (this._dmodCrawler != null) {
                        this._dmodCrawler.Dispose();
                        this._dmodCrawler = null;
                    }
                    if (this._onlineDmodBrowserViewModel != null) {
                        this._onlineDmodBrowserViewModel.Dispose();
                        this._onlineDmodBrowserViewModel = null;
                    }
                }
                else {
                    // initialize dmod crawler immediately after enabling online features so it updates in the background
                    this.EnsureInitializedOnlineDmodBrowser();
                }
            }
#endif
        }
        private void GeneralOnUpdated(object? sender, ConfigUpdateEventArgs e) {
            if (sender is not ConfigGeneral general) return;
            
            foreach (string name in e.UpdatedProperties) {
                if (name == nameof(ConfigGeneral.AdditionalDmodLocations) ||
                    name == nameof(ConfigGeneral.DefaultDmodLocation) ||
                    name == nameof(ConfigGeneral.GameExePaths)) {
                    this._dmodManager?.Initialize(general);
                } else if (name == nameof(ConfigGeneral.ShowDmodDevFeatures)) {
                    this.EnableDmodDeveloperFeatures = general.ShowDmodDevFeatures;
                } 
#if ENABLE_FEATURE_ONLINE
                else if (name == nameof(ConfigGeneral.EnableOnlineFeatures)) {
                    this.EnableOnlineFeatures = general.EnableOnlineFeatures;
                }
#endif
            }
        }

        //
        // arguments...
        //

        public void InitializeArgs(string[]? args) {
            try {
                if (args != null && args.Length == 1) {
                    string path = args[0];
                    FileInfo finfo = new FileInfo(path);
                    if (finfo.Exists && finfo.Extension.ToLowerInvariant() == ".dmod") {
                        // try to open dmod file?...
                        this.CmdShowPageDmodInstaller(finfo.FullName);
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage("Error initializing arguments");
                MyTrace.Global.WriteException(ex);
            }
        }
        
        #region Theme Image stuff

        public Bitmap? SidePanelImage {
            get => this._sidePanelImage;
            private set => this.RaiseAndSetIfChanged(ref this._sidePanelImage, value);
        }
        private Bitmap? _sidePanelImage = null;

        public BitmapBlendingMode SidePanelImageBlendMode {
            get => this._sidePanelImageBlendMode;
            private set => this.RaiseAndSetIfChanged(ref this._sidePanelImageBlendMode, value);
        }
        private BitmapBlendingMode _sidePanelImageBlendMode = BitmapBlendingMode.Multiply;

        private void RefreshSidePanelImage() {
            Bitmap? newBitmap = null;
            BitmapBlendingMode blendMode = BitmapBlendingMode.Multiply;

            if (App.Instance?.TryGetThemeResource("MartridgeSidePanelImageBlendMode", out object? imgBlend) == true && imgBlend is string strImgBlend) {
                if (Enum.TryParse(strImgBlend, out blendMode) == false) 
                    blendMode = BitmapBlendingMode.Multiply;
            }

            if (App.Instance?.TryGetThemeResource("MartridgeSidePanelImageSource", out object? imgSource) == true && imgSource is string strImgSoruce) {
                try {
                    string imgPath = Path.Combine(LocationHelper.GetPathCustomThemes(), strImgSoruce);
                    newBitmap = new Bitmap(imgPath);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(ex);
                }
            }

            if (newBitmap == null) {
                Uri fallbackUri = new Uri("avares://martridge/Assets/dinkbw.png");
                this.SidePanelImage = ImageHelper.GetImage(fallbackUri);
                this.SidePanelImageBlendMode = BitmapBlendingMode.Multiply;
            }
            else {
                this.SidePanelImage = newBitmap;
                this.SidePanelImageBlendMode = blendMode;
            }
        }
        
        #endregion


        #region Drag and drop

        private void DragOver(object? sender, DragEventArgs e) {
            if (e.Source is Control) {
                e.DragEffects &= (DragDropEffects.Copy); 
            }
        }

        private void Drop(object? sender, DragEventArgs e) {
            try {
                if (e.Source is Control) {
                    e.DragEffects = DragDropEffects.None;
                }

                if (e.DataTransfer.TryGetFiles() is IEnumerable<IStorageItem> files) {
                    IStorageItem? file = files.FirstOrDefault();
                    if (file != null) {
                        this.CmdShowPageDmodInstaller(file.Path.LocalPath);
                    }

                    return;
                }
                
                if (e.DataTransfer.TryGetText() is string text) {
                    Task.Run(() => {
                        try {
                            Task<IStorageFile?>? taskFile = App.Instance?.StorageProvider?.TryGetFileFromPathAsync(text);
                            taskFile?.Wait();
                            if (taskFile?.Result is IStorageFile file) {
                                this.CmdShowPageDmodInstaller(file.Path.LocalPath);
                            }

                        } catch (Exception ex) {
                            MyTrace.Global.WriteException(ex);
                        }
                    });
                }
                
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        public void InitializeDragAndDrop(Control c) {
            c.AddHandler(DragDrop.DropEvent, this.Drop);
            c.AddHandler(DragDrop.DragOverEvent, this.DragOver);
        }
        
        
        #endregion
        
        #region Commands for switching view models

        private void SwapCurrentViewModel(ViewModelAppPage? newViewModel) {
            if (this._currentViewModel != null) {
                if (ReferenceEquals(this._currentViewModel, newViewModel)) {
                    // the newViewModel is already loaded
                    return;
                } else
                if (ReferenceEquals(this._currentViewModel, this._dmodBrowserViewModel)) {
                    // NOTE: the DmodBrowserViewModel uses a DataGridCollectionView to display the DMODs in the DataGrid control
                    // the DataGridCollectionView has an issue where it will keep alive the parent View of the DataGrid for its lifetime
                    // so we need to reinitialize it before we create a new view
                    this._dmodBrowserViewModel.ReinitializeDataGridCollectionView();
                } else
#if ENABLE_FEATURE_ONLINE
                if (ReferenceEquals(this._currentViewModel, this._onlineDmodBrowserViewModel)) {
                    // we need to reinitialize the DataGridCollectionView
                    this._onlineDmodBrowserViewModel.ReinitializeDataGridCollectionView();
                }
                else
#endif
                if (this._currentViewModel is SettingsGeneralViewModel settings) {
                    // apply settings...
                    settings.CmdSettingsOk();
                    settings.Dispose();
                } else
                {
                    // dispose current view model
                    this._currentViewModel?.Dispose();
                }
            }
            
            this.CurrentViewModel = newViewModel;
        }
        
        
        private void SwapToDefaultViewModel() {

            ViewModelAppPage? newViewModel = this._dmodBrowserViewModel;
            
            // make sure settings get applied if swapping here from settings VM
            if (this._currentViewModel is SettingsGeneralViewModel settings) {
                // apply settings...
                settings.CmdSettingsOk();
                // dispose old view model
                settings.Dispose();
            } else
            {
                // dispose current view model if needed
                this._currentViewModel?.Dispose();
            }
            
            // check we have a valid game or editor exe
            if (this._config != null &&
                this._config.General.GameExePaths.Count == 0 &&
                (this._config.General.EditorExePaths.Count == 0 || this._config.General.ShowDmodDevFeatures == false)) {
                
#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
                if (this.EnableOnlineFeatures) {
                    NoDinkyViewModel vm = new NoDinkyViewModel();
                    vm.ShowConfigurationPageRequested += (_, _) => {
                        this.CmdShowPageSettings();
                    };
                    vm.ShowDinkInstallerPageRequested += (_, _) => {
                        this.CmdShowPageDinkInstaller();
                    };
                    newViewModel = vm;
                }
                else
#endif
                {
                    // for linux/mac or windows without online features...
                    NoDinkyLinuxViewModel vml = new NoDinkyLinuxViewModel();
                    vml.ShowConfigurationPageRequested += (_, _) => {
                        this.CmdShowPageSettings();
                    };
                    newViewModel = vml;
                }
            }

#if ENABLE_FEATURE_ONLINE
            // initialize online dmod browser if needed
            if (this._enableOnlineFeatures) {
                this.EnsureInitializedOnlineDmodBrowser();
            }
#endif
            
            // load dmod browser view model
            this.SwapCurrentViewModel(newViewModel);
        }
        
#if ENABLE_FEATURE_ONLINE
        private void EnsureInitializedOnlineDmodBrowser() {
            if (this.EnableOnlineFeatures == false)
                return;
            
            if (this._dmodCrawler == null) {
                this._dmodCrawler = new DmodCrawler();
                Task.Run(async () =>
                {
                    try {
                        await this._dmodCrawler.InitializeDmodLists(false);
                        
                        if (this._config == null) 
                            return;

                        if (this._config.General.OnlineDmodListAutoRefreshDays <= 0 ||
                            double.IsNaN(this._config.General.OnlineDmodListAutoRefreshDays))
                            return;

                        if ((DateTime.Now - this._dmodCrawler.DmodPagesOldestWriteTime).TotalDays >= this._config.General.OnlineDmodListAutoRefreshDays) {
                            // if the DMOD page data is too old, force online refresh
                            await this._dmodCrawler.InitializeDmodLists(true);
                        }
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(ex);
                    }
                });
                
            }
            
            if (this._onlineDmodBrowserViewModel == null) {
                this._onlineDmodBrowserViewModel = new OnlineDmodBrowserViewModel();
                this._onlineDmodBrowserViewModel.DmodCrawler = this._dmodCrawler;
                this._onlineDmodBrowserViewModel.InstallDmodRequested += (_, args) => {
                    this.CmdShowPageDmodInstaller(args.Path);
                };
            }
        }
#endif
        
        private void RefreshIsViewModelSwitchable() {
            if (this.IsInitialized == false) {
                this.IsViewModelSwitchable = false;
                return;
            }

            switch (this.CurrentViewModel?.GetType().Name) {
                default:
                    this.IsViewModelSwitchable = false;
                    return;
#if ENABLE_FEATURE_ONLINE
                case nameof(OnlineDmodBrowserViewModel):
#endif
                case nameof(NoDinkyViewModel):
                case nameof(NoDinkyLinuxViewModel):
                case nameof(DmodBrowserViewModel):
                case nameof(SettingsGeneralViewModel):
                case nameof(SettingsThemeViewModel):
                case nameof(AboutViewModel):
                    this.IsViewModelSwitchable = true;
                    return;
#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
                case nameof(DinkInstallerViewModel):
                    this.IsViewModelSwitchable = 
                        ((DinkInstallerViewModel)this.CurrentViewModel).IsInstallerStarted == false ||
                        ((DinkInstallerViewModel)this.CurrentViewModel).IsInstallerFinished == true || 
                        ((DinkInstallerViewModel)this.CurrentViewModel).IsInstallerCancelled == true;
                    return;
#endif
                case nameof(DmodInstallerViewModel):
                    this.IsViewModelSwitchable =
                        ((DmodInstallerViewModel)this.CurrentViewModel).InstallPhase == DmodInstallPhase.Inactive ||
                        ((DmodInstallerViewModel)this.CurrentViewModel).InstallPhase == DmodInstallPhase.Finished;
                    return;
                case nameof(DmodPackerViewModel):
                    this.IsViewModelSwitchable = 
                        ((DmodPackerViewModel)this.CurrentViewModel).PackerPhase == DmodPackerPhase.Inactive ||
                        ((DmodPackerViewModel)this.CurrentViewModel).PackerPhase == DmodPackerPhase.Finished;
                    return;
            }
        }

        // ----------------------------------------------------------------------------------------------------------------------------
        
        
        // ----------------------------------------------------------------------------------------------------------------------------
        // AboutViewModel
        //
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageAbout(object? parameter = null) 
            => this.CurrentViewModel is AboutViewModel || this.IsViewModelSwitchable;
        public void CmdShowPageAbout(object? parameter = null) {
            if (this.CurrentViewModel is AboutViewModel) return; // already the correct view model
            if (this.CanCmdShowPageAbout() == false) return;
            
            try {
                AboutViewModel vm = new AboutViewModel();
                vm.Configuration = this._config!.General;

                this.SwapCurrentViewModel(vm);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        // ----------------------------------------------------------------------------------------------------------------------------
        
        
        // ----------------------------------------------------------------------------------------------------------------------------
        // SettingsThemeViewModel
        //
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageSettingsTheme(object? parameter = null) 
            => this.CurrentViewModel is SettingsThemeViewModel || this.IsViewModelSwitchable;
        public void CmdShowPageSettingsTheme(object? parameter = null) {
            if (this.CurrentViewModel is SettingsThemeViewModel) return; // already the correct view model
            if (this.CanCmdShowPageSettingsTheme() == false) return;
            
            try {
                SettingsThemeViewModel vm = new SettingsThemeViewModel();

                this.SwapCurrentViewModel(vm);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        
        // ----------------------------------------------------------------------------------------------------------------------------
        

        // ----------------------------------------------------------------------------------------------------------------------------
        // SettingsGeneralViewModel
        // 
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageSettings(object? parameter = null) 
            => this.CurrentViewModel is SettingsGeneralViewModel || this.IsViewModelSwitchable;
        public void CmdShowPageSettings(object? parameter = null) {
            if (this.CurrentViewModel is SettingsGeneralViewModel) return; // already the correct view model
            if (this.CanCmdShowPageSettings() == false) return;
            
            try {
                SettingsGeneralViewModel vm = new SettingsGeneralViewModel();
                this.SwapCurrentViewModel(vm);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        // ----------------------------------------------------------------------------------------------------------------------------
        

        // ----------------------------------------------------------------------------------------------------------------------------
        // DinkInstallerViewModel
        // 
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        [DependsOn(nameof(EnableOnlineFeatures))]
        public bool CanCmdShowPageDinkInstaller(object? parameter = null) {
#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
            return this.CurrentViewModel is DinkInstallerViewModel || (this.IsViewModelSwitchable && this.EnableOnlineFeatures);
#else
            return false;
#endif
        }
        public void CmdShowPageDinkInstaller(object? parameter = null) {
#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
            if (this.CurrentViewModel is DinkInstallerViewModel) return; // already the correct view model
            if (this.CanCmdShowPageDinkInstaller() == false) return;
            
            try {
                DinkInstallerViewModel vm = new DinkInstallerViewModel();
                _ = vm.InitializeInstallerList(forceRecache:true); // do not await

                vm.PropertyChanged += (_, args) => {
                    try {
                        if (args.PropertyName == nameof(DinkInstallerViewModel.IsInstallerStarted) || 
                            args.PropertyName == nameof(DinkInstallerViewModel.IsInstallerFinished) || 
                            args.PropertyName == nameof(DinkInstallerViewModel.IsInstallerCancelled)) {
                            this.RefreshIsViewModelSwitchable();
                        }
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(ex);
                    }
                };
                
                vm.InstallerDone += (_, args) => {
                    try {
                        // if DINK installed successfully, update things...
                        // try to update exe path in settings...
                        if (args.Result == DinkInstallerResult.Success &&
                            args.UsedInstaller != null &&
                            args.Destination != null) {
                            // update game exe paths
                            if (string.IsNullOrWhiteSpace(args.UsedInstaller.GameFileName) == false) {
                                string pathGame = Path.Combine(args.Destination.FullName, args.UsedInstaller.GameFileName);
                                if (File.Exists(pathGame)) {
                                    this._config!.General.TryAddGameExePath(pathGame);
                                }
                            }
                            // update editor exe paths
                            if (string.IsNullOrWhiteSpace(args.UsedInstaller.EditorFileName) == false) {
                                string pathGame = Path.Combine(args.Destination.FullName, args.UsedInstaller.EditorFileName);
                                if (File.Exists(pathGame)) {
                                    this._config!.General.TryAddEditorExePath(pathGame);
                                }
                            }
                        }

                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(ex);
                    }
                    
                    this.SwapToDefaultViewModel();
                };

                this.SwapCurrentViewModel(vm);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
#endif
        }
        // ----------------------------------------------------------------------------------------------------------------------------
        

        // ----------------------------------------------------------------------------------------------------------------------------
        // DmodInstallerViewModel
        // 
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageDmodInstallerAndBrowse(object? parameter = null) 
            => this.CurrentViewModel is DmodInstallerViewModel || this.IsViewModelSwitchable;
        public void CmdShowPageDmodInstallerAndBrowse(object? parameter = null) {
            if (this.CurrentViewModel is DmodInstallerViewModel) return; // already the correct view model
            if (this.CanCmdShowPageDmodInstallerAndBrowse() == false) return;
            this.ShowDmodInstallerCommon(parameter, true);
        }
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageDmodInstaller(object? parameter = null) 
            => this.CurrentViewModel is DmodInstallerViewModel || this.IsViewModelSwitchable;
        public void CmdShowPageDmodInstaller(object? parameter = null) {
            if (this.CurrentViewModel is DmodInstallerViewModel) return; // already the correct view model
            if (this.CanCmdShowPageDmodInstaller() == false) return;
            this.ShowDmodInstallerCommon(parameter, false);
        }
        private void ShowDmodInstallerCommon(object? parameter = null, bool browseDmodImmediately = false) {
            try {
                DmodInstallerViewModel vm = new DmodInstallerViewModel();
                if (parameter is string path && string.IsNullOrWhiteSpace(path) == false && File.Exists(path)) {
                    vm.TemporaryDmodSource = path;
                    // since we are already passing a path to a file that exists, start initializing it immediately
                    vm.CmdInitializeDmod(path);
                }
                
                vm.PropertyChanged += (_, args) => {
                    try {
                        if (args.PropertyName == nameof(DmodInstallerViewModel.InstallPhase)) {
                            this.RefreshIsViewModelSwitchable();
                        }
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(ex);
                    }
                };
                
                vm.InstallerDone += (_, args) => {
                    this.SwapToDefaultViewModel();
                    
                    // if DMOD installed successfully, reinitialize dmod manager lists...
                    if (args.Result == DinkInstallerResult.Success) {
                        // refresh dmods...
                        this._dmodManager?.Initialize(this._config!.General).ContinueWith((_) =>
                        {
                            // select DMOD after installing!
                            this._dmodBrowserViewModel?.SelectDmodByPath(args.Destination?.FullName ?? string.Empty);
                        });
                    }
                };

                this.SwapCurrentViewModel(vm);

                // only browse for DMOD if the DMOD source has not been initialized by CfgRemember ...
                if (browseDmodImmediately) {
                    vm.CmdBrowseDmod();
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        // ----------------------------------------------------------------------------------------------------------------------------
        
        
        // ----------------------------------------------------------------------------------------------------------------------------
        // DmodPackerViewModel
        // 
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageDmodPackerAndBrowse(object? parameter = null) 
            => this.CurrentViewModel is DmodPackerViewModel || this.IsViewModelSwitchable;
        public void CmdShowPageDmodPackerAndBrowse(object? parameter = null) {
            if (this.CurrentViewModel is DmodPackerViewModel) return; // already the correct view model
            if (this.CanCmdShowPageDmodPackerAndBrowse(parameter) == false) return;
            this.ShowDmodPackerCommon(parameter, true);
        }
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageDmodPacker(object? parameter = null) 
            => this.CurrentViewModel is DmodPackerViewModel || this.IsViewModelSwitchable;
        public void CmdShowPageDmodPacker(object? parameter = null) {
            if (this.CurrentViewModel is DmodPackerViewModel) return; // already the correct view model
            if (this.CanCmdShowPageDmodPacker(parameter) == false) return; // can't switch
            this.ShowDmodPackerCommon(parameter, false);
        }
        private void ShowDmodPackerCommon(object? parameter = null, bool browseDmodImmediately = false) {
            try {
                DmodPackerViewModel vm = new DmodPackerViewModel();
                if (parameter is string path && string.IsNullOrWhiteSpace(path) == false) {
                    vm.TemporaryDmodSourceDirectory = path;
                }
                
                vm.PropertyChanged += (_, args) => {
                    try {
                        if (args.PropertyName == nameof(DmodPackerViewModel.PackerPhase)) {
                            this.RefreshIsViewModelSwitchable();
                        }
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(ex);
                    }
                };
                
                vm.PackerDone += (_, _) => {
                    this.SwapToDefaultViewModel();
                };

                this.SwapCurrentViewModel(vm);

                if (browseDmodImmediately) {
                    vm.CmdBrowseDmodSource();
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        // ----------------------------------------------------------------------------------------------------------------------------
        
        
        // ----------------------------------------------------------------------------------------------------------------------------
        // DmodBrowserViewModel
        // 
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageMyDmods(object? parameter = null) 
            => this.CurrentViewModel is DmodBrowserViewModel || this.IsViewModelSwitchable;
        public void CmdShowPageMyDmods(object? parameter = null) {
            if (this.CurrentViewModel is DmodBrowserViewModel) return; // already the correct view model
            if (this.CanCmdShowPageMyDmods() == false) return; // can't switch
            
            try {
                this.SwapToDefaultViewModel();
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        // ----------------------------------------------------------------------------------------------------------------------------
        
        
        // ----------------------------------------------------------------------------------------------------------------------------
        // OnlineDmodBrowserViewModel
        // 
#if ENABLE_FEATURE_ONLINE
        [DependsOn(nameof(EnableOnlineFeatures))]
        [DependsOn(nameof(IsViewModelSwitchable))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageOnlineDmods(object? parameter = null) 
            => this.EnableOnlineFeatures && (this.CurrentViewModel is OnlineDmodBrowserViewModel || this.IsViewModelSwitchable);
        public void CmdShowPageOnlineDmods(object? parameter = null) {
            if (this.EnableOnlineFeatures == false) return;
            if (this.CurrentViewModel is OnlineDmodBrowserViewModel) return; // already the correct view model
            if (this.CanCmdShowPageMyDmods() == false) return; // can't switch
            
            try {
                this.EnsureInitializedOnlineDmodBrowser();
                this.SwapCurrentViewModel(this._onlineDmodBrowserViewModel);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
#endif
        // ----------------------------------------------------------------------------------------------------------------------------
        
        #endregion

        #region Other commands
        
        //
        // NOTE: rider may say these are unused, but they are used in bindings over in DmodBrowserDmodListView.axaml
        //
        public bool CanCmdOpenLocation(object? parameter) {
            if (parameter is not string path) return false;
            if (File.Exists(path) || Directory.Exists(path)) return true;
            return false;
        }
        public void CmdOpenLocation(object? parameter) {
            if (parameter is not string path) return;

            string targetPath = "";

            if (File.Exists(path)) {
                // if path is a file, open its directory
                FileInfo finfo = new FileInfo(path);
                if (finfo.Exists && finfo.Directory?.Exists == true) {
                    targetPath = finfo.Directory.FullName;
                }
            } else if (Directory.Exists(path)) {
                targetPath = path;
            }

            if (string.IsNullOrWhiteSpace(targetPath)) return;

            // this is to make sure to open the directory and not a file with the same name...
            targetPath += Path.DirectorySeparatorChar + ".";
            
            // open directory...
            ProcessStartInfo pinfo = new ProcessStartInfo(targetPath) {
                UseShellExecute = true,
                Verb = "open",
            };
            Process.Start(pinfo);
        }
        
        #endregion
        
        #region Keyboard input


        public bool ProcessKeyDown(Key key, KeyModifiers modifiers) {
            // NOTE about the page swapping commands:
            // validation that the command can be executed is done within each of the page switching function
            
            switch (key) {
                case Key.F1:
                    this.CmdShowPageAbout();
                    return true;
                case Key.F2:
#if ENABLE_FEATURE_ONLINE                
                    if (this.EnableOnlineFeatures && this._currentViewModel != null && ReferenceEquals(this._currentViewModel, this._dmodBrowserViewModel)) {
                        // swap to online view model
                        this.CmdShowPageOnlineDmods();
                        return true;
                    }
#endif
                    this.CmdShowPageMyDmods();
                    return true;
                case Key.F3:
                    this.CmdShowPageSettings();
                    return true;
                case Key.F4:
                    this.CmdShowPageSettingsTheme();
                    return true;
                case Key.F5:
                    this.CmdShowPageDmodInstallerAndBrowse();
                    return true;
                case Key.F6:
                    if (this.EnableDmodDeveloperFeatures) {
                        this.CmdShowPageDmodPackerAndBrowse();
                    }
                    return true;
                case Key.F7:
#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
                    this.CmdShowPageDinkInstaller();
#endif
                    return true;
                case Key.L:
                    if ((modifiers & KeyModifiers.Control) != 0) {
                        App.Instance?.ShowLogWindow();
                        return true;
                    }
                    return false;
            }
            // let the current view model process the key input...
            return this._currentViewModel?.ProcessKeyDown(key, modifiers) ?? false;
        }
        
        #endregion


    }
}
