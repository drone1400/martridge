using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Martridge.ViewModels;

namespace Martridge.Views {
    public partial class MainView : UserControl {
        public MainView() {
            this.DataContextChanged += this.MainView_DataContextChanged;
            this.InitializeComponent();
        }

        private void MainView_DataContextChanged(object? sender, System.EventArgs e) {
            (this.DataContext as MainWindowViewModel)?.InitializeDragAndDrop(this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
