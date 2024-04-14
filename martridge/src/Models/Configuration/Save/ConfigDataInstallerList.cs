using System.Collections.Generic;

namespace Martridge.Models.Configuration.Save {
    public class ConfigDataInstallerList {
        public string ConfigDataVersion {
            get => "V3";
        }
        public List<ConfigDataInstaller>? InstallerDefinitions { get; set; }
    }
}
