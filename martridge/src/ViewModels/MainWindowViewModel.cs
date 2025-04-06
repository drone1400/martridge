using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Metadata;
using Martridge.Models;
using Martridge.Models.Dmod;
using Martridge.Models.Installer;
using Martridge.Trace;
using Martridge.ViewModels.About;
using Martridge.ViewModels.Configuration;
using Martridge.ViewModels.DinkyAlerts;
using Martridge.ViewModels.DinkyGraphics;
using Martridge.ViewModels.Dmod;
using Martridge.ViewModels.Installer;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Martridge.Models.Configuration;
using Martridge.Models.DmodInstaller;
using Martridge.Models.DmodPacker;
using Martridge.Models.OnlineDmods;

namespace Martridge.ViewModels {
    public class MainWindowViewModel : ViewModelBase
    {

        private Config? _config = null;
        private DmodManager? _dmodManager = null;
        private DmodCrawler? _dmodCrawler = null;
        
        public MainWindowViewModel()
        {
            
        }

        public AnimatedDinkGraphicViewModel AnimatedDuckWizardLeft {
            get => DinkyAlert.AnimatedDuckWizardLeft;
        }
        
        public AnimatedDinkGraphicViewModel AnimatedDuckWizardRight {
            get => DinkyAlert.AnimatedDuckWizardRight;
        }

        // ------------------------------------------------------------------------------------------
        //      Internal logic 
        //

        public MainViewPage ActiveUserPage {
            get => this._activeUserPage;
            private set {
                this.RaiseAndSetIfChanged(ref this._activeUserPage, value);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Switched MainWindowView active page to {this.ActiveUserPage}");
            }
        }
        private MainViewPage _activeUserPage = MainViewPage.MainView;


        public SettingsGeneralViewModel VmGeneralSettings { get; } = new SettingsGeneralViewModel();

        public DmodBrowserViewModel VmDmodBrowser { get; } = new DmodBrowserViewModel();
        public OnlineDmodBrowserViewModel VmOnlineDmodBrowser { get; } = new OnlineDmodBrowserViewModel();
        
        public DinkInstallerViewModel VmDinkInstaller { get; } = new DinkInstallerViewModel();
        public DmodInstallerViewModel VmDmodInstaller { get; } = new DmodInstallerViewModel();
        public DmodPackerViewModel VmDmodPacker { get; } = new DmodPackerViewModel();

        public AboutWindowViewModel VmAboutWindow { get; } = new AboutWindowViewModel();

        public void Initialize(Config appConfig)
        {
            // sanity check if already initialized.. should never happen...
            if (this._config != null)
                return;
            
            this._config = appConfig;
            
            this._dmodManager = new DmodManager();
            this._dmodManager.Initialize(this._config.General);
            
            this._dmodCrawler = new DmodCrawler();
            this._dmodCrawler.InitializeDmodLists(false); // no await

            this.VmGeneralSettings.Configuration = this._config.General;
            this.VmGeneralSettings.SettingsDone += this.VmGeneralSettingsOnSettingsDone;
            
            this.VmDinkInstaller.InstallerDone += this.VmDinkInstallerOnInstallerDone;

            this.VmDmodInstaller.InstallerDone += this.VmDmodInstallerOnInstallerDone;
            this.VmDmodInstaller.InitializeConfiguration(this._config.General);
            
            this.VmDmodPacker.PackerDone += this.VmDmodPackerOnInstallerDone;
            
            this.VmDmodBrowser.Configuration = this._config;
            this.VmDmodBrowser.DmodManager = this._dmodManager;
            
            this.VmOnlineDmodBrowser.DmodCrawler = this._dmodCrawler;
            this.VmOnlineDmodBrowser.MainVm = this;

            this.VmAboutWindow.Configuration = this._config.General;

            MyTrace.Global.WriteMessage(MyTraceCategory.General, $"App Path = \"{LocationHelper.AppBaseDirectory}\"");

            
        }


