using Martridge.Views.Installer;
using Martridge.ViewModels;
using Martridge.Views;
using Martridge.Views.Log;
using System;

namespace Martridge {
    public class WindowManager {

        public static WindowManager Instance { get; } = new WindowManager();

        public MainWindow? MainWindow { get => this._mainWindow; }
        private MainWindow? _mainWindow;
        private MainWindowViewModel? _mainWindowViewModel;

        public LogWindow? LogWindow { get => this._logWindow; }
        private LogWindow? _logWindow;
        
        public void InitializeMainWindow(string[]? args = null) {
            if (this._mainWindow == null) {
                this._mainWindowViewModel = new MainWindowViewModel();
                this._mainWindowViewModel.InitializeArgs(args);
                this._mainWindow = new MainWindow {
                    DataContext = this._mainWindowViewModel,
                };


                this._mainWindowViewModel.VmDinkInstaller.AssignParentWindow(this._mainWindow);
                this._mainWindowViewModel.VmDmodInstaller.AssignParentWindow(this._mainWindow);
                this._mainWindowViewModel.VmDmodPacker.AssignParentWindow(this._mainWindow);
                this._mainWindowViewModel.VmDmodBrowser.AssignParentWindow(this._mainWindow);
                this._mainWindowViewModel.VmGeneralSettings.AssignParentWindow(this._mainWindow);
                
                this._mainWindow.Closed += this._MainWindow_Closed;
                this._mainWindow.Closing += this._MainWindow_Closing;
                this._mainWindow.Show();
            } else {
                this._mainWindow.Activate();
            }
        }

        private void _MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void _MainWindow_Closed(object? sender, EventArgs e) {
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
    }
}
