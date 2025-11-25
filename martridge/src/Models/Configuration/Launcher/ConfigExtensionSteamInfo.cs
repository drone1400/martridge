using Martridge.Models.Configuration.Launcher.FileData;
namespace Martridge.Models.Configuration.Launcher {
    public class ConfigExtensionSteamInfo {
        /// <summary>
        /// The Known 32 bit Steam ID for the target app exe
        /// </summary>
        public uint SteamId32 => this._steamId32;
        private uint _steamId32 = 0;
        
        /// <summary>
        /// If true, Martridge will try to launch the app through steam, as a steam app instead of a normal executable
        /// </summary>
        public bool PreferLaunchingAsSteamApp => this._preferLaunchingAsSteamApp;
        private bool _preferLaunchingAsSteamApp = false;

        public ConfigExtensionSteamInfo() { }
        
        public ConfigExtensionSteamInfo(uint steamId32, bool preferLaunchingAsSteamApp) {
            this._steamId32 = steamId32;
            this._preferLaunchingAsSteamApp = preferLaunchingAsSteamApp;
        }

        public ConfigExtensionSteamInfo(ConfigDataExtensionSteamInfo data) {
            this._steamId32 = data.SteamId32 ?? 0;
            this._preferLaunchingAsSteamApp = data.PreferLaunchingAsSteamApp;
        }
        
        public void SetFromData(ConfigDataExtensionSteamInfo data) {
            this._steamId32 = data.SteamId32 ?? 0;
            this._preferLaunchingAsSteamApp = data.PreferLaunchingAsSteamApp;
        }

        public ConfigDataExtensionSteamInfo GetData() {
            return new ConfigDataExtensionSteamInfo() {
                SteamId32 = this._steamId32,
                PreferLaunchingAsSteamApp = this._preferLaunchingAsSteamApp
            };
        }
    }
}
