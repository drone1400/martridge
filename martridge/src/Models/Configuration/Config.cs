using Martridge.Models.Configuration.Save;

namespace Martridge.Models.Configuration {
    public class Config {
        public ConfigGeneral General { get; } = new ConfigGeneral();
        public ConfigLaunch Launch { get; } = new ConfigLaunch();

        public ConfigRemember Remember { get; } = new ConfigRemember();

        // NOTE: no longer used...
        //public ConfigAlertResults AlertResults { get; } = new ConfigAlertResults();

        public void SaveToFile(string path) {
            ConfigData data = new ConfigData() {
                General = this.General.GetData(),
                Launch = this.Launch.GetData(),
                Remember = this.Remember.GetData(),
                //AlertResults = this.AlertResults.GetData(),
            };

            data.SaveToFile(path);
        }

        public void LoadFromFile(string path) {
            ConfigData? data = ConfigData.LoadFromFile(path);
            
            if (data?.General != null) {
                this.General.UpdateProperties(data.General.GetValues());
            }
            
            if (data?.Remember != null) {
                this.Remember.UpdateProperties(data.Remember.GetValues());
            }
            
            if (data?.Launch != null) {
                this.Launch.UpdateProperties(data.Launch.GetValues());
            }

            // if (data?.AlertResults != null) {
            //     this.AlertResults.UpdateProperties(data.AlertResults.GetValues());
            // }
        }
    }
}
