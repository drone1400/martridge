using System;

namespace Martridge.Models.OnlineDmods {
    public class DmodInfoUpdatedEventArgs : EventArgs {
        public OnlineDmodInfo DmodInfo { get; }

        public DmodInfoUpdatedEventArgs(OnlineDmodInfo dmodInfo) {
            this.DmodInfo = dmodInfo;
        }
    }
}
