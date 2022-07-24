namespace Martridge.Models.OnlineDmods {
    public class OnlineHelper {
        public static string? DmodUrlToLocalFile(string rawUrl) {
            // Example url:
            // https://www.dinknetwork.com/files/category_dmod/sort_title-asc/page_1/
            //
            string rawUrlLower = rawUrl.ToLowerInvariant();
            string protocolHttps = @"https://";
            string protocolHttp = @"http://";
            string domain = "www.dinknetwork.com";
            
            if (rawUrlLower.StartsWith(protocolHttps)) {
                rawUrlLower = rawUrlLower.Remove(0, protocolHttps.Length);
            } else if (rawUrlLower.StartsWith(protocolHttp)) {
                rawUrlLower = rawUrlLower.Remove(0, protocolHttp.Length);
            } else {
                // this is not an http / https page... idk what to do with it
                return null;
            }

            string[] components = rawUrlLower.Split('/');
            if (components[0] != domain) {
                // this is not a dink network page... idk what to do with it
                return null;
            }
            
            if (rawUrlLower.StartsWith(@"https://www.dinknetwork.com/") == false &&
                rawUrlLower.StartsWith(@"http://www.dinknetwork.com/") == false) {
                
            }

            return "";
        }
    }
}
