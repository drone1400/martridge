using System;
using System.Collections.Generic;

namespace Martridge.Models.OnlineDmods {
    public class OnlineDmodReview {

        public OnlineUser User { get; }
        public string ReviewName { get; }
        public string ReviewText { get; }
        public DateTime ReviewDate { get; }
        public string ReviewVersion { get; }
        public double ReviewScore { get; }

        public OnlineDmodReview(OnlineUser user, string reviewName, string reviewText, DateTime reviewDate, string reviewVersion, double reviewScore) {
            this.User = user;
            this.ReviewName = reviewName;
            this.ReviewText = reviewText;
            this.ReviewDate = reviewDate;
            this.ReviewVersion = reviewVersion;
            this.ReviewScore = reviewScore;
        }

    }
}
