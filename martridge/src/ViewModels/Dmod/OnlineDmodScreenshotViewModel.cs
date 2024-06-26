using Avalonia.Media.Imaging;
using Martridge.Models.OnlineDmods;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.IO;

namespace Martridge.ViewModels.Dmod {
    public class OnlineDmodScreenshotViewModel : ViewModelBase {
        public OnlineDmodScreenshot DmodScreenshot { get; }

        public Bitmap? ScreenshotPreview {
            get => this._screenshotPreview;
            set => this.RaiseAndSetIfChanged(ref this._screenshotPreview, value);
        }
        private Bitmap? _screenshotPreview;
        
        public Bitmap? Screenshot {
            get => this._screenshot;
            set => this.RaiseAndSetIfChanged(ref this._screenshot, value);
        }
        private Bitmap? _screenshot;
        public OnlineDmodScreenshotViewModel(OnlineDmodScreenshot screenshot) {
            this.DmodScreenshot = screenshot;

            this.ReloadScreenshotPreviewFile();
            
            // normal screenshot file is loaded in later...
        }

        public void ReloadScreenshotPreviewFile() {
            try {
                OnlineDmodCachedResource? res = OnlineDmodCachedResource.FromRelativeFileUrl(this.DmodScreenshot.RelativePreviewUrl);
                if (res != null && File.Exists(res.Local)) {
                    try {
                        this.ScreenshotPreview = new Bitmap(res.Local);
                    } catch (Exception ex) {
                        // some of the preview files are corrupted... if we can't load them, use the full image for preview i guess
                        OnlineDmodCachedResource? res2 = OnlineDmodCachedResource.FromRelativeFileUrl(this.DmodScreenshot.RelativeScreenshotUrl);
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
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                this.ScreenshotPreview = null;
            }
        }
        
        public void ReloadScreenshotFile() {
            try {
                OnlineDmodCachedResource? res = OnlineDmodCachedResource.FromRelativeFileUrl(this.DmodScreenshot.RelativeScreenshotUrl);
                if (res != null && File.Exists(res.Local)) {
                    this.Screenshot = new Bitmap(res.Local);
                } else {
                    this.Screenshot = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                this.Screenshot = null;
            }
        }
    }
}
