using System;

namespace Martridge.Models.OnlineDmods {
    public class OnlineDmodVersion {
        public string Name { get; }
        public DateTime Released { get; }
        public int Downloads { get; }
        public string FileSizeString { get; }
        public string ReleaseNotes { get; }
        public string RelativeDownloadUrl { get; }

        public OnlineDmodVersion(string name, string relativeDownloadUrl, string releaseNotes, int downloads, string fileSizeStr, DateTime released) {
            this.Name = name;
            this.Released = released;
            this.FileSizeString = fileSizeStr;
            this.Downloads = downloads;
            this.ReleaseNotes = releaseNotes;
            this.RelativeDownloadUrl = relativeDownloadUrl;
        }
    }
}
