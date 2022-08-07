using Avalonia.Media;
using Martridge.Models.Localization;
using Martridge.Models.OnlineDmods;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Martridge.ViewModels.Dmod {
    public class OnlineDmodInfoViewModel : ViewModelBase{
        
        //
        // Properties parsed from the DMOD list directly
        //
        public OnlineDmodInfo DmodInfo { get;}

        public string Name { get => this.DmodInfo.Name; }
        public string Author { get => this.DmodInfo.Author; }
        public string UrlMain { get => this.DmodInfo.ResMain.Url; }
        public int Downloads { get => this.DmodInfo.Downloads; }
        public string Updated { get => this.DmodInfo.Updated.ToString("d"); }
        
        public double ScoreValue { get => this.DmodInfo.Score; }
        public string Score { get => $"{this.DmodInfo.Score:0.0}"; }
        
        //
        // Properties parsed from the individual DMOD page
        //
        
        public string Description {
            get => this._description;
            private set => this.RaiseAndSetIfChanged(ref this._description, value);
        }
        private string _description = "";

        public ObservableCollection<OnlineDmodVersionViewModel> Versions {
            get => this._versions;
            private set => this.RaiseAndSetIfChanged(ref this._versions, value);
        }
        private ObservableCollection<OnlineDmodVersionViewModel> _versions = new ObservableCollection<OnlineDmodVersionViewModel>();

        public ObservableCollection<OnlineDmodReviewViewModel> Reviews {
            get => this._reviews;
            private set => this.RaiseAndSetIfChanged(ref this._reviews, value);
        }
        private ObservableCollection<OnlineDmodReviewViewModel> _reviews = new ObservableCollection<OnlineDmodReviewViewModel>();
        
        public ObservableCollection<OnlineDmodScreenshotViewModel> Screenshots {
            get => this._screenshots;
            private set => this.RaiseAndSetIfChanged(ref this._screenshots, value);
        }
        private ObservableCollection<OnlineDmodScreenshotViewModel> _screenshots = new ObservableCollection<OnlineDmodScreenshotViewModel>();
        
        public OnlineDmodInfoViewModel(OnlineDmodInfo dmodInfo) {
            this.DmodInfo = dmodInfo;
        }

        public void RefreshOnlineData(Dictionary<string, OnlineUserViewModel> cachedUsersViewModels) {
            this.Description = this.DmodInfo.Description ?? "";
            
            ObservableCollection<OnlineDmodVersionViewModel> versions = new ObservableCollection<OnlineDmodVersionViewModel>();
            foreach (OnlineDmodVersion ver in this.DmodInfo.DmodVersions) {
                versions.Add(new OnlineDmodVersionViewModel(ver));
            }
            this.Versions = versions;
            
            ObservableCollection<OnlineDmodReviewViewModel> reviews = new ObservableCollection<OnlineDmodReviewViewModel>();
            foreach (OnlineDmodReview rev in this.DmodInfo.DmodReviews) {
                reviews.Add(new OnlineDmodReviewViewModel(rev, cachedUsersViewModels[rev.User.Name]));
            }
            this.Reviews = reviews;
            
            ObservableCollection<OnlineDmodScreenshotViewModel> screenshots = new ObservableCollection<OnlineDmodScreenshotViewModel>();
            foreach (OnlineDmodScreenshot scr in this.DmodInfo.DmodScreenshots) {
                screenshots.Add(new OnlineDmodScreenshotViewModel(scr));
            }
            this.Screenshots = screenshots;
        }

        public void UnloadOnlineData() {
            this.Description = "";
            this.Versions = new ObservableCollection<OnlineDmodVersionViewModel>();
            this.Reviews = new ObservableCollection<OnlineDmodReviewViewModel>();
            this.Screenshots = new ObservableCollection<OnlineDmodScreenshotViewModel>();
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
    }
}
