using System.Collections.Generic;

namespace Martridge.Models.OnlineDmods {
    public class OnlineUser {
        public string Name { get; }
        public string TagLine { get; }
        public string RelativeProfileUrl { get; }
        public string RelativePfpBackgroundUrl { get; }
        public string RelativePfpForegroundUrl { get; }
        public List<string> RelativeBadgeIconUrls { get; }

        public OnlineUser(string userName, string relativeProfileUrl, string relativePfpBackgroundUrl, string relativePfpForegroundUrl, string tagLine, List<string> relativeBadgeIconUrls) {
            this.Name = userName;
            this.RelativeProfileUrl = relativeProfileUrl;
            this.RelativePfpBackgroundUrl = relativePfpBackgroundUrl;
            this.RelativePfpForegroundUrl = relativePfpForegroundUrl;
            this.TagLine = tagLine;
            this.RelativeBadgeIconUrls = relativeBadgeIconUrls;
        }
    }
}
