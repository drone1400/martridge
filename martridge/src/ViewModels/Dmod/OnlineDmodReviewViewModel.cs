using Martridge.Models.OnlineDmods;

namespace Martridge.ViewModels.Dmod {
    public class OnlineDmodReviewViewModel {
        public OnlineDmodReview DmodReview { get;}
        
        public OnlineUserViewModel UserViewModel { get; }
        public string ReviewName { get => this.DmodReview.ReviewName; }
        public string ReviewText { get => this.DmodReview.ReviewText; }
        public string ReviewDate { get => this.DmodReview.ReviewDate.ToString("d"); }
        public string ReviewVersion { get => this.DmodReview.ReviewVersion; }
        
        public double ReviewScoreValue { get => this.DmodReview.ReviewScore; }
        public string ReviewScore { get => $"{this.DmodReview.ReviewScore:0.0}";}

        public OnlineDmodReviewViewModel(OnlineDmodReview dmodReview, OnlineUserViewModel userViewModel) {
            this.DmodReview = dmodReview;
            this.UserViewModel = userViewModel;
        }
    }
}
