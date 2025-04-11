namespace Martridge.Models.DinkInstaller {

    public enum DinkInstallPhase
    {
        Inactive = 0,                   // has not started installing yet
        Preparing = 1,                  // various preparations...
        DownloadingResources = 2,       // downloading resources
        Installing = 3,                 // actually installing
        Cleanup = 4,                    // cleaning up temporary files
        Finished = 5,                   // all done!
    }
}
