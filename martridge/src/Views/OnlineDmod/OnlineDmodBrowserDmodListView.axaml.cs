using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Martridge.Views.OnlineDmod {
    public partial class OnlineDmodBrowserDmodListView : UserControl {
        public OnlineDmodBrowserDmodListView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        
        // NOTE: not quite MVVM-ish, but this seems the easiest way to make sure the DataGrid is scrolled to the currently selected DMOD... 
        private void DataGridDmods_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
            this.ScrollDataGridtoSelection();
        }
        private void ScrollDataGridtoSelection() {
            DataGrid? grid = this.FindControl<DataGrid>("dataGridDmods");
            if (grid == null || grid.SelectedItem == null) return;
            grid.ScrollIntoView(grid.SelectedItem, grid.Columns.First());
        }
    }
}

