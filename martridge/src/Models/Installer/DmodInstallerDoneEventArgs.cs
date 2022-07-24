using System;
using System.IO;

namespace Martridge.Models.Installer {
    public class DmodInstallerDoneEventArgs : EventArgs {
        public DinkInstallerResult Result { get; }

        public FileInfo? Source { get; } = null;

        public DirectoryInfo? Destination { get; } = null;

        public Exception? Exception { get; } = null;

        public DmodInstallerDoneEventArgs(DinkInstallerResult result, FileInfo? source = null, DirectoryInfo? dest = null) {
            this.Result = result;
            this.Source = source;
            this.Destination = dest;
        }

        public DmodInstallerDoneEventArgs(Exception ex, FileInfo? source = null, DirectoryInfo? dest = null) {
            this.Result = DinkInstallerResult.Error;
            this.Exception = ex;
            this.Source = source;
            this.Destination = dest;
        }

    }
}
