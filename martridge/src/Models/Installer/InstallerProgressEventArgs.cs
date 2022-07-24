using System;

namespace Martridge.Models.Installer {
    public class InstallerProgressEventArgs {
        public DateTime Timestamp { get; }
        public InstallerReportLevel ProgressLevel { get; }
        public string HeadingMain { get; }
        public string HeadingSecondary { get; }
        public double ProgressPercent { get; }

        public InstallerProgressEventArgs(InstallerReportLevel level, string headingMain, string headingSecondary, double progressPercent) {
            this.Timestamp = DateTime.Now;
            this.ProgressLevel = level;
            this.HeadingMain = headingMain;
            this.HeadingSecondary = headingSecondary;
            this.ProgressPercent = progressPercent;
        }
    }
}
