using System;
using System.Collections.Generic;
namespace Martridge.Models.Configuration {
    public class ConfigUpdateEventArgs : EventArgs {
        public List<string> UpdatedProperties { get; private set; }
        
        public ConfigUpdateEventArgs(List<string> updatedProperties) {
            this.UpdatedProperties = updatedProperties;
        }
    }
}
