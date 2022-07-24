using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Dmod {
    public partial class OnlineUserHeaderView : UserControl {
        public OnlineUserHeaderView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

