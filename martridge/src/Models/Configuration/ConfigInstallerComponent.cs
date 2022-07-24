using System.Collections.Generic;

namespace Martridge.Models.Configuration {
    public class ConfigInstallerComponent {

        public ConfigWebResource WebResource { get; set; } = new ConfigWebResource();

        public List<string> FileFilterList { get; set; } = new List<string>();

        public InstallerFiltering FileFilterMode { get; set; } = InstallerFiltering.NoFiltering;

        public ConfigInstallerComponent Clone() {
            ConfigInstallerComponent comp = new ConfigInstallerComponent {
                FileFilterMode = this.FileFilterMode,
            };
            
            foreach (string str in this.FileFilterList) {
                comp.FileFilterList.Add(str);
            }
            comp.WebResource = new ConfigWebResource() {
                Uri = this.WebResource.Uri,
                Name = this.WebResource.Name,
                Sha256 = this.WebResource.Sha256,
                CheckSha256 = this.WebResource.CheckSha256,
                ResourceArchiveFormat = this.WebResource.ResourceArchiveFormat,
            };

            return comp;
        }
    }
}
