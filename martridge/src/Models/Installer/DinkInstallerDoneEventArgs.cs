using Martridge.Models.Configuration;
using System;
using System.IO;

namespace Martridge.Models.Installer {
    public class DinkInstallerDoneEventArgs : EventArgs {
        public DinkInstallerResult Result { get; }

        public ConfigInstaller? UsedInstaller { get; } = null;

        public DirectoryInfo? Destination { get; } = null;

        public Exception? Exception { get; } = null;

        public DinkInstallerDoneEventArgs(DinkInstallerResult result, ConfigInstaller? installer = null, DirectoryInfo? dest = null) {
            this.Result = result;
            this.UsedInstaller = installer;
            this.Destination = dest;
        }

        public DinkInstallerDoneEventArgs(Exception ex, ConfigInstaller? installer = null, DirectoryInfo? dest = null) {
            this.Result = DinkInstallerResult.Error;
            this.Exception = ex;
            this.UsedInstaller = installer;
            this.Destination = dest;
        }

    }
}
