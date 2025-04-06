using System;

namespace Martridge.Models.Installer {
    public enum DinkInstallerResult {
        Success,
        Cancelled,
        Error,
    }

    public enum DinkInstallPhase
    {
        Inactive = 0,                   // has not started installing yet
        Preparing = 1,                  // various preparations...
        DownloadingResources = 2,       // downloading resources
        Installing = 3,                 // actually installing
        Cleanup = 4,                    // cleaning up temporary files
        Finished = 5,                   // all done!
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
