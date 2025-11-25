using System.Collections.Generic;
namespace Martridge.Models.Configuration.Installer.FileData {
    public class ConfigDataInstallerList {
        public string ConfigDataVersion {
            get => "V3";
        }
        public List<ConfigDataInstaller>? InstallerDefinitions { get; set; }
    }
}
