using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Martridge.Views.OnlineDmod {
    public partial class OnlineUserHeaderView : UserControl {
        public OnlineUserHeaderView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

