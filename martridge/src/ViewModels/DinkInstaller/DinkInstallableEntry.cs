using Martridge.Models.Configuration;
using Martridge.Models.Configuration.Installer;
namespace Martridge.ViewModels.DinkInstaller {
    public class DinkInstallableEntry : ViewModelBase {
        public ConfigInstaller InstallerData { get; }

        public string DisplayName { get; }
        public DinkInstallableEntry (ConfigInstaller data) {
            this.InstallerData = data;
            this.DisplayName = data.Name;
        }
    }
}
