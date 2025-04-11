using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Martridge.Views.DinkInstaller {
    public partial class InstallerProgressView : UserControl {
        public InstallerProgressView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
