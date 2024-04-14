using System.Collections.Generic;
namespace Martridge.Models.Configuration.Save {
    
    /// <summary>
    /// Class used for JSON serialization
    /// </summary>
    public class ConfigDataAlertResults {

        public string? Placeholder { get; set; }

        public Dictionary<string, object?> GetValues() {
            return new Dictionary<string, object?>() {
                // TODO... map property values here...
                [nameof(ConfigDataAlertResults.Placeholder)] = this.Placeholder,
            };
        }
    }
}
