using Martridge.Trace;
using System;
using System.IO;

namespace Martridge.Models.OnlineDmods {
    public class OnlineDmodCachedResource {
        public string Local { get; private set; } = "";
        public string Url { get; private set; } = "";
        public static string DinkNetworkUrlBase => "https://www.dinknetwork.com";
        
        public static OnlineDmodCachedResource FromDmodListPageNumber(int pageNumber) {
            return new OnlineDmodCachedResource() {
                Local = Path.Combine(new [] {
                    LocationHelper.WebCache, "dinknetwork", "file", ".dmodlist", $"page{pageNumber}.html",
                }),
                Url = $"{DinkNetworkUrlBase}/files/category_dmod/sort_title-asc/page_{pageNumber}/",
            };
        }

        public static OnlineDmodCachedResource? FromRelativeFileUrl(string relativeUrl) {
            if (relativeUrl.StartsWith('/') == false ||
                relativeUrl.EndsWith('/')) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Invalid Relative DinkNetwork url: \"{relativeUrl}\"");
                return null;
            }

            string? local = GetLocalPathFromRelativeUrl(relativeUrl);
            if (local == null) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Invalid Relative DinkNetwork url: \"{relativeUrl}\"");
                return null;
            }
            
            return new OnlineDmodCachedResource() {
                Local = local,
                Url = DinkNetworkUrlBase + relativeUrl,
            };
        }

        public static OnlineDmodCachedResource? FromRelativeScreenshotPageUrl(string relativeUrl) {
            if (relativeUrl.StartsWith('/') == false ||
                relativeUrl.EndsWith('/') == false) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Invalid Relative DinkNetwork url: \"{relativeUrl}\"");
                return null;
            }

            string? local = GetLocalPathFromRelativeUrl(relativeUrl);
            if (local == null) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Invalid Relative DinkNetwork url: \"{relativeUrl}\"");
                return null;
            }
            
            local = Path.Combine(local, "page.html");
            
            return new OnlineDmodCachedResource() {
                Local = local,
                Url = DinkNetworkUrlBase + relativeUrl,
            };
        }

        public static string? GetLocalPathFromRelativeUrl(string relativeUrl) {
            if (relativeUrl.StartsWith('/') == false) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Invalid Relative DinkNetwork url: \"{relativeUrl}\"");
                return null;
            }
            
            string[] split = relativeUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);

            string local = Path.Combine(new [] {
                LocationHelper.WebCache, "dinknetwork",
            });

            for (int i = 0; i < split.Length; i++) {
                local = Path.Combine(local, split[i]);
            }

            return local;
        }

        public static OnlineDmodCachedResource FromManualInput(string local, string url) {
            return new OnlineDmodCachedResource() {
                Local = local,
                Url = url,
            };
        }

    }
}
