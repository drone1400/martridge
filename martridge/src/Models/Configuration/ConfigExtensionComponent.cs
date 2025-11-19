using Martridge.Models.Configuration.Save;
namespace Martridge.Models.Configuration {
    public class ConfigExtensionComponent {
        
        /// <summary>
        /// The path of the target exe "file"
        /// </summary>
        public string TargetExePath => this._targetExePath;
        private string _targetExePath = string.Empty;

        public ConfigExtensionLinuxWine? WineData {
            get => this._wineData;
            set => this._wineData = value;
        }
        private ConfigExtensionLinuxWine? _wineData = null;

        public ConfigExtensionSteamInfo? SteamData {
            get => this._steamData;
            set => this._steamData = value;
        }
        private ConfigExtensionSteamInfo? _steamData = null;

        public ConfigExtensionComponent() { }
        public ConfigExtensionComponent(string targetExePath) {
            this._targetExePath = targetExePath;
        }
        public ConfigExtensionComponent(ConfigDataExtensionComponent data) {
            this._targetExePath =  data.TargetExePath ?? string.Empty;
            this._wineData = data.WineData != null ? new ConfigExtensionLinuxWine(data.WineData) : null;
            this._steamData = data.SteamData != null ? new ConfigExtensionSteamInfo(data.SteamData) : null;
        }

        public void SetFromData(ConfigDataExtensionComponent data) {
            this._targetExePath =  data.TargetExePath ?? string.Empty;
            this._wineData = data.WineData != null ? new ConfigExtensionLinuxWine(data.WineData) : null;
            this._steamData = data.SteamData != null ? new ConfigExtensionSteamInfo(data.SteamData) : null;
        }

        public ConfigDataExtensionComponent GetData() {
            return new ConfigDataExtensionComponent() {
                TargetExePath = this._targetExePath,
                WineData = this._wineData?.GetData(),
                SteamData = this._steamData?.GetData()
            };
        }
    }
}
