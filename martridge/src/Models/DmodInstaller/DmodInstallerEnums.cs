namespace Martridge.Models.DmodInstaller
{
    public enum DmodInstallPhase
    {
        // awaiting initialization...
        Inactive = 0,
        
        // busy initializing dmod
        Initializing = 1,
        
        // wait for user input to start installing 
        AwaitingUserInput = 2,
        
        // busy installing dmod
        Installing = 3,
        
        // busy cleaning up temporary files
        Cleanup = 4,
        
        // installer is done!...
        Finished = 5,
    }
    
    public enum DmodInstallPreprocessingMode {
        // just set sourceFile name, do not look at archive structure
        None,
            
        // try to determine top level dmod root dir from the first top level directory name
        QuickPeek,
            
        // look at all the files in the dmod archive
        PeekAll,
    }

}
