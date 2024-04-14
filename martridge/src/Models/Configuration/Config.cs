using Martridge.Models.Configuration.Save;

namespace Martridge.Models.Configuration {
    public class Config {
        public ConfigGeneral General { get; } = new ConfigGeneral();
        public ConfigLaunch Launch { get; } = new ConfigLaunch();
        public ConfigAlertResults AlertResults { get; } = new ConfigAlertResults();

        public void SaveToFile(string path) {
            ConfigData data = new ConfigData() {
                General = this.General.GetData(),
                Launch = this.Launch.GetData(),
                AlertResults = this.AlertResults.GetData(),
            };

            data.SaveToFile(path);
        }

        public void LoadFromFile(string path) {
            ConfigData? data = ConfigData.LoadFromFile(path);
            
            if (data?.General != null) {
                this.General.UpdateProperties(data.General.GetValues());
            }
            
            if (data?.Launch != null) {
                this.Launch.UpdateFromData(data.Launch);
            }

            if (data?.AlertResults != null) {
                this.AlertResults.UpdateFromData(data.AlertResults);
            }
        }
    }
}
