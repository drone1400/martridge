using System;
using System.IO;
using Avalonia.Media.Imaging;
using Martridge.Models.OnlineDmods;
using Martridge.Trace;
using ReactiveUI;
namespace Martridge.ViewModels.OnlineDmod {
    public class OnlineDmodScreenshotViewModel : ViewModelBase {
        private OnlineDmodScreenshot? _dmodScreenshot = null;

        public Bitmap? ScreenshotPreview {
            get => this._screenshotPreview;
            private set => this.RaiseAndSetIfChanged(ref this._screenshotPreview, value);
        }
        private Bitmap? _screenshotPreview;
        
        public Bitmap? Screenshot {
            get => this._screenshot;
            private set => this.RaiseAndSetIfChanged(ref this._screenshot, value);
        }
        private Bitmap? _screenshot;
        
        public OnlineDmodScreenshotViewModel(OnlineDmodScreenshot screenshot) {
            this._dmodScreenshot = screenshot;

            this.ReloadScreenshotPreviewFile(false);
            //this.ReloadScreenshotFile();
        }

        public void ReloadScreenshotPreviewFile(bool force) {
            try {
                if (this._dmodScreenshot == null)
                    return;

                if (this.ScreenshotPreview != null) {
                    if (force == false)
                        return;
                    this.ScreenshotPreview.Dispose();
                    this.ScreenshotPreview = null;
                }
                
                OnlineDmodCachedResource? res = OnlineDmodCachedResource.FromRelativeFileUrl(this._dmodScreenshot.RelativePreviewUrl);
                if (res != null && File.Exists(res.Local)) {
                    try {
                        this.ScreenshotPreview = new Bitmap(res.Local);
                    } catch (Exception) {
                        // some of the preview files are corrupted... if we can't load them, use the full image for preview i guess
                        OnlineDmodCachedResource? res2 = OnlineDmodCachedResource.FromRelativeFileUrl(this._dmodScreenshot.RelativeScreenshotUrl);
                        if (res2 != null && File.Exists(res2.Local)) {
                            this.ScreenshotPreview = new Bitmap(res2.Local);
                        } else {
                            this.ScreenshotPreview = null;
                        }
                    }
                } else {
                    this.ScreenshotPreview = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                this.ScreenshotPreview = null;
            }
        }
        
        public void ReloadScreenshotFile(bool force) {
            try {
                if (this._dmodScreenshot == null)
                    return;
                
                if (this.Screenshot != null) {
                    if (force == false)
                        return;
                    this.Screenshot.Dispose();
                    this.Screenshot = null;
                }
                
                OnlineDmodCachedResource? res = OnlineDmodCachedResource.FromRelativeFileUrl(this._dmodScreenshot.RelativeScreenshotUrl);
                if (res != null && File.Exists(res.Local)) {
                    this.Screenshot = new Bitmap(res.Local);
                } else {
                    this.Screenshot = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                this.Screenshot = null;
            }
        }


        private bool _disposed = false;
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (this._disposed) return;

            if (disposing) {
                this._dmodScreenshot = null;
            }
            
            this.ScreenshotPreview?.Dispose();
            this.ScreenshotPreview = null;
                
            this.Screenshot?.Dispose();
            this.Screenshot = null;
            
            this._disposed = true;
        }
    }
}
