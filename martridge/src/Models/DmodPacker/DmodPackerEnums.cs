namespace Martridge.Models.DmodPacker
{
    public enum DmodPackerPhase
    {
        // awaiting initialization...
        Inactive = 0,
        
        // busy initializing dmod (scanning the DMOD folder contents...)
        Initializing = 1,
        
        // wait for user input to start packing (user can choose to ignore files/folders)
        AwaitingUserInput = 2,
        
        // busy packing dmod
        Packing = 3,
        
        // busy cleaning up temporary files (hmmm....)
        Cleanup = 4,
        
        // installer is done!...
        Finished = 5,
    }
}
