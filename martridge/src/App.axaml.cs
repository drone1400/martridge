using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Citrus.Avalonia;

namespace Martridge {
    
    public enum ApplicationTheme {
        // default theme palettes
        Citrus, Sea, Rust, Candy, Magma,
    }
    
    public partial class App : Application {

        public event EventHandler? OnThemePaletteChange;

        private readonly Styles _styles = new Styles();
        private CitrusTheme? _citrusTheme = null;
        private StyleInclude? _stylesDataGridCitrus = null;
        private StyleInclude? _customStyles = null;

        public override void Initialize() {
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

        public override void OnFrameworkInitializationCompleted() {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                WindowManager.Instance.InitializeMainWindow(desktop.Args);

                desktop.MainWindow = WindowManager.Instance.MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
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
            SetCitrusThemePalette(newPalette);
        }

        public string GetCitrusPalette() {
            if (this._citrusTheme == null) return "";
            return this._citrusTheme.ColorPalette;
        }
    }
}
