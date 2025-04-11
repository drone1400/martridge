using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Citrus.Avalonia;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels;
using Martridge.Views;
using Martridge.Views.Log;

namespace Martridge {
    
    public enum ApplicationTheme {
        // default theme palettes
        Citrus, Sea, Rust, Candy, Magma,
    }
    
    public partial class App : Application {
        public static App? Instance => Application.Current as App;

        public Window? MainWindow => this._mainWindow;

        public IStorageProvider? StorageProvider => this.MainWindow?.StorageProvider;

        public event EventHandler? OnThemePaletteChange;

        public override void Initialize()
        {
            this.InitializeTheme();
            this.InitializeConfiguration();
        }


        public override void OnFrameworkInitializationCompleted() {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                if (this._config.General.ShowLogWindowOnStartup) {
                    this.ShowLogWindow();
                }
                
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"App Path = \"{LocationHelper.AppBaseDirectory}\"");
                
                this.InitializeMainWindow(desktop.Args);
                desktop.MainWindow = this._mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        #region CONFIG stuff
        
        private string _defaultConfigFile = "";
        private readonly Config _config = new Config();

        
        private void InitializeConfiguration()
        {
            this._defaultConfigFile = Path.Combine(LocationHelper.AppBaseDirectory, "config", "config.json");
            this._config.LoadFromFile(this._defaultConfigFile);
            
            #if PLATF_LINUX
            this.AddDefaultLinuxFreeDinkLocations();
            #endif
            
            this._config.General.Updated += this.GeneralOnUpdated;
            this._config.Launch.Updated += this.LaunchOnUpdated;
            
            // try to set loaded theme...
            string themeName = this._config.General.ThemeName;
            if (Enum.TryParse(themeName, out ApplicationTheme themeValue)) {
                this.SetCitrusThemePalette(themeName);
            }
        }
        
        private void LaunchOnUpdated(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance["General/ConfigurationChanged"]);
            this._config.SaveToFile(this._defaultConfigFile);
        }

        private void GeneralOnUpdated(object? sender, ConfigUpdateEventArgs e) {
            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance["General/ConfigurationChanged"]);
            this._config.SaveToFile(this._defaultConfigFile);
        }
        
        
        private void AddDefaultLinuxFreeDinkLocations()
        {
            string defaultLinuxFreedinkExe = "/usr/games/freedink";
            string defaultLinuxDinkGameData = "/usr/share/games/dink";
            string? defaultLinuxHome = Environment.GetEnvironmentVariable("HOME");
            string defaultLinuxDmods = Path.Combine(defaultLinuxHome ?? "", "dmods");

            if (File.Exists(defaultLinuxFreedinkExe) && 
                this._config.General.GameExePaths.Contains(defaultLinuxFreedinkExe) == false) {
                this._config.General.AddGameExePath(defaultLinuxFreedinkExe);
            }

            if (Directory.Exists(defaultLinuxDinkGameData) &&
                this._config.General.AdditionalDmodLocations.Contains(defaultLinuxDinkGameData) == false) {
                this._config.General.AddAdditionalDmodPath(defaultLinuxDinkGameData);
            }

            if (defaultLinuxHome != null &&
                Directory.Exists(defaultLinuxDmods) &&
                this._config.General.AdditionalDmodLocations.Contains(defaultLinuxDmods) == false) {
                this._config.General.AddAdditionalDmodPath(defaultLinuxDmods);
            }
        }
        
        #endregion
        
        #region THEME stuff
        
        private readonly Styles _styles = new Styles();
        private CitrusTheme? _citrusTheme = null;
        private StyleInclude? _stylesDataGridCitrus = null;
        private StyleInclude? _customStyles = null;

        private void InitializeTheme()
        {
            this.Styles.Add(this._styles);
            AvaloniaXamlLoader.Load(this);
            
            this._citrusTheme = new CitrusTheme();
            
            Uri uriDataGridCitrus = new Uri("avares://Citrus.Avalonia.DataGrid/CitrusDataGrid.xaml");
            this._stylesDataGridCitrus = new StyleInclude(uriDataGridCitrus) { Source = uriDataGridCitrus };
            Uri uriCustomStyles = new Uri("avares://Martridge/AppStyles.axaml");
            this._customStyles = new StyleInclude(uriCustomStyles) { Source = uriCustomStyles };
            
            this._styles.Add(this._citrusTheme);
            this._styles.Add(this._stylesDataGridCitrus);
            this._styles.Add(this._customStyles);
        }
        
        public void SetCitrusThemePalette(string paletteKey) {
            if (this._citrusTheme == null) return;
            this._citrusTheme.ColorPalette = paletteKey;

            try {
                this.OnThemePaletteChange?.Invoke(this, EventArgs.Empty);
            } catch (Exception) {
                // TODO...
            }
        }

        public void SetCitrusNextPalette() {
            if (this._citrusTheme == null) return;
            string newPalette = this._citrusTheme.ColorPalette switch {
                "Citrus" => "Sea",
                "Sea" => "Rust",
                "Rust" => "Candy",
                "Candy" => "Magma",
                _ => "Citrus",
            };
            this.SetCitrusThemePalette(newPalette);
        }

        public string GetCitrusPalette() {
            if (this._citrusTheme == null) return "";
            return this._citrusTheme.ColorPalette;
        }
        
        #endregion
        
        #region WINDOW stuff
        
        private MainWindow? _mainWindow;
        private MainWindowViewModel? _mainWindowViewModel;
        private LogWindow? _logWindow;
        
        
        private void InitializeMainWindow(string[]? args = null) {
            if (this._mainWindow == null) {
                this._mainWindowViewModel = new MainWindowViewModel();
                this._mainWindowViewModel.Initialize(this._config);
                this._mainWindowViewModel.InitializeArgs(args);
                this._mainWindow = new MainWindow {
                    DataContext = this._mainWindowViewModel,
                };
                
                this._mainWindow.Closed += this.MainWindow_Closed;
                this._mainWindow.Closing += this.MainWindow_Closing;
                this._mainWindow.Show();
            } else {
                this._mainWindow.Activate();
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
            // save config when closing in order to save ConfigRemember
            this._config.SaveToFile(this._defaultConfigFile);
        }

        private void MainWindow_Closed(object? sender, EventArgs e) {
            this._mainWindow = null;
            this._logWindow?.Close();
        }

        public void ShowLogWindow() {
            if (this._logWindow == null) {
                this._logWindow = new LogWindow();
                this._logWindow.Closed += (s,e) => { this._logWindow = null; };
                this._logWindow.Show();
            } else {
                this._logWindow.Activate();
            }
        }
        
        #endregion
    }
}
