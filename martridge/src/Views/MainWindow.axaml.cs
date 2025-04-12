using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Citrus.Avalonia;
using Martridge.Trace;
using Martridge.ViewModels;
using Martridge.Views.About;

namespace Martridge.Views {
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
        public MainWindow() {
            this.InitializeComponent();
            
#if DEBUG
            this.AttachDevTools();
#endif

            this.InitializeThemeMenu();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        
        public void KeyDownHandler(object? sender, Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Avalonia.Input.Key.F2) {
                if (Application.Current is not App app) return;
                app.SetCitrusNextPalette();
            }
            e.Handled = false;
        }

        public void CloseWindow(object? sender, RoutedEventArgs e) {
            this.Close();
        }

        private void InitializeThemeMenu() {
            try {
                MenuItem? menu = this.FindControl<MenuItem>("menuItemThemes");
                if (menu is null) return;

                IList<string> themeNames = App.Instance?.GetThemeNames() ?? new List<string>();

                foreach (string theme in themeNames) {
                    string header = theme.Replace('_', ' '); // replace underscore with space so it looks nicer!
                    MenuItem menuItem = new MenuItem() {
                        Header = header,
                        CommandParameter = theme,
                    };
                    menuItem.Bind(MenuItem.CommandProperty, new Binding("CmdChangeTheme"));
                    menu.Items.Add(menuItem);
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }
    }
}
