using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Dmod {
    public partial class DmodBrowserView : UserControl {
        public DmodBrowserView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
