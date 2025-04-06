using System;

namespace Martridge.Models.Installer {
    public class DinkInstallerProgressEventArgs {
        public DateTime Timestamp { get; }
        public string HeadingMain { get; }
        public string HeadingSecondary { get; }
        public double ProgressPercent { get; }

        public DinkInstallerProgressEventArgs(string headingMain, string headingSecondary, double progressPercent) {
            this.Timestamp = DateTime.Now;
            this.HeadingMain = headingMain;
            this.HeadingSecondary = headingSecondary;
            this.ProgressPercent = progressPercent;
        }
    }
}
