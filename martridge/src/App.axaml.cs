using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using Citrus.Avalonia;
using Martridge.Models;
using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels;
using Martridge.ViewModels.DinkyAlerts;
using Martridge.Views;
using Martridge.Views.Log;

namespace Martridge {
    
    public partial class App : Application {
        public static App? Instance => Application.Current as App;

        public Window? MainWindow => this._mainWindow;

        public IStorageProvider? StorageProvider => this.MainWindow?.StorageProvider;

        public event EventHandler? OnThemePaletteChange;

        public override void Initialize()
        {
            this.InitializeConfiguration();
            this.InitializeTheme();
        }


        public override void OnFrameworkInitializationCompleted() {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                MyTrace.Global.Listeners.Add(this._console);
                
                if (this._config.Remember.LogWindowShowOnStartup) {
                    this.ShowLogWindow();
                }
                
                
                // add text logger listener to trace...
                MyTrace.Global.Listeners.Add(this._logger);
                    
                MyTrace.Global.WriteMessage($"App Path = \"{LocationHelper.GetPathMartridge()}\"");

                this.PurgeOldLogs();
                
                this.InitializeMainWindow(desktop.Args);
                desktop.MainWindow = this._mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void QuitApplication() {
            Dispatcher.UIThread.Invoke(() => {
                this._mainWindow?.Close();
            });
        }
        
        #region LOGGING stuff
        
        private readonly MyTraceListenerLogger _logger = new MyTraceListenerLogger("martridge");
        private readonly MyTraceListenerConsole _console = new MyTraceListenerConsole("martridgeConsoleLogger");

        private void PurgeOldLogs() {
            try {
                if (this._config.General.MaxLogsToKeep <= 0)
                    return;

                DirectoryInfo dirInfo = new DirectoryInfo(LocationHelper.GetPathLogs());
                FileInfo[] files = dirInfo.GetFiles("martridge*.log");
                
                int delCount = files.Length - this._config.General.MaxLogsToKeep;
                if (delCount <= 0)
                    return;
                
                var orderedFiles = files.OrderBy(f => f.CreationTime);
                foreach (var file in orderedFiles) {
                    file.Delete();
                    delCount--;
                    if (delCount == 0)
                        return;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        
        #endregion
        
        #region CONFIG stuff
        
        private string _defaultConfigFile = "";
        private readonly Config _config = new Config();

        
        private void InitializeConfiguration()
        {
            this._defaultConfigFile = Path.Combine(LocationHelper.GetPathConfig(), "config.json");
            this._config.LoadFromFile(this._defaultConfigFile);
            
            #if PLATF_LINUX
            this.AddDefaultLinuxFreeDinkLocations();
            #endif
            
            this._config.General.Updated += this.GeneralOnUpdated;
            this._config.Launch.Updated += this.LaunchOnUpdated;
        }
        
        private void LaunchOnUpdated(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationChanged"]);
            this._config.SaveToFile(this._defaultConfigFile);
        }

        private void GeneralOnUpdated(object? sender, ConfigUpdateEventArgs e) {
            MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationChanged"]);
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
                this._config.General.TryAddGameExePath(defaultLinuxFreedinkExe);
            }

            if (Directory.Exists(defaultLinuxDinkGameData) &&
                this._config.General.AdditionalDmodLocations.Contains(defaultLinuxDinkGameData) == false) {
                this._config.General.TryAddAdditionalDmodPath(defaultLinuxDinkGameData);
            }

            if (defaultLinuxHome != null &&
                Directory.Exists(defaultLinuxDmods) &&
                this._config.General.AdditionalDmodLocations.Contains(defaultLinuxDmods) == false) {
                this._config.General.TryAddAdditionalDmodPath(defaultLinuxDmods);
            }
        }
        
        #endregion
        
        #region THEME stuff
        
        private readonly Styles _styles = new Styles();
        private CitrusTheme? _citrusTheme = null;
        private StyleInclude? _stylesDataGridCitrus = null;
        private StyleInclude? _customStyles = null;
        private IList<ThemeVariant> _themeVariants = new List<ThemeVariant>();

        private Dictionary<string, FileInfo> _customThemeVariantDefinitions = new Dictionary<string, FileInfo>();
        private Dictionary<string, Uri> _defaultCitrusThemeVariants = new Dictionary<string, Uri>() {
            [CitrusDefaultPalettes.Citrus.ToString()] = new Uri("avares://Citrus.Avalonia/Palette/CitrusPalette.xaml"),
            [CitrusDefaultPalettes.Candy.ToString()] = new Uri("avares://Citrus.Avalonia/Palette/CandyPalette.xaml"),
            [CitrusDefaultPalettes.Magma.ToString()] = new Uri("avares://Citrus.Avalonia/Palette/MagmaPalette.xaml"),
            [CitrusDefaultPalettes.Rust.ToString()] = new Uri("avares://Citrus.Avalonia/Palette/RustPalette.xaml"),
            [CitrusDefaultPalettes.Sea.ToString()] = new Uri("avares://Citrus.Avalonia/Palette/SeaPalette.xaml"),
        };

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
            
            // initialize custom themes
            try {
                string path = LocationHelper.GetPathCustomThemes();
                if (Directory.Exists(path)) {
                    DirectoryInfo di = new DirectoryInfo(path);
                    FileInfo[] files = di.GetFiles();
                    foreach (FileInfo file in files) {
                        try {
                            CitrusThemeVariantData? paletteData = TryLoadThemeVariantFromFile(file);
                            if (paletteData == null) continue;
                            this._citrusTheme.RegisterThemeVariant(paletteData);
                            string key = paletteData.Variant.Key.ToString() ?? string.Empty;
                            if (string.IsNullOrWhiteSpace(key))
                                continue;
                            this._customThemeVariantDefinitions.Add(key, file);
                        } catch (Exception ex) {
                            MyTrace.Global.WriteMessage(ex.ToString(), MyTraceLevel.Warning);
                        }
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(ex.ToString(), MyTraceLevel.Critical);
            }

            this._themeVariants = this._citrusTheme.GetRegisteredThemeVariants();

            if (string.IsNullOrWhiteSpace(this._config.General.DarkThemeOverride) == false) {
                this.OverrideCitrusDarkTheme(this._config.General.DarkThemeOverride);
            }
            
            if (string.IsNullOrWhiteSpace(this._config.General.LightThemeOverride) == false) {
                this.OverrideCitrusLightTheme(this._config.General.LightThemeOverride);
            }
            
            // try to set loaded theme...
            string themeName = this._config.General.ThemeName;
            this.SetCitrusThemePalette(themeName);
        }

        private static CitrusThemeVariantData? TryLoadThemeVariantFromFile(FileInfo file) {
            try {
                string ext = file.Extension.ToLowerInvariant();
                if (ext != ".xaml" && ext != ".axaml")
                    return null;
                string name = file.Name.Substring(0, file.Name.Length - ext.Length);
                using FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                object obj = AvaloniaRuntimeXamlLoader.Load(fileStream);
                if (obj is not ResourceDictionary resDic)
                    return null;
                CitrusThemeVariantData paletteData = new CitrusThemeVariantData(name, resDic);
                return paletteData;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(ex.ToString(), MyTraceLevel.Warning);
                return null;
            }
        }

        private CitrusThemeVariantData? TryGetThemeVariantDataFromKey(string themeKey) {
            try {
                if (this._customThemeVariantDefinitions.TryGetValue(themeKey, out FileInfo? fileInfo)) {
                    return TryLoadThemeVariantFromFile(fileInfo);
                }
                if (this._defaultCitrusThemeVariants.TryGetValue(themeKey, out Uri? uri)) {
                    return new CitrusThemeVariantData(themeKey, uri);
                }
                return null;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(ex.ToString(), MyTraceLevel.Warning);
                return null;
            }
        }

        public void SetCitrusThemePalette(string themeKey) {
            if (themeKey == ThemeVariant.Default.Key.ToString()) {
                this.SetCitrusThemePalette(ThemeVariant.Default);
                return;
            }
            if (themeKey == ThemeVariant.Light.Key.ToString()) {
                this.SetCitrusThemePalette(ThemeVariant.Light);
                return;
            }
            if (themeKey == ThemeVariant.Dark.Key.ToString()) {
                this.SetCitrusThemePalette(ThemeVariant.Dark);
                return;
            }
            
            foreach (ThemeVariant themeVariant in this._themeVariants) {
                if (themeVariant.Key.ToString() == themeKey) {
                    this.SetCitrusThemePalette(themeVariant);
                    return;
                }
            }
        }
        
        private void SetCitrusThemePalette(ThemeVariant themeVariant) {
            this.RequestedThemeVariant = themeVariant;
            
            // update in configuration...
            this._config.General.UpdateProperties(new Dictionary<string, object?>() {
                [nameof(ConfigGeneral.ThemeName)] = themeVariant.Key.ToString(),
            });
            
            // save config to file after changes!
            this._config.SaveToFile(this._defaultConfigFile);
            
            try {
                this.OnThemePaletteChange?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        public void OverrideCitrusLightTheme(string themeKey) {
            CitrusThemeVariantData? data = this.TryGetThemeVariantDataFromKey(themeKey);
            if (data != null && this._citrusTheme != null) {
                // update in configuration...
                this._config.General.UpdateProperties(new Dictionary<string, object?>() {
                    [nameof(ConfigGeneral.LightThemeOverride)] = themeKey,
                });
                // set desired light theme
                this._citrusTheme.DesiredLightThemeVariant = data.VariantProvider;
                
                // save config to file after changes!
                this._config.SaveToFile(this._defaultConfigFile);
            }
        }
        
        public void OverrideCitrusDarkTheme(string themeKey) {
            CitrusThemeVariantData? data = this.TryGetThemeVariantDataFromKey(themeKey);
            if (data != null && this._citrusTheme != null) {
                // update in configuration...
                this._config.General.UpdateProperties(new Dictionary<string, object?>() {
                    [nameof(ConfigGeneral.DarkThemeOverride)] = themeKey,
                });
                // set desired dark theme
                this._citrusTheme.DesiredDarkThemeVariant = data.VariantProvider;
                
                // save config to file after changes!
                this._config.SaveToFile(this._defaultConfigFile);
            }
        }

        public string GetCitrusPalette() {
            return this.RequestedThemeVariant?.ToString() ?? "Default";
        }

        public IList<string> GetThemeNames() {
            List<string> customThemes = new List<string>();
            foreach (var kvp in this._themeVariants) {
                string? key = kvp.Key.ToString();
                if (key == null)
                    continue;
                customThemes.Add(key);
            }
            return customThemes;
        }

        public bool TryGetThemeResource(string key, out object? obj) {
            obj = null;
            return this._citrusTheme?.TryGetResource(key, this.RequestedThemeVariant, out obj) ?? false;
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
                
                this._mainWindowViewModel.InitializeDragAndDrop(this._mainWindow);
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
            
            this._logger.Close();
        }

        private void MainWindow_Closed(object? sender, EventArgs e) {
            this._logger.Close();
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
        
        #region ALERT stuff

        public void SetActiveAlertViewModel(DinkyAlertViewModel vm) {
            if (this._mainWindowViewModel == null) 
                return;
            
            this._mainWindowViewModel.AlertViewModel = vm;
        }

        public void ClearActiveAlertViewModel() {
            if (this._mainWindowViewModel == null) 
                return;
            this._mainWindowViewModel.AlertViewModel = null;
        }
        
        #endregion
    }
}
