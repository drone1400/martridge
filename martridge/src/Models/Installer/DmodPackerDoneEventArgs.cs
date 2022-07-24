using System;
using System.IO;

namespace Martridge.Models.Installer {
    public class DmodPackerDoneEventArgs : EventArgs {
        public DinkInstallerResult Result { get; }

        public FileInfo? Destination { get; } = null;

        public DirectoryInfo? Source { get; } = null;

        public Exception? Exception { get; } = null;

        public DmodPackerDoneEventArgs(DinkInstallerResult result, FileInfo? dest = null, DirectoryInfo? source = null) {
            this.Result = result;
            this.Source = source;
            this.Destination = dest;
        }

        public DmodPackerDoneEventArgs(Exception ex, FileInfo? dest = null, DirectoryInfo? source = null) {
            this.Result = DinkInstallerResult.Error;
            this.Exception = ex;
            this.Source = source;
            this.Destination = dest;
        }

    }
}
