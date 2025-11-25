using System.Collections.Generic;
namespace Martridge.Models.Configuration.LaunchExtension.FileData {
    public class ConfigFileDataExtension {
        public string ConfigDataVersion {
            get => "V25.0";
        }
        public List<ConfigDataExtensionComponent>? ExtensionDefinitions { get; set; }
    }
}
