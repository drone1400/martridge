using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
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
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void InputElement_OnKeyDown(object? sender, KeyEventArgs e) {
            if (this.DataContext is MainWindowViewModel mwvm) {
                if (mwvm.ProcessKeyDown(e.Key, e.KeyModifiers)) {
                    e.Handled = true;
                }
            }

            if (e.Key == Key.F10) {
                this.Width = 1280;
                this.Height = 800;
            }
        }
    }
}
