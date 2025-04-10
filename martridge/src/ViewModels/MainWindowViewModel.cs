using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Metadata;
using Martridge.Models.Dmod;
using Martridge.Models.Installer;
using Martridge.Trace;
using Martridge.ViewModels.About;
using Martridge.ViewModels.Configuration;
using Martridge.ViewModels.Dmod;
using Martridge.ViewModels.Installer;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Martridge.Models.Configuration;
using Martridge.Models.OnlineDmods;

namespace Martridge.ViewModels {
    public class MainWindowViewModel : ViewModelBase
    {

        private Config? _config = null;
        private DmodManager? _dmodManager = null;
        private DmodCrawler? _dmodCrawler = null;

        public bool EnableDmodDeveloperFeatures {
            get => this._enableDmodDeveloperFeatures;
            private set => this.RaiseAndSetIfChanged(ref this._enableDmodDeveloperFeatures, value);
        }
        private bool _enableDmodDeveloperFeatures = false;
        
        // TODO.. this will be set from config later...
        public bool EnableOnlineFeatures {
            get => this._enableOnlineFeatures;
            private set => this.RaiseAndSetIfChanged(ref this._enableOnlineFeatures, value);
        }
        private bool _enableOnlineFeatures = false;
        
        public bool IsInitialized {
            get => this._isInitialized;
            private set => this.RaiseAndSetIfChanged(ref this._isInitialized, value);
        }
        private bool _isInitialized = false;

        public ViewModelAppPage? CurrentViewModel {
            get => this._currentViewModel;
            private set => this.RaiseAndSetIfChanged(ref this._currentViewModel, value);
        }
        private ViewModelAppPage? _currentViewModel = null;

        private ViewModelAppPage? _previousViewModel = null;

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
            this.EnableOnlineFeatures = this._config.General.EnableOnlineFeatures;
            
            this._dmodManager = new DmodManager();
            this._dmodManager.Initialize(this._config.General);

            if (this.EnableOnlineFeatures) {
                this._dmodCrawler = new DmodCrawler();
                this._dmodCrawler.InitializeDmodLists(false); // no await
            }

            this.InitializeMainViewModel();
            
            this.IsInitialized = true;
            
