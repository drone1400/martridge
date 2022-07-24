using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Martridge {
    public partial class App : Application {

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                WindowManager.Instance.InitializeMainWindow(desktop.Args);

                desktop.MainWindow = WindowManager.Instance.MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
