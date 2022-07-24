using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.About {
    public partial class AboutWindow : Window {
        public AboutWindow() {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            StyleManager.Instance.AddWindow(this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        public void ButtonOkClickHandler(object? sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
