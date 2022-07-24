using Martridge.Models.Configuration.Save;

namespace Martridge.Models.Configuration {
    public class Config {
        public ConfigGeneral General { get; } = new ConfigGeneral();
        public ConfigLaunch Launch { get; } = new ConfigLaunch();

        public void SaveToFile(string path) {
            ConfigData data = new ConfigData() {
                General = this.General.GetData(),
                Launch = this.Launch.GetData(),
            };

            data.SaveToFile(path);
        }

        public void LoadFromFile(string path) {
            ConfigData? data = ConfigData.LoadFromFile(path);
            
            if (data?.General != null) {
                this.General.UpdateFromData(data.General);
            }
            
            if (data?.Launch != null) {
                this.Launch.UpdateFromData(data.Launch);
            }
        }
    }
}
