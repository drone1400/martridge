using System;
using System.Collections.Generic;
namespace Martridge.Models.Configuration {
    public interface IConfigGeneric {
        event EventHandler<ConfigUpdateEventArgs>? Updated;

        void UpdateProperties(Dictionary<string, object?> newValues);
    }
}
