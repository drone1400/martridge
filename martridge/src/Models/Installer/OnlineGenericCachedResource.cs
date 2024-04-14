using System.IO;
namespace Martridge.Models.Installer {
    public class OnlineGenericCachedResource
    {
        public string Local { get; private set; } = "";
        public string Url { get; private set; } = "";

        public static OnlineGenericCachedResource FromManualInput(string localFile, string url) {
            return new OnlineGenericCachedResource() {
                Local = Path.Combine(LocationHelper.WebCache, localFile),
                Url = url,
            };
        }
    }
}
