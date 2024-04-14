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
using System.IO;
using System.Linq;

namespace Martridge.ViewModels {
    public class MainWindowViewModel : ViewModelBase {

        private readonly AppLogic _logic;

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
    
        public MainWindowViewModel() {
            this._logic = new AppLogic();

            if (this._logic.Config.General.ShowLogWindowOnStartup) {
                WindowManager.Instance.ShowLogWindow();
            }

            this.VmGeneralSettings.Configuration = this._logic.Config.General;
            this.VmGeneralSettings.SettingsDone += this.VmGeneralSettingsOnSettingsDone;
            
            this.VmDinkInstaller.InstallerDone += this.VmDinkInstallerOnInstallerDone;

            this.VmDmodInstaller.InstallerDone += this.VmDmodInstallerOnInstallerDone;
            this.VmDmodInstaller.InitializeConfiguration(this._logic.Config.General);
            
            this.VmDmodPacker.InstallerDone += this.VmDmodPackerOnInstallerDone;
            
            this.VmDmodBrowser.Configuration = this._logic.Config;
            this.VmDmodBrowser.DmodManager = this._logic.DmodManager;
            
            this.VmOnlineDmodBrowser.DmodCrawler = this._logic.DmodCrawler;
            this.VmOnlineDmodBrowser.MainVm = this;

            this.VmAboutWindow.Configuration = this._logic.Config.General;

            MyTrace.Global.WriteMessage(MyTraceCategory.General, $"App Path = \"{LocationHelper.AppBaseDirectory}\"");

            this._logic.DmodCrawler.InitializeDmodLists(false);
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
                        this.VmDmodInstaller.SelectedDmodPacakge = finfo.FullName;
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
                        this.VmDmodInstaller.SelectedDmodPacakge = finfo.FullName;
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
                    this._logic.DmodManager.Initialize(this._logic.Config.General);
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
                            this._logic.Config.General.AddGameExePath(pathGame);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(e.UsedInstaller.EditorFileName) == false) {
                        string pathGame = Path.Combine(e.Destination.FullName, e.UsedInstaller.EditorFileName);
                        if (File.Exists(pathGame)) {
                            this._logic.Config.General.AddEditorExePath(pathGame);
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
                this.VmDinkInstaller.InitializeInstallerList(this._logic.Config.General.AutoUpdateInstallerList);
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
                    this.VmDmodInstaller.SelectedDmodPacakge = path;
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

        public void CmdShowPageDmodPacker(object? parameter = null) {
            if (this.ActiveUserPage == MainViewPage.MainView) {
                if (parameter is string path &&
                    Directory.Exists(path) && 
                    new DmodFileDefinition(path).IsCorrectlyDefined) {
                    this.VmDmodPacker.SelectedDmodSourceDirectory = path;
                }
                this.ActiveUserPage = MainViewPage.DmodPacker;
            }
        }
        [DependsOn(nameof(ActiveUserPage))]
        public bool CanCmdShowPageDmodPacker(object? parameter = null) {
            return this.ActiveUserPage == MainViewPage.MainView;
        }

        public void CmdShowLogWindow(object? parameter = null) {
            WindowManager.Instance.ShowLogWindow();
        }

    }
}
