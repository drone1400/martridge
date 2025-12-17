using Martridge.Models.Configuration.LaunchExtension.FileData;
namespace Martridge.Models.Configuration.General.FileData {
    public class ConfigFileDataGeneralV1 {
        public string ConfigDataVersion {
            get => "V1";
        }
        
        public ConfigDataGeneralV1? General { get; set; }
        public ConfigDataLaunch? Launch { get; set; }
        public ConfigDataWine? WineGlobal { get; set; }

        public ConfigFileDataGeneralV1() {
            
        }
    }
}
