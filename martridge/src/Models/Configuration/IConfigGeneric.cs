using System;
using System.Collections.Generic;
namespace Martridge.Models.Configuration {
    public interface IConfigGeneric {
        
        /// <summary>
        /// Fired when one or multiple properties are updated
        /// </summary>
        event EventHandler<ConfigUpdateEventArgs>? Updated;

        /// <summary>
        /// Updates multiple properties and fires the Updated event once
        /// </summary>
        /// <param name="newValues"></param>
        void UpdateProperties(Dictionary<string, object?> newValues);
    }
}
