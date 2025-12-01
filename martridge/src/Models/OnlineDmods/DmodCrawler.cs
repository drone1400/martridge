using HtmlAgilityPack;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Martridge.Models.OnlineDmods {
    public class DmodCrawler : IDisposable, INotifyPropertyChanged {
        
        public static DmodCrawler Instance { get; } = new DmodCrawler();
        
        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            try {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        
        #endregion
        
        public event EventHandler? DmodListInitialized;
        private void OnDmodListInitialized() {
            try {
                this.DmodListInitialized?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        
        public bool IsBusy {
            get {
                lock (this._syncRootBusy) {
                    return this._isBusy;
                }
            }
            private set {
                lock (this._syncRootBusy) {
                    this.SetField(ref this._isBusy, value);
                }
            }
        }
        private bool _isBusy = false;
        private readonly object _syncRootBusy = new object();

        public bool IsInitializingDmodList {
            get {
                lock (this._syncRootBusy) {
                    return this._isInitializingDmodList;
                }
            }
            private set {
                lock (this._syncRootBusy) {
                    this.SetField(ref this._isInitializingDmodList, value);
                }
            }
        }
        private bool _isInitializingDmodList = false;

        public DateTime DmodPagesLastWriteTime {
            get => this._dmodPagesLastWriteTime;
            private set => this.SetField(ref this._dmodPagesLastWriteTime, value);
        }
        private DateTime _dmodPagesLastWriteTime = DateTime.MinValue;

        public DateTime DmodPagesOldestWriteTime {
            get => this._dmodPagesOldestWriteTime;
            private set => this.SetField(ref this._dmodPagesOldestWriteTime, value);
        }
        private DateTime _dmodPagesOldestWriteTime = DateTime.MinValue;
        

        private readonly HttpClient _httpClient = new HttpClient() {
            // if not set, default timeout should be ~100 seconds but that seems too long...
            Timeout = TimeSpan.FromSeconds(30), // TODO, make this not hardcoded in the future...
        };
        
        private readonly int _knownDmodPages = 9; // TODO, make this not hardcoded in the future...

        public List<OnlineDmodInfo> DmodList { get => this._dmodList; }
        private List<OnlineDmodInfo> _dmodList = new List<OnlineDmodInfo>();

        private Dictionary<string, OnlineUser> _onlineUsers = new Dictionary<string, OnlineUser>();

        private TimeSpan _genericHttpClienTaskWaitToStartTime = TimeSpan.FromSeconds(30);
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        
        public async Task InitializeDmodLists(bool forceOnlineRefresh) {
            bool hasLock = false;
            
            try {
                if (this._disposed)
                    return;

                lock (this._syncRootBusy) {
                    if (this._isInitializingDmodList)
                        return;
                    this.IsInitializingDmodList = true;
                }

                hasLock = await this.TryStartHttpClientTask(this._genericHttpClienTaskWaitToStartTime, this._cancellationTokenSource.Token);
                if (hasLock == false) {
                    // failed to obtain lock...
                    return;
                }
                
                int dmodPageIdx = 1;

                List<OnlineDmodInfo> dmodEntries = new List<OnlineDmodInfo>();

                bool nextPageExists = false;
                while (dmodPageIdx <= this._knownDmodPages || nextPageExists) {
                    try {
                        OnlineDmodCachedResource cachedResource = OnlineDmodCachedResource.FromDmodListPageNumber(dmodPageIdx);

                        FileInfo localHtml = new FileInfo(cachedResource.Local);
                        if (localHtml.Directory?.Exists == false) {
                            localHtml.Directory.Create();
                        }
                        if (localHtml.Exists == false || forceOnlineRefresh) {
                            bool success = await this.DownloadWebContentInternal(cachedResource);
                            if (!success) {
                                MyTrace.Global.WriteMessage($"Error downloading dmod lists #{dmodPageIdx}...", MyTraceLevel.Error);
                            }
                        }
                        localHtml.Refresh();
                        if (localHtml.Exists) {
                            if (localHtml.LastWriteTime > this.DmodPagesLastWriteTime) {
                                this.DmodPagesLastWriteTime = localHtml.LastWriteTime;
                            }
                            if (localHtml.LastWriteTime < this.DmodPagesOldestWriteTime) {
                                this.DmodPagesOldestWriteTime =  localHtml.LastWriteTime;
                            }
                            int dmodCount = this.ParseDmodsPage(cachedResource.Local, dmodEntries);
                            nextPageExists = this.ParseDmodsNextPageExists(cachedResource.Local);
                        }
                        else {
                            nextPageExists = false;
                        }

                        dmodPageIdx++;
                    } catch (Exception ex) {
                        MyTrace.Global.WriteMessage($"Error parsing dmod lists #{dmodPageIdx}...", MyTraceLevel.Error);
                        MyTrace.Global.WriteException(ex);
                    }
                }

                this._dmodList = dmodEntries;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            } finally {
                if (hasLock) {
                    this.IsBusy = false;
                    this.IsInitializingDmodList = false;
                }
                this.OnDmodListInitialized();
            }
        }

        public async Task UpdateDmodData(OnlineDmodInfo dmodInfo, bool forceRefresh) {
            bool hasLock = false;

            try {
                if (this._disposed)
                    return;

                hasLock = await this.TryStartHttpClientTask(this._genericHttpClienTaskWaitToStartTime, this._cancellationTokenSource.Token);
                if (hasLock == false) {
                    // failed to obtain lock...
                    MyTrace.Global.WriteMessage("Error reading online resource, Online DMOD Crawler seems busy... Try again later?");
                    return;
                }

                if (!Directory.Exists(dmodInfo.LocalBase)) {
                    Directory.CreateDirectory(dmodInfo.LocalBase);
                }

                //
                // cache html if necessary
                //

                if (File.Exists(dmodInfo.ResMain.Local) == false || forceRefresh) {
                    if (!await this.DownloadWebContentInternal(dmodInfo.ResMain)) {
                        MyTrace.Global.WriteMessage($"Error reading online DMOD data from: {dmodInfo.ResMain.Url}", MyTraceLevel.Error);
                    }
                }

                if (File.Exists(dmodInfo.ResVersions.Local) == false || forceRefresh) {
                    if (!await this.DownloadWebContentInternal(dmodInfo.ResVersions)) {
                        MyTrace.Global.WriteMessage($"Error reading online DMOD data from: {dmodInfo.ResVersions.Url}", MyTraceLevel.Error);
                    }
                }

                if (File.Exists(dmodInfo.ResReviews.Local) == false || forceRefresh) {
                    if (!await this.DownloadWebContentInternal(dmodInfo.ResReviews)) {
                        MyTrace.Global.WriteMessage($"Error reading online DMOD data from: {dmodInfo.ResReviews.Url}", MyTraceLevel.Error);
                    }
                }

                if (File.Exists(dmodInfo.ResScreenshots.Local) == false || forceRefresh) {
                    if (!await this.DownloadWebContentInternal(dmodInfo.ResScreenshots)) {
                        MyTrace.Global.WriteMessage($"Error reading online DMOD data from: {dmodInfo.ResScreenshots.Url}", MyTraceLevel.Error);
                    }
                }

                //
                //
                //
                string? description = this.ParseDmodDescription(dmodInfo.ResMain.Local);
                List<OnlineDmodVersion> versions = this.ParseDmodVersions(dmodInfo.ResVersions.Local);
                List<OnlineDmodReview> reviews = this.ParseDmodReviews(dmodInfo.ResReviews.Local);
                List<OnlineDmodScreenshot> screenshots = await this.ParseDmodScreenshots(dmodInfo.ResScreenshots.Local);

                dmodInfo.UpdateOnlineInfo(description, versions, reviews, screenshots);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            } finally {
                if (hasLock) {
                    this.IsBusy = false;
                }
            }
        }
        
        public async Task UpdateDmodVersionData(OnlineDmodInfo dmodInfo, bool forceRefresh) {
            bool hasLock = false;
            
            try {
                if (this._disposed)
                    return;
                
                hasLock = await this.TryStartHttpClientTask(this._genericHttpClienTaskWaitToStartTime, this._cancellationTokenSource.Token);
                if (hasLock == false) {
                    // failed to obtain lock...
                    MyTrace.Global.WriteMessage("Error reading online resource, Online DMOD Crawler seems busy... Try again later?");
                    return;
                }
                
                if (!Directory.Exists(dmodInfo.LocalBase)) {
                    Directory.CreateDirectory(dmodInfo.LocalBase);
                }

                //
                // cache html if necessary
                //

                if (File.Exists(dmodInfo.ResVersions.Local) == false || forceRefresh) {
                    if (!await this.DownloadWebContentInternal(dmodInfo.ResVersions)) {
                        MyTrace.Global.WriteMessage($"Error reading online DMOD data from: {dmodInfo.ResVersions.Url}", MyTraceLevel.Error);
                    }
                }

                //
                //
                //
                List<OnlineDmodVersion> versions = this.ParseDmodVersions(dmodInfo.ResVersions.Local);

                dmodInfo.UpdateVersionInfo(versions);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            } finally {
                if (hasLock) {
                    this.IsBusy = false;
                }
            }
        }

        public async Task CacheUserData(OnlineUser user, bool forceRefresh) {
            bool hasLock = false;
            
            try {
                if (this._disposed)
                    return;
                
                hasLock = await this.TryStartHttpClientTask(this._genericHttpClienTaskWaitToStartTime, this._cancellationTokenSource.Token);
                if (hasLock == false) {
                    // failed to obtain lock...
                    MyTrace.Global.WriteMessage("Error reading online resource, Online DMOD Crawler seems busy... Try again later?");
                    return;
                }
            
                List<OnlineDmodCachedResource> resources = new List<OnlineDmodCachedResource>();
                
                OnlineDmodCachedResource? pfpBack = OnlineDmodCachedResource.FromRelativeFileUrl(user.RelativePfpBackgroundUrl);
                OnlineDmodCachedResource? pfpFore = OnlineDmodCachedResource.FromRelativeFileUrl(user.RelativePfpForegroundUrl);

                if (pfpBack != null) resources.Add(pfpBack);
                if (pfpFore != null) resources.Add(pfpFore);
                
                foreach (string badgeStr in user.RelativeBadgeIconUrls) {
                    OnlineDmodCachedResource? badge = OnlineDmodCachedResource.FromRelativeFileUrl(badgeStr);
                    if (badge != null) resources.Add(badge);
                }

                foreach (OnlineDmodCachedResource res in resources) {
                    if (File.Exists(res.Local) == false || forceRefresh) {
                        if (!await this.DownloadWebContentInternal(res)) {
                            MyTrace.Global.WriteMessage($"Error reading online DMOD data from: {res.Url}", MyTraceLevel.Error);
                        } 
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            } finally {
                if (hasLock) {
                    this.IsBusy = false;
                }
            }
        }

        public async Task<bool> DownloadWebContent(OnlineDmodCachedResource res) {
            bool hasLock = false;
            
            try {
                if (this._disposed)
                    return false;
                
                hasLock = await this.TryStartHttpClientTask(this._genericHttpClienTaskWaitToStartTime, this._cancellationTokenSource.Token);
                if (hasLock == false) {
                    // failed to obtain lock...
                    MyTrace.Global.WriteMessage("Error reading online resource, Online DMOD Crawler seems busy... Try again later?");
                    return false;
                }

                return await this.DownloadWebContentInternal(res);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                return false;
            } finally {
                if (hasLock) {
                    this.IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Downloads online DMOD resources without checking/setting busy flag
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private async Task<bool> DownloadWebContentInternal(OnlineDmodCachedResource res) {
            try {
                if (this._disposed)
                    return false;
                
                MyTrace.Global.WriteMessage($"Sending HTTP Request to URL: \"{res.Url}\"");

                // first only read the header
                using HttpResponseMessage responseHeader = await this._httpClient.GetAsync(res.Url, HttpCompletionOption.ResponseHeadersRead);
                // check if request was successful
                if (responseHeader.IsSuccessStatusCode == false) {
                    MyTrace.Global.WriteMessage($"Error executing HTTP Request, StatusCode={responseHeader.StatusCode}");
                    return false;
                }

                // since headers were ok, now read the content
                using HttpResponseMessage response = await this._httpClient.GetAsync(res.Url, HttpCompletionOption.ResponseContentRead);
                // check if request was successful
                if (response.IsSuccessStatusCode == false) {
                    MyTrace.Global.WriteMessage($"Error executing HTTP Request, StatusCode={response.StatusCode}");
                    return false;
                }

                // ensure parent directory exists
                FileInfo fileInfo = new FileInfo(res.Local);
                if (fileInfo.Directory?.Exists == false) {
                    fileInfo.Directory.Create();
                }

                // create file stream and read the data
                await using FileStream fileStream = new FileStream(res.Local, FileMode.Create, FileAccess.Write, FileShare.Read);
                MyTrace.Global.WriteMessage($"    HTTP Response Status = {response.StatusCode}");
                await response.Content.CopyToAsync(fileStream);
                MyTrace.Global.WriteMessage($"    Content saved to = {res.Local}");
                
                // all done, yay
                return true;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                return false;
            }
            
        }
        
        /// <summary>
        /// Tries to start a HttpClient related task
        /// </summary>
        /// <param name="waitTimeout">maximum TimeSpan to wait</param>
        /// <param name="cancellationToken">Cancellation token to stop waiting</param>
        /// <returns>true if lock was successful and IsBusy flag was set to true</returns>
        private async Task<bool> TryStartHttpClientTask(TimeSpan waitTimeout, CancellationToken cancellationToken) {
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime +  waitTimeout;

            while (DateTime.Now < endTime) {
                if (cancellationToken.IsCancellationRequested)
                    return false;
                lock (this._syncRootBusy) {
                    if (this._isBusy == false) {
                        this.IsBusy = true;
                        return true;
                    }
                }
                await Task.Delay(500, cancellationToken);
            }
            return false;
        }
        
        #region Dmod Parsing

        private string? ParseDmodDescription(string localHtml) {
            try {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.Load(localHtml);

                HtmlNodeCollection infoboxContentNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='subcontent']//div[@class='infoboxcontent']");

                // description should be stored in the first 'infoboxcontent' div, the next one has the review data...
                HtmlNode? description = infoboxContentNodes.First();

                return description?.InnerText;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage("Error parsing Online Dmod Description", MyTraceLevel.Error);
                MyTrace.Global.WriteException(ex);
                return null;
            }
        }
        
        private async Task<List<OnlineDmodScreenshot>> ParseDmodScreenshots(string localHtml) {
            try {
                
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.Load(localHtml);

                HtmlNodeCollection? screenshotLinks = htmlDoc.DocumentNode.SelectNodes("//div[@class='subcontent']//div[@class='infobox']//div[@class='infoboxcontent']//a");
                
                List<OnlineDmodScreenshot> screenshots = new List<OnlineDmodScreenshot>();

                if (screenshotLinks != null) {
                    for (int i = 0; i < screenshotLinks.Count; i++) {
                        string pageUrl = screenshotLinks[i].Attributes["href"].Value;
                        string previewUrl = screenshotLinks[i].SelectSingleNode(".//img").Attributes["src"].Value;
                        string screenshotUrl = "";

                        OnlineDmodCachedResource? res = OnlineDmodCachedResource.FromRelativeScreenshotPageUrl(pageUrl);

                        if (res != null) {
                            // add screenshot if resource for it is valid
                            
                            if (File.Exists(res.Local) == false) {
                                if (!await this.DownloadWebContentInternal(res)) {
                                    MyTrace.Global.WriteMessage($"Error reading online DMOD data from: {res.Url}", MyTraceLevel.Error);
                                }
                            }

                            if (File.Exists(res.Local)) {
                                HtmlDocument htmlDocScreenshot = new HtmlDocument();
                                htmlDocScreenshot.Load(res.Local);

                                HtmlNode screenshotPageLink =
                                    htmlDocScreenshot.DocumentNode.SelectSingleNode("//div[@class='subcontent']//div[@class='infobox']//div[@class='infoboxcontent']//a");
                                screenshotUrl = screenshotPageLink.SelectSingleNode(".//img").Attributes["src"].Value;
                            }

                            OnlineDmodScreenshot screenshot = new OnlineDmodScreenshot(previewUrl, pageUrl, screenshotUrl);
                            screenshots.Add(screenshot);
                        }
                    }
                }

                return screenshots;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage("Error parsing Online Dmod Screenshots", MyTraceLevel.Error);
                MyTrace.Global.WriteException(ex);
                return new List<OnlineDmodScreenshot>();
            }
        }

        private List<OnlineDmodReview> ParseDmodReviews(string localHtml) {
            try {
                
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.Load(localHtml);

                HtmlNodeCollection? reviewCaptionNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='subcontent']//div[@class='caption']");
                HtmlNodeCollection? reviewContentNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='subcontent']//div[@class='infobox']");
                
                List<OnlineDmodReview> reviews = new List<OnlineDmodReview>();

                if (reviewCaptionNodes != null && reviewContentNodes != null) {
                    for (int i = 0; i < reviewCaptionNodes.Count; i++) {
                        string reviewName = reviewCaptionNodes[i].InnerText;

                        HtmlNode reviewContent = reviewContentNodes[i].SelectSingleNode(".//div[@class='infoboxcontent']");
                        string reviewText = reviewContent.InnerText;

                        HtmlNode reviewHeader = reviewContentNodes[i].SelectSingleNode(".//div[@class='infoboxheader']");

                        HtmlNodeCollection reviewMeta = reviewHeader.SelectNodes(".//div[@class='infoboxeyes']//div[@class='friendlytext']");
                        string reviewDateStr = reviewMeta[0].InnerText;
                        string reviewVersion = reviewMeta[1].InnerText;
                        DateTime reviewDate = ShittyParseDateTime(reviewDateStr);

                        HtmlNodeCollection reviewScoreImages = reviewHeader.SelectNodes(".//div[@class='foreground']//img");
                        string rScoreDigit1Str = reviewScoreImages[1].Attributes["alt"].Value;
                        string rScoreDigit2Str = reviewScoreImages[3].Attributes["alt"].Value;
                        double reviewScore = double.NaN;

                        try {
                            reviewScore = int.Parse(rScoreDigit1Str) + 0.1 * int.Parse(rScoreDigit2Str);
                        } catch (Exception ex) {
                            MyTrace.Global.WriteMessage("Error parsing Online Dmod Review Score", MyTraceLevel.Error);
                            MyTrace.Global.WriteException(ex);
                        }


                        HtmlNode userNode = reviewHeader.SelectSingleNode(".//div[@class='userdetail']//a[@class='username']");
                        string userName = userNode.InnerText;

                        if (!this._onlineUsers.ContainsKey(userName)) {

                            string userUrl = userNode.Attributes["href"].Value;

                            HtmlNodeCollection userBadges = reviewHeader.SelectNodes(".//div[@class='usericons']//img");
                            List<string> userBadgeUrl = new List<string>();
                            foreach (HtmlNode node in userBadges) {
                                userBadgeUrl.Add(node.Attributes["src"].Value);
                            }

                            string pfpBackgroundRaw = reviewHeader.SelectSingleNode(".//div[@class='persona']//div[@class='personaback']").Attributes["style"].Value.Trim();
                            // pfpBackgroundUrl looks something like
                            // background-image: url(/images/locations/forest.gif);
                            // it has 22 characters, then the url, then 2 more characters, so we extract the substring that is the effective url...
                            string pfpBackgroundUrl = pfpBackgroundRaw.Substring(22, pfpBackgroundRaw.Length - 24);
                            string pfpForegroundUrl = reviewHeader.SelectSingleNode(".//div[@class='persona']//img").Attributes["src"].Value;

                            string userTagline = reviewHeader.SelectSingleNode(".//div[@class='tagline']").InnerText;
                            userTagline = userTagline.Replace("&nbsp;", " ");

                            OnlineUser newUser = new OnlineUser(userName, userUrl, pfpBackgroundUrl, pfpForegroundUrl, userTagline, userBadgeUrl);
                            this._onlineUsers.Add(userName, newUser);
                        }

                        OnlineDmodReview review = new OnlineDmodReview(this._onlineUsers[userName], reviewName, reviewText, reviewDate, reviewVersion, reviewScore);
                        reviews.Add(review);
                    }
                }

                return reviews.OrderByDescending(x => x.ReviewDate).ThenByDescending(x => x.User.Name).ToList();
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage("Error parsing Online Dmod Reviews", MyTraceLevel.Error);
                MyTrace.Global.WriteException(ex);
                return new List<OnlineDmodReview>();
            }
        }

        private List<OnlineDmodVersion> ParseDmodVersions(string localHtml) {
            try {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.Load(localHtml);

                HtmlNodeCollection? versionCaptions = htmlDoc.DocumentNode.SelectNodes("//div[@class='subcontent']//div[@class='caption']");
                HtmlNodeCollection? versionTables = htmlDoc.DocumentNode.SelectNodes("//div[@class='subcontent']//table[@class='infobox']");

                List<OnlineDmodVersion> versions = new List<OnlineDmodVersion>();

                if (versionCaptions != null && versionTables != null) {
                    for (int i = 0; i < versionCaptions.Count; i++) {
                        string name = versionCaptions[i].InnerText;
                        HtmlNodeCollection tableData = versionTables[i].SelectNodes(".//tr//td");

                        string dateStr = tableData[0].InnerText;
                        string sizeStr = tableData[1].InnerText;
                        string downloadsStr = tableData[2].InnerText;
                        string releaseNotes = tableData[3].InnerText;

                        DateTime date = ShittyParseDateTime(dateStr);

                        if (!int.TryParse(downloadsStr, out int downloads)) {
                            MyTrace.Global.WriteMessage("Error parsing dmod version downloads...", MyTraceLevel.Warning);
                        }
                        string downloadUrl = "";

                        // parsing download url... should be the first link that starts with https://www.dinknetwork.com/download/
                        HtmlNodeCollection dmodDownloads = versionTables[i].SelectNodes(".//tr//td[@class='download']//a");
                        for (int j = 0; j < dmodDownloads.Count; j++) {
                            string href = dmodDownloads[j].Attributes["href"].Value;
                            if (href.ToLowerInvariant().StartsWith(@"/download/")) {
                                downloadUrl = href;
                                break;
                            }
                        }

                        OnlineDmodVersion version = new OnlineDmodVersion(name, downloadUrl, releaseNotes, downloads, sizeStr, date );
                        versions.Add(version);
                    }
                }

                return versions.OrderByDescending(x => x.Released).ThenByDescending(x => x.Name).ToList();
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage("Error parsing Online Dmod Versions", MyTraceLevel.Error);
                MyTrace.Global.WriteException(ex);
                return new List<OnlineDmodVersion>();
            }
        }

        #endregion

        #region Dmod List parsing
        private bool ParseDmodsNextPageExists(string localCache) {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(localCache);
            HtmlNode? linksUl = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='links']");
            HtmlNode? nextPage = linksUl?.SelectSingleNode("//a[.='Next']");
            if (nextPage != null) {
                return true;
            }
            return false;
        }
        private int ParseDmodsPage(string localCache, List<OnlineDmodInfo> entries) {
            // make sure html doc exists
            if (File.Exists(localCache) == false) {
                MyTrace.Global.WriteMessage("Tried parsing file that does not exist, this should be impossible?!...", MyTraceLevel.Error);
                MyTrace.Global.WriteMessage($"    Path=\"{localCache}\"");
                return 0;
            }
            
            
            int rowCount = 0;
            
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(localCache);
            
            HtmlNodeCollection? tableRows = htmlDoc.DocumentNode.SelectNodes("//table[@class='index']//tr");
            if (tableRows != null) {
                
                foreach (HtmlNode node in tableRows) {
                    try {
                        // row columns are defined like this:
                        // Title, Author, Updated, Downloads, Score
                        // the score is displayed using images of single digits
                        
                        HtmlNodeCollection? cells = node.SelectNodes(".//td");
                        if (cells != null && cells.Count == 5) {
                            HtmlNode dmodLink = cells[0].SelectSingleNode(".//a");
                            string dmodUrl = dmodLink.Attributes["href"].Value;
                            string dmodName = dmodLink.InnerText;
                            string dmodAuthor = cells[1].InnerText;
                            string dmodUpdatedText = cells[2].InnerText;
                            string dmodDownloadsText = cells[3].InnerText;
                            DateTime dmodUpdated = DateTime.Parse(dmodUpdatedText);
                            if (!int.TryParse(dmodDownloadsText, out int dmodDownloads)) {
                                MyTrace.Global.WriteMessage($"Error parsing dmod downloads number for dmod #{rowCount}, {dmodName}...", MyTraceLevel.Warning);
                            }
                            double dmodScore = double.NaN;
                            
                            HtmlNodeCollection? divScoreForegroundImg = cells[4].SelectNodes(".//div[@class='foreground']//img");
                            if (divScoreForegroundImg != null && divScoreForegroundImg.Count == 3) {
                                int scoreA = int.Parse(divScoreForegroundImg[0].Attributes["alt"].Value);
                                int scoreB = int.Parse(divScoreForegroundImg[2].Attributes["alt"].Value);
                                dmodScore = scoreA + 0.1 * scoreB;
                            }
                            
                            
                            OnlineDmodInfo dmod = new OnlineDmodInfo(
                                relativeUrlMain: dmodUrl.Trim(),
                                name: dmodName.Trim(),
                                author: dmodAuthor.Trim(),
                                updated: dmodUpdated,
                                downloads: dmodDownloads,
                                score: dmodScore
                            );
                            entries.Add(dmod);
                        }
                    } catch (Exception ex) {
                        MyTrace.Global.WriteMessage($"Error parsing dmod entry #{rowCount}...", MyTraceLevel.Error);
                        MyTrace.Global.WriteException(ex);
                    }

                    rowCount++;
                }
            }
            
            return rowCount;
        }
        
        #endregion
        
        #region  Misc Helper Functions

        private static DateTime ShittyParseDateTime(string dateStr) {
            // dirty and easy way to parse date, just remove the number suffix and use DateTime.Parse...
            dateStr = Regex.Replace(dateStr, @"([0-9]+)(st|nd|rd|th)", "$1", RegexOptions.IgnoreCase );

            if (!DateTime.TryParse(dateStr, out DateTime date)) {
                MyTrace.Global.WriteMessage("Error parsing dmod version release date...", MyTraceLevel.Warning);
                date = DateTime.MinValue;
            }

            return date;
        }

        

        #endregion


        ~DmodCrawler() {
            this.Dispose(false);
        }
        
        public void Dispose() {
            this.Dispose(true);
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing) {
            if (this._disposed)
                return;
            
            if (disposing) {
                // ...
                this._onlineUsers.Clear();
                this._dmodList.Clear();
            }

            this._cancellationTokenSource.Cancel();
            this._httpClient.Dispose();
            
            this._disposed = true;
        }
        
    }
}
