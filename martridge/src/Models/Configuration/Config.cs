using Martridge.Models.Configuration.Save;

namespace Martridge.Models.Configuration {
    public class Config {
        public ConfigGeneral General { get; } = new ConfigGeneral();
        public ConfigLaunch Launch { get; } = new ConfigLaunch();

        public ConfigRemember Remember { get; } = new ConfigRemember();
        
        public void SaveConfig(string pathConfig) {
            ConfigData data = new ConfigData() {
                General = this.General.GetData(),
                Launch = this.Launch.GetData(),
            };

            data.SaveToFile(pathConfig);
        }

        public void SaveAppState(string pathState) {
            ConfigDataAppState state = new ConfigDataAppState() {
                Remember = this.Remember.GetData(),
            };
            
            state.SaveToFile(pathState);
        }
        
        public void LoadConfig(string pathConfig) {
            ConfigData? data = ConfigData.LoadFromFile(pathConfig);
            
            if (data?.General != null) {
                this.General.UpdateProperties(data.General.GetValues());
            }

            if (data?.Launch != null) {
                this.Launch.UpdateProperties(data.Launch.GetValues());
            }
        }

        public void LoadAppState(string pathState) {
            ConfigDataAppState? state = ConfigDataAppState.LoadFromFile(pathState);
            
            if (state?.Remember != null) {
                this.Remember.UpdateProperties(state.Remember.GetValues());
            }
        }
    }
}
