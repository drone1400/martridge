using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Installer {
    public partial class DinkInstallerView : UserControl {
        public DinkInstallerView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
