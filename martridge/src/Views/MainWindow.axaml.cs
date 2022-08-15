using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Martridge.ViewModels;
using Martridge.Views.About;

namespace Martridge.Views {
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
        public MainWindow() {
            this.InitializeComponent();
            StyleManager.Instance.AddWindow(this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        
        public void KeyDownHandler(object? sender, Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Avalonia.Input.Key.F2) {
                StyleManager.Instance.UseNextTheme();
            }
            e.Handled = false;
        }

        public void CloseWindow(object? sender, RoutedEventArgs e) {
            this.Close();
        }


        public void ShowAboutWindow(object? sender, RoutedEventArgs e) {
            AboutWindow aboutWindow = new AboutWindow() {
                DataContext = (this.DataContext as MainWindowViewModel)?.VmAboutWindow,
            };
            aboutWindow.ShowDialog(this);
        }
    }
}
