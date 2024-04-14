using System.Collections.Generic;

namespace Martridge.Models.Configuration.Save {
    
    /// <summary>
    /// Class used for JSON serialization
    /// </summary>
    public class ConfigDataInstaller {
        public string? Name { get; set; }
        public string? DestinationName { get; set; }
        
        // legacy property from V2...
        public string? ApplicationFileName { get; set; }
        
        public string? GameFileName { get; set; }
        public string? EditorFileName { get; set; }
        public string? Category { get; set; }

        public List<ConfigDataInstallerComponent>? InstallerComponents { get; set; }
    }
}
