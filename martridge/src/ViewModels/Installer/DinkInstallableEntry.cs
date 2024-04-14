using Martridge.Models.Configuration;
namespace Martridge.ViewModels.Installer {
    public class DinkInstallableEntry : ViewModelBase {
        public ConfigInstaller InstallerData { get; }

        public string DisplayName { get; }
        public DinkInstallableEntry (ConfigInstaller data) {
            this.InstallerData = data;
            this.DisplayName = data.Name;
        }
    }
}
