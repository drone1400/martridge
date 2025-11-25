using System;
using System.Collections.Generic;
namespace Martridge.Models.Configuration {
    public class ConfigUpdateEventArgs : EventArgs {
        
        /// <summary>
        /// Names of updated properties
        /// </summary>
        public List<string> UpdatedProperties { get; private set; }
        
        public ConfigUpdateEventArgs(List<string> updatedProperties) {
            this.UpdatedProperties = updatedProperties;
        }
    }
}
