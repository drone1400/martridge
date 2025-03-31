using System;
using Martridge.Models.Installer;
namespace Martridge.Models.DmodInstaller
{
    public class DmodInstallerProgressEventArgs {
        public DateTime Timestamp { get; }
        public DmodInstallPhase Phase { get; }
        public DinkInstallerResult Result { get; }
        public double ProgressPercent { get; }

        public DmodInstallerProgressEventArgs(DmodInstallPhase phase, DinkInstallerResult result, double progressPercent) {
            this.Timestamp = DateTime.Now;
            this.Phase = phase;
            this.Result = result;
            this.ProgressPercent = progressPercent;
        }
    }
}
