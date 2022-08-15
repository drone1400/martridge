using System;

namespace Martridge.Models.Installer {
    public enum DinkInstallerResult {
        Success,
        Cancelled,
        Error,
    }
    

    public class DinkInstallerExceptionEventArgs : EventArgs{
        public Exception Exception { get; }

        public DinkInstallerExceptionEventArgs(Exception ex) {
            this.Exception = ex;
        }
    }

    public class DinkInstallerInvalidDmodFormatException : Exception {
        public DinkInstallerInvalidDmodFormatException() : base() { }
        public DinkInstallerInvalidDmodFormatException(string? msg) : base(msg) { }
        public DinkInstallerInvalidDmodFormatException(string? msg, Exception innerEx) : base(msg, innerEx) { }
    }

    public class DinkInstallerFileSystemException : Exception {

        public DinkInstallerFileSystemException() : base() { }
        public DinkInstallerFileSystemException(string? msg) : base(msg) { }

        public DinkInstallerFileSystemException(string? msg, Exception innerEx) : base(msg, innerEx) { }
    }

    public class DinkInstallerDownloadException : Exception {

        public DinkInstallerDownloadException() : base() { }
        public DinkInstallerDownloadException(string? msg) : base(msg) { }

        public DinkInstallerDownloadException(string? msg, Exception innerEx) : base(msg, innerEx) { }
    }

    public class DinkInstallerUnzipException : Exception {

        public DinkInstallerUnzipException() : base() { }
        public DinkInstallerUnzipException(string? msg) : base(msg) { }

        public DinkInstallerUnzipException(string? msg, Exception innerEx) : base(msg, innerEx) { }
    }

    public class DinkInstallerCancelledByUserException : Exception {

    }

    
}
