using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace Martridge.Views.Configuration {
    public partial class SettingsWineUserControl : UserControl {
        public SettingsWineUserControl() {
            InitializeComponent();
        }
        
        private void ListBoxItemGrid_OnGotFocus(object? sender, GotFocusEventArgs e) {
            if (sender is Grid grid && grid.FindAncestorOfType<ListBox>() is { } listBox) {
                listBox.SelectedItem = grid.DataContext;
            }
        }
    }
}

