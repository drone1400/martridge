using System.Collections.Generic;
namespace Martridge.Models.Configuration.LaunchExtension.FileData {
    public class ConfigFileDataExtensionV1 {
        public string ConfigDataVersion {
            get => "V1";
        }
        public List<ConfigDataExtensionComponent>? ExtensionDefinitions { get; set; }
    }
}
