using System.Collections.Generic;

namespace Martridge.Models.Configuration.Save {
    
    /// <summary>
    /// Class used for JSON serialization
    /// </summary>
    public class ConfigDataInstallerComponent {
        public ConfigDataWebResource? WebResource { get; set; }

        public List<string>? FileFilterList { get; set; }

        public InstallerFiltering? FileFilterMode { get; set; }
    }
}
