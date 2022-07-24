using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Installer {
    public partial class DmodInstallerView : UserControl {
        public DmodInstallerView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
