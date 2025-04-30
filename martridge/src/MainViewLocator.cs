using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Martridge.ViewModels;
using Martridge.ViewModels.About;
using Martridge.ViewModels.Configuration;
using Martridge.ViewModels.Dmod;
using Martridge.Views.About;
using Martridge.Views.Configuration;
using Martridge.Views.Dmod;

#if ENABLE_FEATURE_ONLINE
using Martridge.ViewModels.OnlineDmod;
using Martridge.Views.OnlineDmod;
#endif

#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
using Martridge.ViewModels.DinkInstaller;
using Martridge.Views.DinkInstaller;
#endif

namespace Martridge
{
    public class MainViewLocator : IDataTemplate
    {

        public Control? Build(object? param)
        {
            switch (param?.GetType().Name ?? "")
            {
#if ENABLE_FEATURE_ONLINE
                case nameof(OnlineDmodBrowserViewModel): return new OnlineDmodBrowserView();
                case nameof(DualDmodBrowserViewModel): return new DualDmodBrowserView();
#endif
#if ENABLE_FEATURE_DINK_INSTALLER && ENABLE_FEATURE_ONLINE
                case nameof(DinkInstallerViewModel): return new DinkInstallerView();
#endif
                case nameof(DmodBrowserViewModel): return new DmodBrowserView();
                case nameof(DmodInstallerViewModel): return new DmodInstallerView();
                case nameof(DmodPackerViewModel): return new DmodPackerView();
                case nameof(AboutViewModel): return new AboutView();
                case nameof(SettingsGeneralViewModel): return new SettingsGeneralView();
                case nameof(SettingsThemeViewModel): return new SettingsThemeView();
                case nameof(NoDinkyViewModel): return new NoDinkyView();
                case nameof(NoDinkyLinuxViewModel): return new NoDinkyLinuxView();
                
                default: return null;
            }
            
            // string? name = param?.GetType().FullName?.Replace("ViewModel", "View");
            // Type? type = name != null ? Type.GetType(name) : null;
            //
            // if (type != null) return Activator.CreateInstance(type) as Control;
            // return null;
        }
        public bool Match(object? data)
        {
            return data is ViewModelAppPage;
        }
    }
}
