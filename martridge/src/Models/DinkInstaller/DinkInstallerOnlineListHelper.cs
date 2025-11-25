using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Martridge.Models.Configuration;
using Martridge.Models.Configuration.Installer;
using Martridge.Trace;
namespace Martridge.Models.DinkInstaller {
    public class DinkInstallerOnlineListHelper {

        private HttpClient _httpClient = new HttpClient();

        public static string DefaultConfigInstallerListUrl = @"https://raw.githubusercontent.com/drone1400/martridge-cfg-install/master/configInstallerList.json";

        public static TimeSpan RecacheInterval = new TimeSpan(0, 0, 30, 0, 0);
        
        public CancellationTokenSource CancelTokenSource { get; } = new CancellationTokenSource();

        protected CancellationToken CancelToken => this.CancelTokenSource.Token;

        private ConfigInstallerList? TryGetListFromLocalCachedFile(string path, bool checkWriteTime) {
            FileInfo finfo1 = new FileInfo(path);
            if (finfo1.Exists &&
                finfo1.Length > 0 && 
                (checkWriteTime == false || finfo1.LastWriteTime + DinkInstallerOnlineListHelper.RecacheInterval > DateTime.Now)) {
                // use existing file..
                ConfigInstallerList? list = ConfigInstallerList.LoadFromFile(finfo1.FullName);

                if (list != null && list.Installables.Count > 0) {
                    return list;
                }
            }
            return null;
        }

        public static bool IsValidUri(Uri uri) {
            if (uri.AbsoluteUri.StartsWith("file://")) return true;
            if (uri.AbsoluteUri.StartsWith("https://")) return true;
            if (uri.AbsoluteUri.StartsWith("http://")) return true;
            return false;
        }

        public async Task<ConfigInstallerList?> GetConfigInstallerList(Uri uri, bool ignoreCache) {
            try {
                MyTrace.Global.WriteMessage("Trying to get configInstallerList.json");

                if (uri.AbsoluteUri.StartsWith("file://")) {
                    // loading from local path
                    
                    ConfigInstallerList? list = ConfigInstallerList.LoadFromFile(uri.LocalPath);

                    if (list != null && list.Installables.Count > 0) {
                        return list;
                    }
                } else if (uri.AbsoluteUri.StartsWith("https://") || uri.AbsoluteUri.StartsWith("http://")) {
                    // loading from online resource or cached online resource
                    
                    OnlineGenericCachedResource resourceTemp = OnlineGenericCachedResource.FromManualInput(
                        "tempConfigInstallerList.json", DefaultConfigInstallerListUrl);

                    ConfigInstallerList? list = null;

                    if (ignoreCache == false) {
                        list = this.TryGetListFromLocalCachedFile(resourceTemp.Local, true);
                    }
                    
                    if (list != null) {
                        MyTrace.Global.WriteMessage("Trying to get configInstallerList.json... done!");
                        return list;
                    }

                    if (this.CancelToken.IsCancellationRequested) throw new TaskCanceledException();

                    HttpStatusCode result = await this.DownloadWebContent(resourceTemp);

                    if (this.CancelToken.IsCancellationRequested) throw new TaskCanceledException();

                    ConfigInstallerList? list2 = this.TryGetListFromLocalCachedFile(resourceTemp.Local, false);
                    if (list2 != null) {
                        MyTrace.Global.WriteMessage("Trying to get configInstallerList.json... done!");
                        return list2;
                    }
                }

                MyTrace.Global.WriteMessage("Trying to get configInstallerList.json... failed!");
                return null;
            } catch (TaskCanceledException) {
                MyTrace.Global.WriteMessage("Trying to get configInstallerList.json... cancelled by user!");
                return null;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage("Trying to get configInstallerList.json... ERROR!");
                MyTrace.Global.WriteException(ex);
                return null;
            }
        }
        
        
        public async Task<HttpStatusCode> DownloadWebContent(OnlineGenericCachedResource res) {
            FileInfo fileInfo = new FileInfo(res.Local);
            if (fileInfo.Directory?.Exists == false) {
                fileInfo.Directory.Create();
            }
            
            MyTrace.Global.WriteMessage($"Sending HTTP Request to URL: \"{res.Url}\"");
            using HttpResponseMessage response = await this._httpClient.GetAsync(res.Url, HttpCompletionOption.ResponseContentRead, this.CancelToken);
            using FileStream fstream = new FileStream(res.Local, FileMode.Create, FileAccess.Write, FileShare.Read);
            
            MyTrace.Global.WriteMessage($"    HTTP Response Status = {response.StatusCode}");
            await response.Content.CopyToAsync(fstream);
            MyTrace.Global.WriteMessage($"    Content saved to = {res.Local}");

            return response.StatusCode;
            
        }
    }
}
