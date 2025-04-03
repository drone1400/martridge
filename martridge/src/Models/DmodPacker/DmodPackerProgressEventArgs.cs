using System;
using Martridge.Models.Installer;
namespace Martridge.Models.DmodPacker
{
    public class DmodPackerProgressEventArgs : EventArgs {
        public DateTime Timestamp { get; }
        public DmodPackerPhase Phase { get; }
        public DinkInstallerResult Result { get; }
        public double ProgressPercent { get; }

        public DmodPackerProgressEventArgs(DmodPackerPhase phase, DinkInstallerResult result, double progressPercent) {
            this.Timestamp = DateTime.Now;
            this.Phase = phase;
            this.Result = result;
            this.ProgressPercent = progressPercent;
        }
    }
}
