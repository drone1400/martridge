using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.DinkyAlerts {
    public partial class DinkyAlertWindow : Window {
        public DinkyAlertWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

