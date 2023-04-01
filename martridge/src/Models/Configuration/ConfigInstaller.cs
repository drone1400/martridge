using Martridge.Models.Configuration.Save;
using System.Collections.Generic;

namespace Martridge.Models.Configuration {
    public class ConfigInstaller {

        /// <summary>
        /// This is the name of the folder in which to install
        /// </summary>
        public string DestinationName { get; set; } = "";
        
        /// <summary>
        /// This is the versioned name of the application being installed, ex: 'Dink HD V1.9.9', 'Dink V108', etc
        /// </summary>
        public string Name { get; set; } = "";
        
        /// <summary>
        /// This is the file name of the main application being installed. It is used to determine what executable file to add to the list of Dink launchers.
        /// </summary>
        public string ApplicationFileName { get; set; } = "";

        /// <summary>
        /// This is the specific name of the application being installed, ex: 'Dink HD', 'Dink', 'Feedink', 'Freedinkedit', 'WinDinkedit' etc
        /// </summary>
        public string Category { get; set; } = "";

        public List<ConfigInstallerComponent> InstallerComponents { get; set; } = new List<ConfigInstallerComponent>();

        public ConfigInstaller Clone() {
            ConfigInstaller cfg = new ConfigInstaller {
                ApplicationFileName = this.ApplicationFileName,
                DestinationName = this.DestinationName,
                Name = this.Name,
            };
            for (int i = 0; i < this.InstallerComponents.Count; i++) {
                ConfigInstallerComponent comp = this.InstallerComponents[i];
                cfg.InstallerComponents.Add(comp.Clone());
            }

            return cfg;
        }

        public static ConfigInstaller? FromJsonData(ConfigDataInstaller data) {
            if (data.Name != null &&
                data.Category != null &&
                data.ApplicationFileName != null &&
                data.InstallerComponents != null) {
                ConfigInstaller cfg = new ConfigInstaller() {
                    Name = data.Name,
                    DestinationName = data.DestinationName ?? "",
                    Category = data.Category,
                    ApplicationFileName = data.ApplicationFileName,
                };

                foreach (ConfigDataInstallerComponent compJ in data.InstallerComponents) {
                    ConfigInstallerComponent? comp = ConfigInstallerComponent.FromJsonData(compJ);
                    // if installer definition is incomplete, abort
                    if (comp == null) return null;
                    cfg.InstallerComponents.Add(comp);
                }

                return cfg;
            }

            return null;
        }

        public ConfigDataInstaller ToJsonData() {
            List<ConfigDataInstallerComponent> list = new List<ConfigDataInstallerComponent>();
            foreach (ConfigInstallerComponent comp in this.InstallerComponents) {
                list.Add(comp.ToJsonData());
            }

            return new ConfigDataInstaller() {
                Name = this.Name,
                DestinationName = this.Name,
                Category = this.Category,
                ApplicationFileName = this.ApplicationFileName,
                InstallerComponents = list,
            };
        }
    }
}
