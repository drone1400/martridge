using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Documents;
using Martridge.Models.Configuration;
using Martridge.Models.OnlineDmods;
using Martridge.Trace;
namespace Martridge.Models.Installer {
    public class DinkInstallerOnlineListHelper {

        private HttpClient _httpClient = new HttpClient();

        public static string DefaultConfigInstallerListUrl = @"https://raw.githubusercontent.com/drone1400/martridge-cfg-install/master/configInstallerList.json";

        public static TimeSpan RecacheInterval = new TimeSpan(0, 0, 30, 0, 0);
        
        public CancellationTokenSource CancelTokenSource { get; } = new CancellationTokenSource();

        protected CancellationToken CancelToken => CancelTokenSource.Token;

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

        public async Task<ConfigInstallerList?> GetConfigInstallerList() {
            try {
                MyTrace.Global.WriteMessage(MyTraceCategory.DinkInstaller, $"Trying to get configInstallerList.json");

                OnlineGenericCachedResource resourceTemp = OnlineGenericCachedResource.FromManualInput(
                    "tempConfigInstallerList.json", DefaultConfigInstallerListUrl);

                ConfigInstallerList? list = this.TryGetListFromLocalCachedFile(resourceTemp.Local, true);
                if (list != null) {
                    MyTrace.Global.WriteMessage(MyTraceCategory.DinkInstaller, $"Trying to get configInstallerList.json... done!");
                    return list;
                }

                if (this.CancelToken.IsCancellationRequested) throw new TaskCanceledException();

                HttpStatusCode result = await DownloadWebContent(resourceTemp);

                if (this.CancelToken.IsCancellationRequested) throw new TaskCanceledException();

                ConfigInstallerList? list2 = this.TryGetListFromLocalCachedFile(resourceTemp.Local, false);
                if (list2 != null) {
                    MyTrace.Global.WriteMessage(MyTraceCategory.DinkInstaller, $"Trying to get configInstallerList.json... done!");
                    return list2;
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.DinkInstaller, $"Trying to get configInstallerList.json... failed!");
                return null;
            } catch (TaskCanceledException) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DinkInstaller, $"Trying to get configInstallerList.json... cancelled by user!");
                return null;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DinkInstaller, $"Trying to get configInstallerList.json... ERROR!");
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                return null;
            }
        }
        
        
        public async Task<HttpStatusCode> DownloadWebContent(OnlineGenericCachedResource res) {
            FileInfo fileInfo = new FileInfo(res.Local);
            if (fileInfo.Directory?.Exists == false) {
                fileInfo.Directory.Create();
            }
            
            MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Sending HTTP Request to URL: \"{res.Url}\"");
            using HttpResponseMessage response = await this._httpClient.GetAsync(res.Url, HttpCompletionOption.ResponseContentRead, this.CancelToken);
            using FileStream fstream = new FileStream(res.Local, FileMode.Create, FileAccess.Write, FileShare.Read);
            
            MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"    HTTP Response Status = {response.StatusCode}");
            await response.Content.CopyToAsync(fstream);
            MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"    Content saved to = {res.Local}");

            return response.StatusCode;
            
        }
    }
}
