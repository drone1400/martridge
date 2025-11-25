namespace Martridge.Models.Configuration.Launcher.FileData {
    public class ConfigDataExtensionComponent {
        /// <summary>
        /// The path of the target exe "file"
        /// </summary>
        public string? TargetExePath { get; set; }

        public ConfigDataExtensionLinuxWine? WineData { get; set; }

        public ConfigDataExtensionSteamInfo? SteamData { get; set; }
    }
}
