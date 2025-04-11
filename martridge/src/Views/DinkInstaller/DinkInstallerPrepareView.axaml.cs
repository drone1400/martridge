using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Martridge.Views.DinkInstaller {
    public partial class DinkInstallerPrepareView : UserControl {
        public DinkInstallerPrepareView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
