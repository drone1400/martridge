using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Martridge.ViewModels.About;
using Martridge.ViewModels.Configuration;
using Martridge.ViewModels.Dmod;
using Martridge.ViewModels.Installer;
using Martridge.Views.About;
using Martridge.Views.Configuration;
using Martridge.Views.Dmod;
using Martridge.Views.Installer;

namespace Martridge.ViewModels
{
    public class MainViewLocator : IDataTemplate
    {

        public Control? Build(object? param)
        {
            switch (param?.GetType().Name ?? "")
            {
#if PLATF_WINDOWS
                case nameof(DinkInstallerViewModel): return new DinkInstallerView();
#endif
                case nameof(DualDmodBrowserViewModel): return new DualDmodBrowserView();
                case nameof(DmodBrowserViewModel): return new DmodBrowserView();
                case nameof(OnlineDmodBrowserViewModel): return new OnlineDmodBrowserView();
                case nameof(DmodInstallerViewModel): return new DmodInstallerView();
                case nameof(DmodPackerViewModel): return new DmodPackerView();
                case nameof(AboutViewModel): return new AboutView();
                case nameof(SettingsGeneralViewModel): return new SettingsGeneralView();
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
