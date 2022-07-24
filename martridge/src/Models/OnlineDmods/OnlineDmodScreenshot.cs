namespace Martridge.Models.OnlineDmods {
    public class OnlineDmodScreenshot {

        public string RelativePreviewUrl { get; }
        public string RelativeScreenshotPageUrl { get; }
        public string RelativeScreenshotUrl { get; }

        public OnlineDmodScreenshot(string previewUrl, string pageUrl, string screenshotUrl) {
            this.RelativePreviewUrl = previewUrl;
            this.RelativeScreenshotPageUrl = pageUrl;
            this.RelativeScreenshotUrl = screenshotUrl;
        }
    }
}
