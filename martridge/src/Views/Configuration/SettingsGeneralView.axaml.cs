using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Configuration {
    public partial class SettingsGeneralView : UserControl {
        public SettingsGeneralView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
