using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Martridge.Views.Dmod {
    public partial class DmodPackerView : UserControl {
        public DmodPackerView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
