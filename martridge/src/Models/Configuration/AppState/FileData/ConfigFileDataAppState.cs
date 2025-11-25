namespace Martridge.Models.Configuration.AppState.FileData {
    public class ConfigFileDataAppState{
        public string ConfigDataVersion {
            get => "V25.0";
        }
        
        public ConfigDataAppState? AppState { get; set; }

        public ConfigFileDataAppState() {
            
        }

        public ConfigFileDataAppState(ConfigDataAppState appState) {
            this.AppState = appState;
        }
    }
}
