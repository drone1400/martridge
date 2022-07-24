using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Dmod {
    public partial class OnlineDmodBrowserView : UserControl {
        public OnlineDmodBrowserView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
