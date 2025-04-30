using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Styling;
using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public class SettingsThemeViewModel : ViewModelAppPageWithCfg {

        public string CurrentTheme {
            get => this._currentTheme;
            set => this.RaiseAndSetIfChanged(ref this._currentTheme, value);
        }
        private string _currentTheme = string.Empty;
        
        public IList<ThemeVariant> AvailableThemes {
            get => this._availableThemes; 
            set => this.RaiseAndSetIfChanged(ref this._availableThemes, value);
        }
        private IList<ThemeVariant> _availableThemes = new List<ThemeVariant>();

        public IList<DmodDefinitionPlaceholderViewModel> SampleDmods =>
            new List<DmodDefinitionPlaceholderViewModel>() {
                new DmodDefinitionPlaceholderViewModel("DMOD 1"),
                new DmodDefinitionPlaceholderViewModel("DMOD 2"),
                new DmodDefinitionPlaceholderViewModel("DMOD 3"),
                new DmodDefinitionPlaceholderViewModel("DMOD 4"),
                new DmodDefinitionPlaceholderViewModel("DMOD 5"),
            };

        public ThemeVariant? SelectedPreviewTheme {
            get => this._selectedPreviewTheme;
            set => this.RaiseAndSetIfChanged(ref this._selectedPreviewTheme, value);
        }
        private ThemeVariant? _selectedPreviewTheme;

        public SettingsThemeViewModel() {
            this.InitializeThemes();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                if (Application.Current is not App app) return;

                app.OnThemePaletteChange -= this.AppOnThemePaletteChanged;
            }
        }

        private void InitializeThemes() {
            if (Application.Current is not App app) return;

            app.OnThemePaletteChange += this.AppOnThemePaletteChanged;

            var themes = app.GetThemeNames();
            
            List<ThemeVariant> availableThemes = new List<ThemeVariant>();
            foreach (var theme in themes) {
                availableThemes.Add(new ThemeVariant(theme, null));
            }
            
            this.AvailableThemes = availableThemes;
            
            string themeKey = app.GetCitrusPalette();
            this.CurrentTheme = themeKey;
            this.SelectThemeByKey(themeKey);

            if (this.SelectedPreviewTheme == null) {
                this.SelectedPreviewTheme = this.AvailableThemes[0];
            }
        }

        private void AppOnThemePaletteChanged(object? sender, EventArgs e) {
            if (sender is not App app) return;
            string themeKey = app.GetCitrusPalette();
            this.CurrentTheme = themeKey;
            this.SelectThemeByKey(themeKey);
        }

        private void SelectThemeByKey(string themeKey) {
            if (themeKey == "Default" || themeKey == "Light" || themeKey == "Dark") {
                return;
            }

            foreach (ThemeVariant theme in this.AvailableThemes) {
                if (theme.Key.ToString() == themeKey) {
                    this.SelectedPreviewTheme = theme;
                    return;
                }
            }
        }

        public void CmdUseSelectedThemeAsLight(object? parameter) {
            if (Application.Current is not App app) return;
            app.OverrideCitrusLightTheme(this.SelectedPreviewTheme?.Key.ToString() ?? string.Empty);
        }
        public void CmdUseSelectedThemeAsDark(object? parameter) {
            if (Application.Current is not App app) return;
            app.OverrideCitrusDarkTheme(this.SelectedPreviewTheme?.Key.ToString() ?? string.Empty);
        }
        public void CmdUseSelectedTheme(object? parameter) {
            if (Application.Current is not App app) return;
            app.SetCitrusThemePalette(this.SelectedPreviewTheme?.Key.ToString() ?? string.Empty);
        }
        public void CmdUseDefaultSystemTheme(object? parameter) {
            if (Application.Current is not App app) return;
            app.SetCitrusThemePalette("Default");
        }
        public void CmdUseDefaultLightTheme(object? parameter) {
            if (Application.Current is not App app) return;
            app.SetCitrusThemePalette("Light");
        }
        public void CmdUseDefaultDarkTheme(object? parameter) {
            if (Application.Current is not App app) return;
            app.SetCitrusThemePalette("Dark");
        }
    }
}
