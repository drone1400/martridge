using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using System;
using System.Collections.Generic;

namespace Martridge {
    public class StyleManager {

        public static StyleManager Instance { get; } = new StyleManager();

        public enum Theme { Default, Citrus, Sea, Rust, Candy, Magma }

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

        public Theme CurrentTheme { get; private set; } = Theme.Citrus;

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

        public void UseTheme(Theme theme) {
            this.CurrentTheme = theme;

            foreach (Window window in this._windows) {
                this.UseCurrentThemeInWindow(window);
            }
        }

        private void UseCurrentThemeInWindow(Window window) {
            // Here, we change the first style in the main window styles
            // section, and the main window instantly refreshes. Remember
            // to invoke such methods from the UI thread.
            window.Styles[0] = this.GetCurrentStyle();
        }

        private StyleInclude GetCurrentStyle() {
            return this.GetStyle(this.CurrentTheme);
        }

        private Uri GetStyleUri(Theme theme) {
            return theme switch {
                Theme.Default => this._citrusStyleUri,
                Theme.Citrus => this._citrusStyleUri,
                Theme.Sea => this._seaStyleUri,
                Theme.Rust => this._rustStyleUri,
                Theme.Candy => this._candyStyleUri,
                Theme.Magma => this._magmaStyleUri,
                _ => throw new ArgumentOutOfRangeException(nameof(theme))
            };
        }

        private StyleInclude GetStyle(Theme theme) {
            return theme switch {
                Theme.Default => this.CitrusStyle,
                Theme.Citrus => this.CitrusStyle,
                Theme.Sea => this.SeaStyle,
                Theme.Rust => this.RustStyle,
                Theme.Candy => this.CandyStyle,
                Theme.Magma => this.MagmaStyle,
                _ => throw new ArgumentOutOfRangeException(nameof(theme))
            };
        }

        public void UseNextTheme() {
            this.UseTheme(this.CurrentTheme switch {
                Theme.Default => Theme.Sea,
                Theme.Citrus => Theme.Sea,
                Theme.Sea => Theme.Rust,
                Theme.Rust => Theme.Candy,
                Theme.Candy => Theme.Magma,
                Theme.Magma => Theme.Citrus,
                _ => Theme.Default,
            });
        }

        private StyleInclude CreateStyle(Uri source) {
            return new StyleInclude(this._something1) { Source = source, };
        }
    }
}

