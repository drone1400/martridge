using Martridge.Models.Configuration.LaunchExtension.FileData;
namespace Martridge.Models.Configuration.General.FileData {
    public class ConfigFileDataGeneralV2 {
        public string ConfigDataVersion {
            get => "V2";
        }
        
        public ConfigDataGeneralV2? General { get; set; }
        public ConfigDataLaunch? Launch { get; set; }
        public ConfigDataWine? WineGlobal { get; set; }

        public ConfigFileDataGeneralV2() {
            
        }
    }
}
