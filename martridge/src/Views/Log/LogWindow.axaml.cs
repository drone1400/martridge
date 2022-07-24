using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Martridge.ViewModels;
using System;

namespace Martridge.Views.Log {
    public partial class LogWindow : Window {

        //public LogConsoleViewModel ConsoleVm {
        //    get => this._consoleVm;
        //    set {
        //        this._consoleVm = value;
        //        this.DataContext = this._consoleVm;
        //    }
        //}
        private LogConsoleViewModel _consoleVm = new LogConsoleViewModel();

        public LogWindow() {
            this.DataContext = this._consoleVm;

            this.InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif

            StyleManager.Instance.AddWindow(this);
        }

        public void OnWindowClosed(object? sender, EventArgs e) {
            this._consoleVm?.CloseTraceListener();
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
