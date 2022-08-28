namespace Martridge.Models.Configuration.Save {
    
    /// <summary>
    /// Class used for JSON serialization
    /// </summary>
    public class ConfigDataWebResource {
        public string? Uri { get; set; }
        public string? Name { get; set; }
        public string? Sha256 { get; set; }
        public bool? CheckSha256 { get; set; }
        public string? ResourceArchiveFormat { get; set; }
    }
}
