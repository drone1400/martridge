using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.DinkyAlerts {
    public partial class DinkyAlertView : UserControl {
        public DinkyAlertView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

