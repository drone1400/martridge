using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using Martridge.Models.OnlineDmods;
using Martridge.Trace;
using ReactiveUI;
namespace Martridge.ViewModels.OnlineDmod {
    public class OnlineUserViewModel : ViewModelBase {
        private OnlineUser? _user = null;

        public string Name { get => this._user?.Name ?? string.Empty; }
        public string TagLine { get => this._user?.TagLine ?? string.Empty; }
        
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
        
        public IReadOnlyList<Bitmap> BadgeImages {
            get => this._badgeImages;
            private set => this.RaiseAndSetIfChanged(ref this._badgeImages, value);
        }
        private IReadOnlyList<Bitmap> _badgeImages = new ObservableCollection<Bitmap>();

        public OnlineUserViewModel(OnlineUser user) {
            this._user = user;
            this.ReloadImages();
        }

        public void ReloadImages() {
            if (this._user == null)
                return;
            
            // make sure to unload old images first...
            this.UnloadImages();
            
            OnlineDmodCachedResource? pfpBack = OnlineDmodCachedResource.FromRelativeFileUrl(this._user.RelativePfpBackgroundUrl);
            OnlineDmodCachedResource? pfpFore = OnlineDmodCachedResource.FromRelativeFileUrl(this._user.RelativePfpForegroundUrl);

            try {
                if (pfpBack != null && File.Exists(pfpBack.Local)) {
                    this.PfpImageBackground = new Bitmap(pfpBack.Local);
                } else {
                    this.PfpImageBackground = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                this.PfpImageBackground = null;
            }

            try {
                if (pfpFore != null && File.Exists(pfpFore.Local)) {
                    this.PfpImageForeground = new Bitmap(pfpFore.Local);
                } else {
                    this.PfpImageForeground = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                this.PfpImageForeground = null;
            }

            List<Bitmap> badges = new List<Bitmap>();
            foreach (string relativeImg in this._user.RelativeBadgeIconUrls) {
                try {
                    OnlineDmodCachedResource? res = OnlineDmodCachedResource.FromRelativeFileUrl(relativeImg);
                    if (res != null && File.Exists(res.Local)) {
                        Bitmap image = new Bitmap(res.Local);
                        badges.Add(image);
                    }
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(ex);
                }
            }
            this.BadgeImages = badges;
        }

        public void UnloadImages() {
            this.PfpImageBackground?.Dispose();
            this.PfpImageForeground?.Dispose();
            foreach (var badge in this.BadgeImages) {
                badge.Dispose();
            }
            this.BadgeImages = new List<Bitmap>();
        }

        
        private bool _disposed = false;
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (this._disposed) return;
            
            if (disposing) {
                // ...
            }
            
            this.UnloadImages();
            
            this._disposed = true;
        }
    }
}
