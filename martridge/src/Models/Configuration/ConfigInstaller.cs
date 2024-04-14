using Martridge.Models.Configuration.Save;
using System.Collections.Generic;
using SharpCompress;

namespace Martridge.Models.Configuration {
    public class ConfigInstaller {

        /// <summary>
        /// This is the name of the folder in which to install
        /// </summary>
        public string DestinationName { get; private init; } = "";
        
        /// <summary>
        /// This is the versioned name of the application being installed, ex: 'Dink HD V1.9.9', 'Dink V108', etc
        /// </summary>
        public string Name { get; private init; } = "";
        
        /// <summary>
        /// This is the file name of the main application being installed. It is used to determine what executable file to add to the list of Dink games.
        /// </summary>
        public string GameFileName { get; private init;  } = "";
        
        /// <summary>
        /// This is the file name of the editor being installed. It is used to determine what executable file to add to the list of Dink editors.
        /// </summary>
        public string EditorFileName { get; private init; } = "";

        /// <summary>
        /// This is the specific name of the application being installed, ex: 'Dink HD', 'Dink', 'Feedink', 'Freedinkedit', 'WinDinkedit' etc
        /// </summary>
        public string Category { get; private init;  } = "";

        public ReadOnlyCollection<ConfigInstallerComponent> InstallerComponents { get; }
        private readonly List<ConfigInstallerComponent> _installerComponents = new List<ConfigInstallerComponent>();

        public ConfigInstaller() {
            this.InstallerComponents = new ReadOnlyCollection<ConfigInstallerComponent>(this._installerComponents);
        }
        
        public ConfigInstaller Clone() {
            ConfigInstaller cfg = new ConfigInstaller {
                GameFileName = this.GameFileName,
                EditorFileName = this.EditorFileName,
                DestinationName = this.DestinationName,
                Name = this.Name,
            };
            
            for (int i = 0; i < this._installerComponents.Count; i++) {
                ConfigInstallerComponent comp = this._installerComponents[i];
                cfg.InstallerComponents.Add(comp.Clone());
            }

            return cfg;
        }

        public static ConfigInstaller? FromJsonData(ConfigDataInstaller data) {
            if (data.Name != null &&
                data.Category != null &&
                data.InstallerComponents != null) {
                
                ConfigInstaller cfg = new ConfigInstaller() {
                    Name = data.Name,
                    DestinationName = data.DestinationName ?? "",
                    Category = data.Category,
                    GameFileName = data.GameFileName ?? data.ApplicationFileName ?? "",
                    EditorFileName = data.EditorFileName ?? "",
                };

                foreach (ConfigDataInstallerComponent compJ in data.InstallerComponents) {
                    ConfigInstallerComponent? comp = ConfigInstallerComponent.FromJsonData(compJ);
                    // if installer definition is incomplete, abort
                    if (comp == null) return null;
                    cfg._installerComponents.Add(comp);
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
                DestinationName = this.DestinationName,
                Category = this.Category,
                ApplicationFileName = null,
                GameFileName = string.IsNullOrWhiteSpace(this.GameFileName) == false ? this.GameFileName : null,
                EditorFileName = string.IsNullOrWhiteSpace(this.EditorFileName) == false ? this.EditorFileName : null,
                InstallerComponents = list,
            };
        }
    }
}
