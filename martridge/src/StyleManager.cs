using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using System;
using System.Collections.Generic;

namespace Martridge {
    
    public enum ApplicationTheme { Default, Citrus, Sea, Rust, Candy, Magma }
    public class StyleManager {

        public event EventHandler? ThemeChanged;

        public static StyleManager Instance { get; } = new StyleManager();

        private Uri _styleOverrideUri = new Uri(@"avares://martridge/AppStyles.axaml");

        private Uri _avaloniaDefaultUri = new Uri(@"avares://Avalonia.Themes.Default/DefaultTheme.xaml");

        private Uri _magmaStyleUri = new Uri(@"avares://Citrus.Avalonia/Magma.xaml");
        private Uri _candyStyleUri = new Uri(@"avares://Citrus.Avalonia/Candy.xaml");
        private Uri _rustStyleUri = new Uri(@"avares://Citrus.Avalonia/Rust.xaml");
        private Uri _citrusStyleUri = new Uri(@"avares://Citrus.Avalonia/Citrus.xaml");
        private Uri _seaStyleUri = new Uri(@"avares://Citrus.Avalonia/Sea.xaml");

        private Uri _something1 = new Uri(@"resm:Styles?assembly=martridge");

        //
        
        //@"resm:Styles?assembly=martridge"

        private StyleInclude StyleOverride { get => CreateStyle(this._styleOverrideUri); }


        private StyleInclude AvaloniaDefault { get => CreateStyle(this._avaloniaDefaultUri); } 
        private StyleInclude MagmaStyle { get => CreateStyle(this._magmaStyleUri); }
        private StyleInclude CandyStyle { get => CreateStyle(this._candyStyleUri); }
        private StyleInclude CitrusStyle { get => CreateStyle(this._citrusStyleUri); }
        private StyleInclude RustStyle { get => CreateStyle(this._rustStyleUri); }
        private StyleInclude SeaStyle { get => CreateStyle(this._seaStyleUri); }

        private List<Window> _windows;

        public ApplicationTheme CurrentApplicationTheme { get; private set; } = ApplicationTheme.Citrus;

        public StyleManager() {
            this._windows = new List<Window>();
        }

        public void AddWindow(Window window) {
            this._windows.Add(window);
            window.Closed += this.Window_Closed;

            window.Styles.Clear();
            window.Styles.Add(this.GetCurrentStyle());
            window.Styles.Add(this.StyleOverride);
        }

        private void Window_Closed(object? sender, EventArgs e) {
            if (!(sender is Window window)) { return; }

            window.Closed -= this.Window_Closed;
            this._windows.Remove(window);
        }

        public void UseTheme(ApplicationTheme applicationTheme) {
            if (this.CurrentApplicationTheme != applicationTheme) {
                this.CurrentApplicationTheme = applicationTheme;

                foreach (Window window in this._windows) {
                    this.UseCurrentThemeInWindow(window);
                }

                try {
                    this.ThemeChanged?.Invoke(this, EventArgs.Empty);
                } catch (Exception ex) {
                    // TODO
                }
            } 
        }

        private void UseCurrentThemeInWindow(Window window) {
            // Here, we change the first style in the main window styles
            // section, and the main window instantly refreshes. Remember
            // to invoke such methods from the UI thread.
            window.Styles[0] = this.GetCurrentStyle();
        }

        private StyleInclude GetCurrentStyle() {
            return this.GetStyle(this.CurrentApplicationTheme);
        }

        private Uri GetStyleUri(ApplicationTheme applicationTheme) {
            return applicationTheme switch {
                ApplicationTheme.Default => this._citrusStyleUri,
                ApplicationTheme.Citrus => this._citrusStyleUri,
                ApplicationTheme.Sea => this._seaStyleUri,
                ApplicationTheme.Rust => this._rustStyleUri,
                ApplicationTheme.Candy => this._candyStyleUri,
                ApplicationTheme.Magma => this._magmaStyleUri,
                _ => throw new ArgumentOutOfRangeException(nameof(applicationTheme))
            };
        }

        private StyleInclude GetStyle(ApplicationTheme applicationTheme) {
            return applicationTheme switch {
                ApplicationTheme.Default => this.CitrusStyle,
                ApplicationTheme.Citrus => this.CitrusStyle,
                ApplicationTheme.Sea => this.SeaStyle,
                ApplicationTheme.Rust => this.RustStyle,
                ApplicationTheme.Candy => this.CandyStyle,
                ApplicationTheme.Magma => this.MagmaStyle,
                _ => throw new ArgumentOutOfRangeException(nameof(applicationTheme))
            };
        }

        public void UseNextTheme() {
            this.UseTheme(this.CurrentApplicationTheme switch {
                ApplicationTheme.Default => ApplicationTheme.Sea,
                ApplicationTheme.Citrus => ApplicationTheme.Sea,
                ApplicationTheme.Sea => ApplicationTheme.Rust,
                ApplicationTheme.Rust => ApplicationTheme.Candy,
                ApplicationTheme.Candy => ApplicationTheme.Magma,
                ApplicationTheme.Magma => ApplicationTheme.Citrus,
                _ => ApplicationTheme.Default,
            });
        }

        private StyleInclude CreateStyle(Uri source) {
            return new StyleInclude(this._something1) { Source = source, };
        }
    }
}

