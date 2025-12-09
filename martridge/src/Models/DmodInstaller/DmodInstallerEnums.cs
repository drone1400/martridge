namespace Martridge.Models.DmodInstaller
{
    public enum DmodInstallPhase
    {
        // awaiting initialization...
        Inactive = 0,
        
        // busy initializing dmod
        Decompressing = 1,
        
        // wait for user input to start installing 
        AwaitingUserInput = 2,
        
        // busy installing dmod
        CopyingFiles = 3,
        
        // busy cleaning up temporary files
        Cleanup = 4,
        
        // installer is done!...
        Finished = 5,
    }
}
