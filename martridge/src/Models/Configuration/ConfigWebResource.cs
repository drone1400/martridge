using Martridge.Models.Configuration.Save;

namespace Martridge.Models.Configuration {
    public class ConfigWebResource {
        public string Uri { get; private set; } = "";
        public string Name { get; private set; } = "";
        public string Sha256 { get; private set; } = "";
        public bool CheckSha256 { get; private set; } = false;
        public string ResourceArchiveFormat { get; private set; } = "Unknown";

        public ConfigWebResource Clone() {
            return new ConfigWebResource() {
                Uri = this.Uri,
                Name = this.Name,
                Sha256 = this.Sha256,
                CheckSha256 = this.CheckSha256,
                ResourceArchiveFormat = this.ResourceArchiveFormat,
            };
        }

        public static ConfigWebResource? FromJsonData(ConfigDataWebResource data) {
            if (data.Uri != null &&
                data.Name != null &&
                data.ResourceArchiveFormat != null) {
                
                return new ConfigWebResource() {
                    Uri = data.Uri,
                    Name = data.Name,
                    ResourceArchiveFormat = data.ResourceArchiveFormat,
                    Sha256 = (data.Sha256 != null && data.CheckSha256 == true) ? data.Sha256 : "",
                    CheckSha256 = (data.Sha256 != null && data.CheckSha256 == true),
                };
            }
            return null;
        }

        public ConfigDataWebResource ToJsonData() {
            return new ConfigDataWebResource() {
                Uri = this.Uri,
                Name = this.Name,
                Sha256 = this.Sha256,
                CheckSha256 = this.CheckSha256,
                ResourceArchiveFormat = this.ResourceArchiveFormat,
            };
        }
    }
}
