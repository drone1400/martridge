namespace Martridge.Models.Configuration.Save {
    public class ConfigDataExtensionSteamInfo {
        /// <summary>
        /// The Known 32 bit Steam ID for the target app exe
        /// </summary>
        public uint? SteamId32 { get; set; }

        /// <summary>
        /// If true, Martridge will try to launch the app through steam, as a steam app instead of a normal executable
        /// </summary>
        public bool PreferLaunchingAsSteamApp { get; set; }
    }
}
