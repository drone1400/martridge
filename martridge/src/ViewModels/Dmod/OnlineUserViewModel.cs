using Avalonia;
using Avalonia.Media.Imaging;
using Martridge.Models.OnlineDmods;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Martridge.ViewModels.Dmod {
    public class OnlineUserViewModel : ViewModelBase {
        public OnlineUser User { get; }

        public string Name { get => this.User.Name; }
        public string TagLine { get => this.User.TagLine; }
        
        public Bitmap? PfpImageBackground {
            get => this._pfpImageBackground;
            private set => this.RaiseAndSetIfChanged(ref this._pfpImageBackground, value);
        }
        private Bitmap? _pfpImageBackground;
        
        public Bitmap? PfpImageForeground {
            get => this._pfpImageForeground;
            private set => this.RaiseAndSetIfChanged(ref this._pfpImageForeground, value);
        }
        private Bitmap? _pfpImageForeground;
        
        public ObservableCollection<Bitmap> BadgeImages {
            get => this._badgeImages;
            private set => this.RaiseAndSetIfChanged(ref this._badgeImages, value);
        }
        private ObservableCollection<Bitmap> _badgeImages = new ObservableCollection<Bitmap>();

        public OnlineUserViewModel(OnlineUser user) {
            this.User = user;
            this.ReloadImages();
        }

        public void ReloadImages() {
            OnlineDmodCachedResource? pfpBack = OnlineDmodCachedResource.FromRelativeFileUrl(this.User.RelativePfpBackgroundUrl);
            OnlineDmodCachedResource? pfpFore = OnlineDmodCachedResource.FromRelativeFileUrl(this.User.RelativePfpForegroundUrl);

            try {
                if (pfpBack != null && File.Exists(pfpBack.Local)) {
                    this.PfpImageBackground = new Bitmap(pfpBack.Local);
                } else {
                    this.PfpImageBackground = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                this.PfpImageBackground = null;
            }

            try {
                if (pfpFore != null && File.Exists(pfpFore.Local)) {
                    this.PfpImageForeground = new Bitmap(pfpFore.Local);
                } else {
                    this.PfpImageForeground = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                this.PfpImageForeground = null;
            }

            ObservableCollection<Bitmap> badges = new ObservableCollection<Bitmap>();
            foreach (string relativeImg in this.User.RelativeBadgeIconUrls) {
                try {
                    OnlineDmodCachedResource? res = OnlineDmodCachedResource.FromRelativeFileUrl(relativeImg);
                    if (res != null && File.Exists(res.Local)) {
                        Bitmap image = new Bitmap(res.Local);
                        badges.Add(image);
                    }
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                }
            }
            this.BadgeImages = badges;
        }
    }
}
