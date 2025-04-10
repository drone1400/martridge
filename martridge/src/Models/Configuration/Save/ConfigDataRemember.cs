using System.Collections.Generic;
namespace Martridge.Models.Configuration.Save {
    public class ConfigDataRemember {
        /// <summary>
        /// Last used DMOD source path for Install DMOD
        /// </summary>
        public string InstallDmodSourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Last used DMOD base destination location for Install DMOD
        /// </summary>
        public string InstallDmodDestinationBaseDirectory { get; set; } = string.Empty;
        
        /// <summary>
        /// Last used DMOD source path for Pack DMOD
        /// </summary>
        public string PackDmodSourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Last used DMOD destination path for Pack DMOD
        /// </summary>
        public string PackDmodDestinationPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Last selected DMOD in the DMOD browser
        /// </summary>
        public string DmodBrowserSelectedDmodPath { get; set; } = string.Empty;
        
        public Dictionary<string, object?> GetValues() {
            return new Dictionary<string, object?>() {
                [nameof(ConfigRemember.InstallDmodSourcePath)] = this.InstallDmodSourcePath,
                [nameof(ConfigRemember.InstallDmodDestinationBaseDirectory)] = this.InstallDmodDestinationBaseDirectory,
                [nameof(ConfigRemember.PackDmodSourcePath)] = this.PackDmodSourcePath,
                [nameof(ConfigRemember.PackDmodDestinationPath)] = this.PackDmodDestinationPath,
                [nameof(ConfigRemember.DmodBrowserSelectedDmodPath)] = this.DmodBrowserSelectedDmodPath,
            };
        }
    }
}
