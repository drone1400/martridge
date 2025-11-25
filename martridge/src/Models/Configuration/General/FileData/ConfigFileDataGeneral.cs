namespace Martridge.Models.Configuration.General.FileData {
    public class ConfigFileDataGeneral {
        public string ConfigDataVersion {
            get => "V25.0";
        }
        
        public ConfigDataGeneral? General { get; set; }
        public ConfigDataLaunch? Launch { get; set; }

        public ConfigFileDataGeneral() {
            
        }
        
        public ConfigFileDataGeneral(ConfigDataGeneral general,  ConfigDataLaunch launch) {
            this.General = general;
            this.Launch = launch;
        }
    }
}
