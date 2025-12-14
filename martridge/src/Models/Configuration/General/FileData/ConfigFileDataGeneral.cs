using Martridge.Models.Configuration.LaunchExtension.FileData;
namespace Martridge.Models.Configuration.General.FileData {
    public class ConfigFileDataGeneral {
        public string ConfigDataVersion {
            get => "V25.0";
        }
        
        public ConfigDataGeneral? General { get; set; }
        public ConfigDataLaunch? Launch { get; set; }
        public ConfigDataWine? WineGlobal { get; set; }

        public ConfigFileDataGeneral() {
            
        }
    }
}
