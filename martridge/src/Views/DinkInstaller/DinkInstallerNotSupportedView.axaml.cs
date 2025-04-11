using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Martridge.Views.DinkInstaller {
    public partial class DinkInstallerNotSupportedView : UserControl {
        public DinkInstallerNotSupportedView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