            this.PropertyChanged += this.OnPropertyChanged;
        }
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.EnableOnlineFeatures)) {
                if (this.EnableOnlineFeatures == false) {
                    // TODO make sure no online features are being used...
                    if (this._previousViewModel is DualDmodBrowserViewModel or OnlineDmodBrowserViewModel) {
                        this._previousViewModel?.Dispose();
                        this._previousViewModel = null;
                    }

                    if (this.CurrentViewModel is DualDmodBrowserViewModel or OnlineDmodBrowserViewModel) {
                        this.InitializeMainViewModel();
                    }

                    if (this._dmodCrawler != null) {
                        // TODO.. make DMOD Crawler disposable and dispose of it here?...
                        this._dmodCrawler = null;
                    }
                }
                else {
                    // make sure we are using online features...
                    if (this._previousViewModel is DmodBrowserViewModel) {
                        this._previousViewModel?.Dispose();
                        this._previousViewModel = null;
                    }
                    if (this.CurrentViewModel is DmodBrowserViewModel) {
                        this.InitializeMainViewModel();
                    }

                    if (this._dmodCrawler == null) {
                        this._dmodCrawler = new DmodCrawler();
                        this._dmodCrawler.InitializeDmodLists(false); // no await
                    }
                }
                
            }
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
                } else if (name == nameof(ConfigGeneral.EnableOnlineFeatures)) {
                    this.EnableOnlineFeatures = general.EnableOnlineFeatures;
                }
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
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Error initializing arguments");
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }


        #region Drag and drop

        // TODO... fix this

        private void DragOver(object? sender, DragEventArgs e) {
            if (e.Source is Control c && c.Name == "DmodBrowserView") {
                e.DragEffects = e.DragEffects & (DragDropEffects.Copy); 
            }

            // Only allow if the dragged data contains filenames.
            if (!e.Data.Contains(DataFormats.FileNames)) {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private void Drop(object? sender, DragEventArgs e) {
            if (e.Source is Control c && c.Name == "DmodBrowserView") {
                e.DragEffects = e.DragEffects & (DragDropEffects.Copy);
            }

            if (e.Data.Contains(DataFormats.FileNames)) {
                IEnumerable<string>? files = e.Data.GetFileNames();
                if (files != null) {
                    string file = files.First();
                    FileInfo finfo = new FileInfo(file);
                    if (finfo.Exists && finfo.Extension.ToLowerInvariant() == ".dmod") {
                        this.CmdShowPageDmodInstaller(finfo.FullName);
                    }
                }
            }
        }

        public void InitializeDragAndDrop(Control c) {
            c.AddHandler(DragDrop.DropEvent, this.Drop);
            c.AddHandler(DragDrop.DragOverEvent, this.DragOver);
        }
        
        
        #endregion
        
        #region Commands for switching view models

        private void SaveCurrentViewModel() {
            this._previousViewModel?.Dispose();
            this._previousViewModel = this.CurrentViewModel;
        }
        
        private void InitializeMainViewModel() {
            
            // make sure current view model is gone first...
            this.CurrentViewModel?.Dispose();
            this.CurrentViewModel = null;

            if (this._config!.General.GameExePaths.Count == 0) {
#if PLATF_WINDOWS
                if (this.EnableOnlineFeatures) {
                    NoDinkyViewModel vm = new NoDinkyViewModel();
                    vm.ShowConfigurationPageRequested += (_, _) => {
                        this.CmdShowPageSettings();
                    };
                    vm.ShowDinkInstallerPageRequested += (_, _) => {
                        this.CmdShowPageDinkInstaller();
                    };
                    this.CurrentViewModel = vm;
                    return;
                }
#endif
                // for linux/mac or windows without online features...
                NoDinkyLinuxViewModel vml = new NoDinkyLinuxViewModel();
                vml.ShowConfigurationPageRequested += (_, _) => {
                    this.CmdShowPageSettings();
                };
                this.CurrentViewModel = vml;
                return;
            }
            
            if (this._enableOnlineFeatures) {
                DmodBrowserViewModel dbVm = new DmodBrowserViewModel();
                dbVm.DmodManager = this._dmodManager; // NOTE: initialize this first or the remembered selected DMOD won't be restored
                dbVm.CfgGeneral = this._config?.General;
                dbVm.CfgLaunch = this._config?.Launch;
                dbVm.CfgRemember = this._config?.Remember;

                OnlineDmodBrowserViewModel odbVm = new OnlineDmodBrowserViewModel();
                odbVm.DmodCrawler = this._dmodCrawler;
                odbVm.InstallDmodRequested += (_, args) => {
                    this.CmdShowPageDmodInstaller(args.Path);
                };

                DualDmodBrowserViewModel vm = new DualDmodBrowserViewModel();
                vm.DmodBrowserVm = dbVm;
                vm.OnlineDmodBrowserVm = odbVm;

                this.CurrentViewModel = vm;
            }
            else {
                DmodBrowserViewModel dbVm = new DmodBrowserViewModel();
                dbVm.DmodManager = this._dmodManager; // NOTE: initialize this first or the remembered selected DMOD won't be restored
                dbVm.CfgGeneral = this._config?.General;
                dbVm.CfgLaunch = this._config?.Launch;
                dbVm.CfgRemember = this._config?.Remember;
                    
                this.CurrentViewModel = dbVm;
            }
        }

        private void RestorePreviousViewModel() {
            
            this.CurrentViewModel?.Dispose();
            this.CurrentViewModel = null;

            if (this._previousViewModel != null) {
                if (this._previousViewModel is NoDinkyViewModel or NoDinkyLinuxViewModel) {
                    if (this._config!.General.GameExePaths.Count > 0) {
                        // we now have the Dinky!
                        this._previousViewModel.Dispose();
                        this._previousViewModel = null;

                        this.InitializeMainViewModel();
                        return;
                    }
                }
                this.CurrentViewModel = this._previousViewModel;
                this._previousViewModel = null;
            }
            else {
                this.InitializeMainViewModel();
            }
        }
        
        [DependsOn(nameof(IsInitialized))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageAbout(object? parameter = null) => this.CanSwitchViewModel();
        public void CmdShowPageAbout(object? parameter = null) {
            if (this.CanCmdShowPageAbout() == false) return;
            
            try {
                this.SaveCurrentViewModel();
                
                AboutViewModel vm = new AboutViewModel();
                vm.Configuration = this._config!.General;
                vm.GoBackRequested += (_, _) => {
                    // return to previous view model...
                    // NOTE: this should also clean up the current view model...
                    this.RestorePreviousViewModel();
                };

                this.CurrentViewModel = vm;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }
        
        [DependsOn(nameof(IsInitialized))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageSettings(object? parameter = null) => this.CanSwitchViewModel();
        public void CmdShowPageSettings(object? parameter = null) {
            if (this.CanCmdShowPageSettings() == false) return;
            
            try {
                this.SaveCurrentViewModel();
                
                SettingsGeneralViewModel vm = new SettingsGeneralViewModel();
                vm.CfgGeneral = this._config?.General;
                vm.SettingsDone += (_, _) => {
                    // return to previous view model...
                    // NOTE: this should also clean up the current view model...
                    this.RestorePreviousViewModel();
                };

                this.CurrentViewModel = vm;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }

        [DependsOn(nameof(IsInitialized))]
        [DependsOn(nameof(CurrentViewModel))]
        [DependsOn(nameof(EnableOnlineFeatures))]
        public bool CanCmdShowPageDinkInstaller(object? parameter = null) {
            #if PLATF_WINDOWS
            return this.CanSwitchViewModel() && this.EnableOnlineFeatures;
            #endif
            return false;
        }
        public void CmdShowPageDinkInstaller(object? parameter = null) {
            if (this.CanCmdShowPageDinkInstaller() == false) return;
            
#if PLATF_WINDOWS
            try {
                this.SaveCurrentViewModel();

                DinkInstallerViewModel vm = new DinkInstallerViewModel();
                vm.InitializeInstallerList(this._config!.General.AutoUpdateInstallerList);
                vm.InstallerDone += (_, args) => {
                    // return to previous view model...
                    // NOTE: this should also clean up the current view model...
                    this.RestorePreviousViewModel();
                    
                    // if DINK installed successfully, update things...
                    // try to update exe path in settings...
                    if (args.Result == DinkInstallerResult.Success && 
                        args.UsedInstaller != null && 
                        args.Destination != null) {
                        if (string.IsNullOrWhiteSpace(args.UsedInstaller.GameFileName) == false) {
                            string pathGame = Path.Combine(args.Destination.FullName, args.UsedInstaller.GameFileName);
                            if (File.Exists(pathGame)) {
                                this._config!.General.AddGameExePath(pathGame);
                            }
                        }
                        if (string.IsNullOrWhiteSpace(args.UsedInstaller.EditorFileName) == false) {
                            string pathGame = Path.Combine(args.Destination.FullName, args.UsedInstaller.EditorFileName);
                            if (File.Exists(pathGame)) {
                                this._config!.General.AddEditorExePath(pathGame);
                            }
                        }
                    }
                };

                this.CurrentViewModel = vm;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
#endif
        }

        [DependsOn(nameof(IsInitialized))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageDmodInstallerAndBrowse(object? parameter = null) => this.CanSwitchViewModel();
        public void CmdShowPageDmodInstallerAndBrowse(object? parameter = null) {
            if (this.CanCmdShowPageDmodInstallerAndBrowse() == false) return;
            this.ShowDmodInstallerCommon(parameter, true);
        }

        [DependsOn(nameof(IsInitialized))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageDmodInstaller(object? parameter = null) => this.CanSwitchViewModel();
        public void CmdShowPageDmodInstaller(object? parameter = null) {
            if (this.CanCmdShowPageDmodInstaller() == false) return;
            this.ShowDmodInstallerCommon(parameter, false);
        }
        

        private void ShowDmodInstallerCommon(object? parameter = null, bool browseDmodImmediately = false) {
            try {
                this.SaveCurrentViewModel();

                DmodInstallerViewModel vm = new DmodInstallerViewModel();
                vm.CfgGeneral = this._config?.General;
                vm.CfgRemember = this._config?.Remember;
                if (parameter is string path && string.IsNullOrWhiteSpace(path) == false) {
                    vm.TemporaryDmodSource = path;
                }
                vm.InstallerDone += (_, args) => {
                    // return to previous view model...
                    // NOTE: this should also clean up the current view model...
                    this.RestorePreviousViewModel();
                    
                    // if DMOD installed successfully, reinitialize dmod manager lists...
                    if (args.Result == DinkInstallerResult.Success) {
                        // refresh dmods...
                        this._dmodManager?.Initialize(this._config!.General);
                    }
                };

                this.CurrentViewModel = vm;

                // only browse for DMOD if the DMOD source has not been initialized by CfgRemember ...
                if (browseDmodImmediately) {
                    vm.CmdBrowseDmod();
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }
        
        [DependsOn(nameof(IsInitialized))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageDmodPackerAndBrowse(object? parameter = null) => this.CanSwitchViewModel();
        public void CmdShowPageDmodPackerAndBrowse(object? parameter = null) {
            if (this.CanCmdShowPageDmodPackerAndBrowse(parameter) == false) return;
            this.ShowDmodPackerCommon(parameter, true);
        }

        
        [DependsOn(nameof(IsInitialized))]
        [DependsOn(nameof(CurrentViewModel))]
        public bool CanCmdShowPageDmodPacker(object? parameter = null) => this.CanSwitchViewModel();
        public void CmdShowPageDmodPacker(object? parameter = null) {
            if (this.CanCmdShowPageDmodPacker(parameter) == false) return;
            this.ShowDmodPackerCommon(parameter, false);
        }

        private void ShowDmodPackerCommon(object? parameter = null, bool browseDmodImmediately = false) {
            try {
                this.SaveCurrentViewModel();

                DmodPackerViewModel vm = new DmodPackerViewModel();
                vm.CfgGeneral = this._config?.General;
                vm.CfgRemember = this._config?.Remember;
                if (parameter is string path && string.IsNullOrWhiteSpace(path) == false) {
                    vm.TemporaryDmodSourceDirectory = path;
                }
                vm.PackerDone += (_, _) => {
                    // return to previous view model...
                    // NOTE: this should also clean up the current view model...
                    this.RestorePreviousViewModel();
                };

                this.CurrentViewModel = vm;

                if (browseDmodImmediately) {
                    vm.CmdBrowseDmodSource();
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }

        private bool CanSwitchViewModel() {
            if (this.IsInitialized == false) 
                return false;
            
            switch (this.CurrentViewModel?.GetType().Name) {
                default: return false;
                case nameof(NoDinkyViewModel):
                case nameof(NoDinkyLinuxViewModel):
                case nameof(DualDmodBrowserViewModel):
                case nameof(DmodBrowserViewModel):
                case nameof(OnlineDmodBrowserViewModel):
                case nameof(AboutViewModel):
                    return true;
            }
        }
        
        #endregion

        #region Other commands

        public void CmdChangeTheme(object? parameter) {
            string? themeName = null;
            if (parameter is ApplicationTheme themeValue) themeName = themeValue.ToString();
            if (parameter is string themeStr) themeName = themeStr;

            if (themeName == null) return;
            
            // update in configuration...
            if (Application.Current is not App app) return;
            app.SetCitrusThemePalette(themeName);
        }
        
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
        
        public void CmdShowLogWindow(object? parameter = null) {
            App.Instance?.ShowLogWindow();
        }
        
        #endregion



    }
}
