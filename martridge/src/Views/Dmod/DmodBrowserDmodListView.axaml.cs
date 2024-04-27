using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Martridge.Views.Dmod {
    public partial class DmodBrowserDmodListView : UserControl {
        public DmodBrowserDmodListView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

