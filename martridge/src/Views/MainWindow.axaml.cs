using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
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
    }
}
