using System;
namespace Martridge.Models {
    public enum DinkInstallerResult {
        Success,
        Cancelled,
        Error,
    }

    public class DinkInstallerFileSystemException : Exception {
        
        public DinkInstallerFileSystemException(string? msg) : base(msg) { }

        public DinkInstallerFileSystemException(string? msg, Exception innerEx) : base(msg, innerEx) { }
    }

    public class DinkInstallerDownloadException : Exception {
        
        public DinkInstallerDownloadException(string? msg) : base(msg) { }

        public DinkInstallerDownloadException(string? msg, Exception innerEx) : base(msg, innerEx) { }
    }

    public class DinkInstallerUnzipException : Exception {
        
        public DinkInstallerUnzipException(string? msg) : base(msg) { }

        public DinkInstallerUnzipException(string? msg, Exception innerEx) : base(msg, innerEx) { }
    }

    public class DinkInstallerCancelledByUserException : Exception {

    }

    
}
