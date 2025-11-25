using Martridge.Models.Configuration.AppState;
using Martridge.Models.Configuration.AppState.FileData;
using Martridge.Models.Configuration.General;
using Martridge.Models.Configuration.Launcher;
using Martridge.Models.Configuration.Launcher.FileData;
using Martridge.Models.Configuration.Save;

namespace Martridge.Models.Configuration {
    public class Config {
        public ConfigGeneral General { get; } = new ConfigGeneral();
        public ConfigAppState AppState { get; } = new ConfigAppState();
        public ConfigLaunch Launch { get; } = new ConfigLaunch();
        public ConfigExtension LaunchExtension { get; } = new ConfigExtension();
        
        public void SaveConfig(string pathConfig) {
            ConfigData data = new ConfigData() {
                General = this.General.GetData(),
                Launch = this.Launch.GetData(),
            };

            data.SaveToFile(pathConfig);
        }

        public void SaveAppState(string pathState) {
            ConfigDataAppState state = new ConfigDataAppState() {
                Remember = this.AppState.GetData(),
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
                this.AppState.UpdateProperties(state.Remember.GetValues());
            }
        }
        
        
        public void SaveConfigExtension(string pathExtension) {
            ConfigDataExtension extension = this.LaunchExtension.GetData();
            if (extension.ExtensionDefinitions?.Count > 0) {
                extension.SaveToFile(pathExtension);
            }
        }

        public void LoadConfigExtension(string pathExtension) {
            ConfigDataExtension? data  = ConfigDataExtension.LoadFromFile(pathExtension);
            this.LaunchExtension.SetFromData(data);
        }
    }
}
