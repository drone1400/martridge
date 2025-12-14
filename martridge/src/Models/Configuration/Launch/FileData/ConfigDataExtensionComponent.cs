namespace Martridge.Models.Configuration.LaunchExtension.FileData {
    public class ConfigDataExtensionComponent {
        /// <summary>
        /// The path of the target exe "file"
        /// </summary>
        public string? TargetExePath { get; set; }

        public ConfigDataWine? WineData { get; set; }

        public ConfigDataExtensionSteamInfo? SteamData { get; set; }
    }
}