        //
        // drag and drop
        //

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
                        this.CmdShowPageDmodInstaller();
                        this.VmDmodInstaller.TemporaryDmodSource = finfo.FullName;
                    }
                }
            }
        }

        public void InitializeDragAndDrop(Control c) {
            c.AddHandler(DragDrop.DropEvent, this.Drop);
            c.AddHandler(DragDrop.DragOverEvent, this.DragOver);
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
                        this.CmdShowPageDmodInstaller();
                        this.VmDmodInstaller.TemporaryDmodSource = finfo.FullName;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Error initializing arguments");
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }

        //
        // misc event handlers...
        //

        private void VmGeneralSettingsOnSettingsDone(object? sender, EventArgs e) {
            // switch back to the main view...
            this.ActiveUserPage = MainViewPage.MainView;
        }

        private void VmDmodPackerOnInstallerDone(object? sender, DmodPackerDoneEventArgs e) {
            // switch back to the main view...
            this.ActiveUserPage = MainViewPage.MainView;
        }
        

        private void VmDmodInstallerOnInstallerDone(object? sender, DmodInstallerDoneEventArgs e) {
            try {
                if (e.Result == DinkInstallerResult.Success) {
                    // refresh dmods...
                    this._dmodManager.Initialize(this._config.General);
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }

            // switch back to the main view...
            this.ActiveUserPage = MainViewPage.MainView;
        }


        private void VmDinkInstallerOnInstallerDone(object? sender, DinkInstallerDoneEventArgs e) {
            try {
                // try to update exe path in settings...
                if (e.Result == DinkInstallerResult.Success && e.UsedInstaller != null && e.Destination != null) {
                    if (string.IsNullOrWhiteSpace(e.UsedInstaller.GameFileName) == false) {
                        string pathGame = Path.Combine(e.Destination.FullName, e.UsedInstaller.GameFileName);
                        if (File.Exists(pathGame)) {
                            this._config.General.AddGameExePath(pathGame);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(e.UsedInstaller.EditorFileName) == false) {
                        string pathGame = Path.Combine(e.Destination.FullName, e.UsedInstaller.EditorFileName);
                        if (File.Exists(pathGame)) {
                            this._config.General.AddEditorExePath(pathGame);
                        }
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }

            // switch back to the main view...
            this.ActiveUserPage = MainViewPage.MainView;
        }

        //
        // GUI commands...
        //

        public void CmdShowPageSettings(object? parameter = null) {
            if (this.ActiveUserPage == MainViewPage.MainView) {
                this.ActiveUserPage = MainViewPage.Settings;
            }
        }

        [DependsOn(nameof(ActiveUserPage))]
        public bool CanCmdShowPageSettings(object? parameter = null) {
            if (this.ActiveUserPage == MainViewPage.MainView) { return true; }
            return false;
        }

        public void CmdShowPageDinkInstaller(object? parameter = null) {
            if (this.ActiveUserPage == MainViewPage.MainView) {
                this.ActiveUserPage = MainViewPage.DinkInstaller;
                this.VmDinkInstaller.InitializeInstallerList(this._config.General.AutoUpdateInstallerList);
            }
        }

        [DependsOn(nameof(ActiveUserPage))]
        public bool CanCmdShowPageDinkInstaller(object? parameter = null) {
            return this.ActiveUserPage == MainViewPage.MainView;
        }

        public void CmdShowPageDmodInstallerAndBrowse(object? parameter = null) {
            if (this.ActiveUserPage == MainViewPage.MainView) {
                this.ActiveUserPage = MainViewPage.DmodInstaller;
                this.VmDmodInstaller.CmdBrowseDmod();
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        public bool CanCmdShowPageDmodInstallerAndBrowse(object? parameter = null) {
            return this.ActiveUserPage == MainViewPage.MainView;
        }

        public void CmdShowPageDmodInstaller(object? parameter = null) {
            if (this.ActiveUserPage == MainViewPage.MainView) {
                if (parameter is string path &&
                    File.Exists(path)) {
                    this.VmDmodInstaller.TemporaryDmodSource = path;
                }
                this.ActiveUserPage = MainViewPage.DmodInstaller;
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        public bool CanCmdShowPageDmodInstaller(object? parameter = null) {
            return this.ActiveUserPage == MainViewPage.MainView;
        }
        
        public void CmdShowPageDmodPackerAndBrowse(object? parameter = null) {
            if (this.ActiveUserPage == MainViewPage.MainView) {
                this.ActiveUserPage = MainViewPage.DmodPacker;
                this.VmDmodPacker.CmdBrowseDmodSource();
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        public bool CanCmdShowPageDmodPackerAndBrowse(object? parameter = null) {
            return this.ActiveUserPage == MainViewPage.MainView;
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

            targetPath += Path.DirectorySeparatorChar + ".";
            
            // open directory...
            ProcessStartInfo pinfo = new ProcessStartInfo(targetPath) {
                UseShellExecute = true,
                Verb = "open",
            };
            Process.Start(pinfo);
        }
        public bool CanCmdOpenLocation(object? parameter) {
            if (parameter is not string path) return false;
            if (File.Exists(path) || Directory.Exists(path)) return true;
            return false;
        }


        public void CmdShowPageDmodPacker(object? parameter = null) {
            if (parameter is not string path) return;
            if (Directory.Exists(path) == false) return;
            if (this.ActiveUserPage != MainViewPage.MainView) return;
            
            if (new DmodFileDefinition(path).IsCorrectlyDefined) {
                this.VmDmodPacker.TemporaryDmodSourceDirectory = path;
                this.ActiveUserPage = MainViewPage.DmodPacker;
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        public bool CanCmdShowPageDmodPacker(object? parameter = null) {
            if (parameter is not string path) return false;
            if (Directory.Exists(path)) return this.ActiveUserPage == MainViewPage.MainView;
            return false;
        }

        public void CmdShowLogWindow(object? parameter = null) {
            App.Instance?.ShowLogWindow();
        }

    }
}
