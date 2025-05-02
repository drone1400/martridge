using Martridge.Models.OnlineDmods;
using ReactiveUI;
namespace Martridge.ViewModels.OnlineDmod {
    public class OnlineDmodReviewViewModel : ViewModelBase {
        private OnlineDmodReview? _dmodReview = null; 

        public OnlineUserViewModel? UserViewModel {
            get => this._userViewModel;
            private set => this.RaiseAndSetIfChanged(ref this._userViewModel, value);
        }
        private OnlineUserViewModel? _userViewModel = null;
        
        public string ReviewName { get => this._dmodReview?.ReviewName ?? string.Empty; }
        public string ReviewText { get => this._dmodReview?.ReviewText ?? string.Empty; }
        public string ReviewDate { get => this._dmodReview?.ReviewDate.ToString("d") ?? string.Empty; }
        public string ReviewVersion { get => this._dmodReview?.ReviewVersion ?? string.Empty; }
        
        public double ReviewScoreValue { get => this._dmodReview?.ReviewScore ?? 0.0; }
        public string ReviewScore { get => this._dmodReview != null ? $"{this._dmodReview.ReviewScore:0.0}" : string.Empty; }

        public OnlineDmodReviewViewModel(OnlineDmodReview dmodReview, OnlineUserViewModel userViewModel) {
            this._dmodReview = dmodReview;
            this.UserViewModel = userViewModel;
        }

        private bool _disposed = false;
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (this._disposed) return;
            
            if (disposing) {
                this.UserViewModel = null;
                this._dmodReview = null;
                // NOTE: do not actually dispose of the view model...
            }
            
            this._disposed = true;
        }
    }
}
