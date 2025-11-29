using System;
using Martridge.Models.OnlineDmods;
namespace Martridge.ViewModels.OnlineDmod {
    public class OnlineDmodVersionViewModel : ViewModelBase {
        private OnlineDmodVersion? _dmodVersion = null;
        
        public string Name { get => this._dmodVersion?.Name ?? string.Empty; }
        public DateTime Released { get => this._dmodVersion?.Released ?? DateTime.MinValue; }
        public int Downloads { get => this._dmodVersion?.Downloads ?? 0; }
        public string FileSizeString { get => this._dmodVersion?.FileSizeString ?? string.Empty; }
        public string ReleaseNotes { get => this._dmodVersion?.ReleaseNotes ?? string.Empty; }
        public string RelativeDownloadUrl {get => this._dmodVersion?.RelativeDownloadUrl ?? string.Empty; }
        
        public OnlineDmodVersionViewModel(OnlineDmodVersion dmodVersion) {
            this._dmodVersion = dmodVersion;
        }

        private bool _disposed = false;
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            
            if (this._disposed) return;

            if (disposing) {
                this._dmodVersion = null;
            }
            
            this._disposed = true;
        }
    }
}
