using Martridge.Models.OnlineDmods;

namespace Martridge.ViewModels.Dmod {
    public class OnlineDmodVersionViewModel {
        public OnlineDmodVersion DmodVersion { get;}
        
        public string Name { get => this.DmodVersion.Name; }
        public string Released { get => this.DmodVersion.Released.ToString("d"); }
        public string Downloads { get => this.DmodVersion.Downloads.ToString(); }
        public string FileSizeString { get => this.DmodVersion.FileSizeString; }
        public string ReleaseNotes { get => this.DmodVersion.ReleaseNotes; }
        
        public OnlineDmodVersionViewModel(OnlineDmodVersion dmodVersion) {
            this.DmodVersion = dmodVersion;
        }
    }
}
