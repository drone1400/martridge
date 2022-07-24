using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Installer {
    public partial class InstallerViewPage1 : UserControl {
        public InstallerViewPage1() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
