using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Martridge.Views.Configuration {
    public partial class SettingsThemeView : UserControl {
        public SettingsThemeView() {
            InitializeComponent();
        }
        
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

