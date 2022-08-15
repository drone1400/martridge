using System;
using System.Collections.Generic;
using System.IO;

namespace Martridge.Models.OnlineDmods {
    public class OnlineDmodInfo {
        public string Name { get; }
        
        public string LocalBase { get; }
        public OnlineDmodCachedResource ResMain { get; }
        public OnlineDmodCachedResource ResReviews { get; }
        public OnlineDmodCachedResource ResScreenshots { get; }
        public OnlineDmodCachedResource ResVersions { get; }

        public string Author { get; }
        public DateTime Updated { get; }
        public int Downloads { get; }
        public double Score { get; }

        public string? Description { get; private set; } = null;

        public List<OnlineDmodVersion> DmodVersions { get; private set; } = new List<OnlineDmodVersion>();
        
        public List<OnlineDmodReview> DmodReviews { get; private set; } = new List<OnlineDmodReview>();
        public List<OnlineDmodScreenshot> DmodScreenshots { get; private set; } = new List<OnlineDmodScreenshot>();

        public OnlineDmodInfo(string relativeUrlMain, string name, string author, DateTime updated, int downloads, double score) {
            this.Name = name;
            this.Author = author;
            this.Updated = updated;
            this.Downloads = downloads;
            this.Score = score;

            string? localBase = OnlineDmodCachedResource.GetLocalPathFromRelativeUrl(relativeUrlMain);

            if (localBase == null) {
                // Note: this should be impossible, i think?
                throw new ArgumentException($"Could not initialize local cache for online dmod info using relative url: \"{relativeUrlMain}\"");
            }
            
            this.LocalBase = localBase;
            
            this.ResMain = OnlineDmodCachedResource.FromManualInput(
                local: Path.Combine(this.LocalBase, "page_main.html"),
                url: OnlineDmodCachedResource.DinkNetworkUrlBase + relativeUrlMain);
            
            this.ResVersions = OnlineDmodCachedResource.FromManualInput(
                local: Path.Combine(this.LocalBase, "page_versions.html"),
                url: OnlineDmodCachedResource.DinkNetworkUrlBase + relativeUrlMain + "versions/");
            
            this.ResReviews = OnlineDmodCachedResource.FromManualInput(
                local: Path.Combine(this.LocalBase, "page_reviews.html"),
                url: OnlineDmodCachedResource.DinkNetworkUrlBase + relativeUrlMain + "reviews/");
            
            this.ResScreenshots = OnlineDmodCachedResource.FromManualInput(
                local: Path.Combine(this.LocalBase, "page_screenshots.html"),
                url: OnlineDmodCachedResource.DinkNetworkUrlBase + relativeUrlMain + "screenshots/");
        }

        public void UpdateOnlineInfo(string? description, 
                List<OnlineDmodVersion> versions, 
                List<OnlineDmodReview> reviews,
                List<OnlineDmodScreenshot> screenshots) {
            this.Description = description;
            this.DmodVersions = versions;
            this.DmodReviews = reviews;
            this.DmodScreenshots = screenshots;
        }
    }
}
