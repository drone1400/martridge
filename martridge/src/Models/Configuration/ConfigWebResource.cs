namespace Martridge.Models.Configuration {
    public class ConfigWebResource {
        public string Uri { get; set; } = "";
        public string Name { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public bool CheckSha256 { get; set; } = false;
        public string ResourceArchiveFormat { get; set; } = "Unknown";
    }
}
