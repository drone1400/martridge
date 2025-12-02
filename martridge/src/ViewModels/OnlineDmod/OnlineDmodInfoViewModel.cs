using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Martridge.Models.OnlineDmods;
using ReactiveUI;
namespace Martridge.ViewModels.OnlineDmod {
    public class OnlineDmodInfoViewModel : ViewModelBase{
        
        //
        // Properties parsed from the DMOD list directly
        //
        public OnlineDmodInfo DmodInfo { get;}

        public string Name { get => this.DmodInfo.Name; }
        public string Author { get => this._author; }
        private string _author = string.Empty;
        public string AuthorOnePerLine { get => this._authorOnePerLine; }
        private string _authorOnePerLine = string.Empty;
        public string UrlMain { get => this.DmodInfo.ResMain.Url; }
        public int Downloads { get => this.DmodInfo.Downloads; }
        public DateTime Updated { get => this._updated; }
        private DateTime _updated;
        
        public double ScoreValue { get => this.DmodInfo.Score; }
        public string Score { get => $"{this.DmodInfo.Score:0.0}"; }
        
        //
        // Properties parsed from the individual DMOD page
        //
        
        public DateTime LastRefreshed {
            get => this._lastRefreshed;
            private set => this.RaiseAndSetIfChanged(ref this._lastRefreshed, value);
        }
        private DateTime _lastRefreshed = DateTime.MinValue;
        
        public string Description {
            get => this._description;
            private set => this.RaiseAndSetIfChanged(ref this._description, value);
        }
        private string _description = "";

        public IReadOnlyList<OnlineDmodVersionViewModel> Versions {
            get => this._versions;
            private set => this.RaiseAndSetIfChanged(ref this._versions, value);
        }
        private IReadOnlyList<OnlineDmodVersionViewModel> _versions = new List<OnlineDmodVersionViewModel>();

        public IReadOnlyList<OnlineDmodReviewViewModel> Reviews {
            get => this._reviews;
            private set => this.RaiseAndSetIfChanged(ref this._reviews, value);
        }
        private IReadOnlyList<OnlineDmodReviewViewModel> _reviews = new List<OnlineDmodReviewViewModel>();
        
        public IReadOnlyList<OnlineDmodScreenshotViewModel> Screenshots {
            get => this._screenshots;
            private set => this.RaiseAndSetIfChanged(ref this._screenshots, value);
        }
        private IReadOnlyList<OnlineDmodScreenshotViewModel> _screenshots = new List<OnlineDmodScreenshotViewModel>();
        
        public OnlineDmodInfoViewModel(OnlineDmodInfo dmodInfo) {
            this.DmodInfo = dmodInfo;

            this._author = this.DmodInfo.Author;
            this._authorOnePerLine = string.Empty;
            string[] authors = this.DmodInfo.Author.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int idx = 0; idx < authors.Length; idx++) {
                if (idx > 0) {
                    this._authorOnePerLine += Environment.NewLine;
                }
                if (idx >= 3) {
                    this._authorOnePerLine += "...";
                    break;
                }
                
                this._authorOnePerLine += authors[idx];
            }
            this._updated = this.DmodInfo.Updated;
        }

        public void RefreshOnlineData(Dictionary<string, OnlineUserViewModel> cachedUsersViewModels) {
            this.Description = this.DmodInfo.Description ?? "";
            
            List<OnlineDmodVersionViewModel> versions = new List<OnlineDmodVersionViewModel>();
            foreach (OnlineDmodVersion ver in this.DmodInfo.DmodVersions) {
                versions.Add(new OnlineDmodVersionViewModel(ver));
            }
            this.Versions = versions;
            
            List<OnlineDmodReviewViewModel> reviews = new List<OnlineDmodReviewViewModel>();
            foreach (OnlineDmodReview rev in this.DmodInfo.DmodReviews) {
                reviews.Add(new OnlineDmodReviewViewModel(rev, cachedUsersViewModels[rev.User.Name]));
            }
            this.Reviews = reviews;
            
            List<OnlineDmodScreenshotViewModel> screenshots = new List<OnlineDmodScreenshotViewModel>();
            foreach (OnlineDmodScreenshot scr in this.DmodInfo.DmodScreenshots) {
                screenshots.Add(new OnlineDmodScreenshotViewModel(scr));
            }
            this.Screenshots = screenshots;
            
            string testFile = this.DmodInfo.ResMain.Local;
            if (File.Exists(testFile)) {
                FileInfo finfoTest = new FileInfo(testFile);
                this.LastRefreshed = finfoTest.LastWriteTime;
            }
            else {
                this.LastRefreshed = DateTime.MinValue;
            }
        }

        public void UnloadOnlineData() {
            // NOTE: for future reference, this does not disposed the cached model data, only the view models themselves
            
            foreach (var version in this.Versions) {
                version.Dispose();
            }
            foreach (var review in this.Reviews) {
                review.Dispose();
            }
            foreach (var screenshot in this.Screenshots) {
                screenshot.Dispose();
            }
            
            this.Description = "";
            this.Versions = new List<OnlineDmodVersionViewModel>();
            this.Reviews = new List<OnlineDmodReviewViewModel>();
            this.Screenshots = new List<OnlineDmodScreenshotViewModel>();
        }
        
        public override string ToString() {
            return $"{this.Name} [{this.UrlMain}]";
        }
        
        public void CmdOpenDmodHyperlink(object? parameter = null) {

            ProcessStartInfo pinfo = new ProcessStartInfo(this.UrlMain) {
                UseShellExecute = true,
                Verb = "open",
            };
            Process.Start(pinfo);
        }
        public bool CanCmdOpenDmodHyperlink(object? parameter = null) {
            return true;
        }
        
        public void CmdOpenCacheLocation(object? parameter) {
            ProcessStartInfo pinfo = new ProcessStartInfo(this.DmodInfo.LocalBase) {
                UseShellExecute = true,
                Verb = "open",
            };
            Process.Start(pinfo);
        }
        public bool CanCmdOpenCacheLocation(object? parameter) {
            if (Directory.Exists(this.DmodInfo.LocalBase)) return true;
            return false;
        }

        private bool _disposed = false;
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (this._disposed) return;

            if (disposing) {
                this.UnloadOnlineData();
            }
            
            this._disposed = true;
        }
    }
}
