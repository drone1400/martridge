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
                OnlineDmodCachedResource res = OnlineDmodCachedResource.FromRelativeFileUrl(this.DmodScreenshot.RelativePreviewUrl);
                if (File.Exists(res.Local)) {
                    this.ScreenshotPreview = new Bitmap(res.Local);
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
                OnlineDmodCachedResource res = OnlineDmodCachedResource.FromRelativeFileUrl(this.DmodScreenshot.RelativePreviewUrl);
                if (File.Exists(res.Local)) {
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
