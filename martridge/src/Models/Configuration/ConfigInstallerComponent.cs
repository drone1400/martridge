using Martridge.Models.Configuration.Save;
using System.Collections.Generic;

namespace Martridge.Models.Configuration {
    public class ConfigInstallerComponent {

        public ConfigWebResource WebResource { get; private set; } = new ConfigWebResource();

        public List<string> FileFilterList { get; private set; } = new List<string>();

        public InstallerFiltering FileFilterMode { get; private set; } = InstallerFiltering.NoFiltering;

        public ConfigInstallerComponent Clone() {
            ConfigInstallerComponent comp = new ConfigInstallerComponent {
                FileFilterMode = this.FileFilterMode,
                WebResource = this.WebResource.Clone(),
            };
            
            foreach (string str in this.FileFilterList) {
                comp.FileFilterList.Add(str);
            }

            return comp;
        }
        
        public static ConfigInstallerComponent? FromJsonData(ConfigDataInstallerComponent data) {
            if (data.WebResource != null) {
                ConfigWebResource? webResource = ConfigWebResource.FromJsonData(data.WebResource);

                if (webResource != null) {
                    ConfigInstallerComponent comp = new ConfigInstallerComponent() {
                        WebResource = webResource,
                        FileFilterMode = InstallerFiltering.NoFiltering,
                    };

                    if (data.FileFilterMode != null && data.FileFilterMode != InstallerFiltering.NoFiltering && data.FileFilterList != null) {
                        comp.FileFilterMode = (InstallerFiltering)data.FileFilterMode;
                        foreach (string s in data.FileFilterList) {
                            comp.FileFilterList.Add(s);
                        }
                    }

                    return comp;
                }
            }
            return null;
        }

        public ConfigDataInstallerComponent ToJsonData() {
            List<string> filtered = new List<string>();
            foreach (string s in this.FileFilterList) {
                filtered.Add(s);
            }
            
            return new ConfigDataInstallerComponent() {
                WebResource = this.WebResource.ToJsonData(),
                FileFilterMode = this.FileFilterMode,
                FileFilterList = filtered,
            };
        }
    }
}
