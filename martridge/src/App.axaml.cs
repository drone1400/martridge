using System;
using System.Collections.Generic;
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
using Tmds.DBus.Protocol;

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
                if (this._config.Remember.LogWindowShowOnStartup) {
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

        private void RestoreWindowState(Window window, WindowState state, double width, double height, int positionX, int positionY) {
            switch (state) {
                case WindowState.Maximized:
                case WindowState.FullScreen:
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    window.WindowState = state;
                    break;
                case WindowState.Minimized:
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    break;
                case WindowState.Normal: {
                    if (double.IsNaN(width) == false && double.IsFinite(width) && width > 0 &&
                        double.IsNaN(height) == false && double.IsFinite(height) && height > 0) {
                        window.Width = width;
                        window.Height = height;
                    }
                    if (positionX != 0 && positionY != 0) {
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Position = new PixelPoint(positionX, positionY);
                    }
                    else {
                        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }
                    break;
                }
            }
        }
        
        private void InitializeMainWindow(string[]? args = null) {
            if (this._mainWindow == null) {
                this._mainWindowViewModel = new MainWindowViewModel();
                this._mainWindowViewModel.Initialize(this._config);
                this._mainWindowViewModel.InitializeArgs(args);
                this._mainWindow = new MainWindow {
                    DataContext = this._mainWindowViewModel,
                };

                this.RestoreWindowState(
                    this._mainWindow,
                    this._config.Remember.MainWindowState,
                    this._config.Remember.MainWindowWidth,
                    this._config.Remember.MainWindowHeight,
                    this._config.Remember.MainWindowPositionX,
                    this._config.Remember.MainWindowPositionY);
                
                this._mainWindow.Closed += this.MainWindow_Closed;
                this._mainWindow.Closing += this.MainWindow_Closing;
                this._mainWindow.Show();
            } else {
                this._mainWindow.Activate();
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
            if (this._logWindow is Window logWindow) {
                Dictionary<string, object?> values = new Dictionary<string, object?>() {
                    [nameof(ConfigRemember.LogWindowState)] = logWindow.WindowState,
                    [nameof(ConfigRemember.LogWindowWidth)] = logWindow.Width,
                    [nameof(ConfigRemember.LogWindowHeight)] = logWindow.Height,
                    [nameof(ConfigRemember.LogWindowPositionX)] = logWindow.Position.X,
                    [nameof(ConfigRemember.LogWindowPositionY)] = logWindow.Position.Y,
                    [nameof(ConfigRemember.LogWindowShowOnStartup)] = true,
                };
                this._config.Remember.UpdateProperties(values);
            }
            
            if (this._mainWindow is Window mainWindow) {
                Dictionary<string, object?> values = new Dictionary<string, object?>() {
                    [nameof(ConfigRemember.MainWindowState)] = mainWindow.WindowState,
                    [nameof(ConfigRemember.MainWindowWidth)] = mainWindow.Width,
                    [nameof(ConfigRemember.MainWindowHeight)] = mainWindow.Height,
                    [nameof(ConfigRemember.MainWindowPositionX)] = mainWindow.Position.X,
                    [nameof(ConfigRemember.MainWindowPositionY)] = mainWindow.Position.Y,
                };
                this._config.Remember.UpdateProperties(values);
            }
            
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
                
                this.RestoreWindowState(
                    this._logWindow,
                    this._config.Remember.LogWindowState,
                    this._config.Remember.LogWindowWidth,
                    this._config.Remember.LogWindowHeight,
                    this._config.Remember.LogWindowPositionX,
                    this._config.Remember.LogWindowPositionY);

                this._logWindow.Closing += this.LogWindowOnClosing;
                this._logWindow.Closed += this.LogWindowOnClosed;
                this._logWindow.Show();
            } else {
                this._logWindow.Activate();
            }
        }
        private void LogWindowOnClosed(object? sender, EventArgs e) {
            this._logWindow = null;
        }
        private void LogWindowOnClosing(object? sender, WindowClosingEventArgs e) {
            if (sender is not Window window) 
                return;

            if (this._mainWindow == null) 
                return;
            
            Dictionary<string, object?> values = new Dictionary<string, object?>() {
                [nameof(ConfigRemember.LogWindowState)] = window.WindowState,
                [nameof(ConfigRemember.LogWindowWidth)] = window.Width,
                [nameof(ConfigRemember.LogWindowHeight)] = window.Height,
                [nameof(ConfigRemember.LogWindowPositionX)] = window.Position.X,
                [nameof(ConfigRemember.LogWindowPositionY)] = window.Position.Y,
                [nameof(ConfigRemember.LogWindowShowOnStartup)] = false,
            };
            this._config.Remember.UpdateProperties(values);
        }

        #endregion
    }
}
