using System;

namespace Martridge.ViewModels.DinkyAlerts {
    [Flags]
    public enum AlertResults : int {
        None = 0x00,
        Ok = 0x01,
        Yes = 0x02,
        No = 0x04,
        Cancel = 0x08,
        RememberResult = 0x8000,
    }
}
