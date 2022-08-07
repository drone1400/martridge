using HtmlAgilityPack;
using Martridge.ViewModels.Dmod;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Martridge.Models.OnlineDmods {
    public class DmodCrawler {

        public DateTime DmodPagesLastWriteTime { get; private set; } = DateTime.MinValue;
        public event EventHandler DmodListInitialized;

        private HttpClient _httpClient = new HttpClient();
        
        private readonly int _knownDmodPages = 8;

        public List<OnlineDmodInfo> DmodList { get => this._dmodList; }
        private List<OnlineDmodInfo> _dmodList = new List<OnlineDmodInfo>();

        private Dictionary<string, OnlineUser> _onlineUsers = new Dictionary<string, OnlineUser>();

        
        
        public DmodCrawler() {
            
        }

        public async Task InitializeDmodLists(bool forceOnlineRefresh) {
            int dmodPageIdx = 1;
            
            try {
                List<OnlineDmodInfo> dmodEntries = new List<OnlineDmodInfo>();

                bool nextPageExists = false;
                while (dmodPageIdx <= this._knownDmodPages || nextPageExists) {
                    OnlineDmodCachedResource cachedResource = OnlineDmodCachedResource.FromDmodListPageNumber(dmodPageIdx);
                    FileInfo localHtml = new FileInfo(cachedResource.Local);
                    if (localHtml.Directory.Exists == false) {
                        localHtml.Directory.Create();
                    }
                    if (localHtml.Exists == false || forceOnlineRefresh) {
                        await this.DownloadWebContent(cachedResource);
                    }
                    localHtml.Refresh();
                    if (localHtml.Exists) {
                        if (localHtml.LastWriteTime > this.DmodPagesLastWriteTime) {
                            this.DmodPagesLastWriteTime = localHtml.LastWriteTime;
                        }
                        int dmodCount = this.ParseDmodsPage(cachedResource.Local, dmodEntries);
                        nextPageExists = this.ParseDmodsNextPageExists(cachedResource.Local);
                    } else {
                        nextPageExists = false;
                    }

                    dmodPageIdx++;
                }
                
                this._dmodList = dmodEntries;
                this.DmodListInitialized?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Error parsing dmod lists #{dmodPageIdx}...");
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
            }
        }

        public async Task UpdateDmodData(OnlineDmodInfo dmodInfo, bool forceRefresh) {
            if (!Directory.Exists(dmodInfo.LocalBase)) {
                Directory.CreateDirectory(dmodInfo.LocalBase);
            }

            //
            // cache html if necessary
            //
            
            if (File.Exists(dmodInfo.ResVersions.Local) == false || forceRefresh) {
                HttpStatusCode resultVersions = await this.DownloadWebContent(dmodInfo.ResVersions);
            }
            
            if (File.Exists(dmodInfo.ResMain.Local) == false || forceRefresh) {
                HttpStatusCode resultMain = await this.DownloadWebContent(dmodInfo.ResMain);
            }

            if (File.Exists(dmodInfo.ResReviews.Local) == false || forceRefresh) {
                HttpStatusCode resultReviews = await this.DownloadWebContent(dmodInfo.ResReviews);
            }

            if (File.Exists(dmodInfo.ResScreenshots.Local) == false || forceRefresh) {
                HttpStatusCode resultScreenshots = await this.DownloadWebContent(dmodInfo.ResScreenshots);
            }
            
            //
            //
            //
            string? description = this.ParseDmodDescription(dmodInfo.ResMain.Local);
            List<OnlineDmodVersion> versions = this.ParseDmodVersions(dmodInfo.ResVersions.Local);
            List<OnlineDmodReview> reviews = this.ParseDmodReviews(dmodInfo.ResReviews.Local);
            List<OnlineDmodScreenshot> screenshots = await this.ParseDmodScreenshots(dmodInfo.ResScreenshots.Local);
            
            dmodInfo.UpdateOnlineInfo(description, versions, reviews, screenshots);
        }

        public async Task CacheUserData(OnlineUser user, bool forceRefresh) {
            List<OnlineDmodCachedResource> resources = new List<OnlineDmodCachedResource>();
            
            OnlineDmodCachedResource pfpBack = OnlineDmodCachedResource.FromRelativeFileUrl(user.RelativePfpBackgroundUrl);
            OnlineDmodCachedResource pfpFore = OnlineDmodCachedResource.FromRelativeFileUrl(user.RelativePfpForegroundUrl);

            resources.Add(pfpBack);
            resources.Add(pfpFore);
            
            foreach (string badgeStr in user.RelativeBadgeIconUrls) {
                OnlineDmodCachedResource badge = OnlineDmodCachedResource.FromRelativeFileUrl(badgeStr);
                resources.Add(badge);
            }

            foreach (OnlineDmodCachedResource res in resources) {
                if (File.Exists(res.Local) == false || forceRefresh) {
                    HttpStatusCode result = await this.DownloadWebContent(res);
                }
            }
        }
        
        

        public async Task<HttpStatusCode> DownloadWebContent(OnlineDmodCachedResource res) {
            FileInfo fileInfo = new FileInfo(res.Local);
            if (fileInfo.Directory.Exists == false) {
                fileInfo.Directory.Create();
            }
            
            MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Sending HTTP Request to URL: \"{res.Url}\"");
            using (HttpResponseMessage response = await this._httpClient.GetAsync(res.Url, HttpCompletionOption.ResponseContentRead))
            using (FileStream fstream = new FileStream(res.Local, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"    HTTP Response Status = {response.StatusCode}");
                await response.Content.CopyToAsync(fstream);
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"    Content saved to = {res.Local}");

                return response.StatusCode;
            }
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
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, "Error parsing Online Dmod Description");
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
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

                        OnlineDmodCachedResource res = OnlineDmodCachedResource.FromRelativeScreenshotPageUrl(pageUrl);

                        if (File.Exists(res.Local) == false) {
                            HttpStatusCode result = await this.DownloadWebContent(res);
                        }

                        if (File.Exists(res.Local) == true) {
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

                return screenshots;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, "Error parsing Online Dmod Screenshots", MyTraceLevel.Error);
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
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
                            MyTrace.Global.WriteMessage(MyTraceCategory.Online, "Error parsing Online Dmod Review Score", MyTraceLevel.Error);
                            MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
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

                return reviews;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, "Error parsing Online Dmod Reviews", MyTraceLevel.Error);
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
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

                        int downloads = 0;
                        if (!int.TryParse(downloadsStr, out downloads)) {
                            MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Error parsing dmod version downloads...", MyTraceLevel.Warning);
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

                return versions;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, "Error parsing Online Dmod Versions", MyTraceLevel.Error);
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
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
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Tried parsing file that does not exist, this should be impossible?!...", MyTraceLevel.Error);
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"    Path=\"{localCache}\"");
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
                            if (rowCount == 44 || rowCount == 39) {
                                int x = 0;
                            }
                            
                            HtmlNode dmodLink = cells[0].SelectSingleNode(".//a");
                            string dmodUrl = dmodLink.Attributes["href"].Value;
                            string dmodName = dmodLink.InnerText;
                            string dmodAuthor = cells[1].InnerText;
                            string dmodUpdatedText = cells[2].InnerText;
                            string dmodDownloadsText = cells[3].InnerText;
                            DateTime dmodUpdated = DateTime.Parse(dmodUpdatedText);
                            int dmodDownloads = 0;
                            if (!int.TryParse(dmodDownloadsText, out dmodDownloads)) {
                                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Error parsing dmod downloads number for dmod #{rowCount}, {dmodName}...", MyTraceLevel.Warning);
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
                        MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Error parsing dmod entry #{rowCount}...", MyTraceLevel.Error);
                        MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
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
            // TODO maybe think up a better way to do this?...
            // does the date format ever change?...
            dateStr = dateStr.ToLowerInvariant();
            dateStr = dateStr.Replace("st", "");
            dateStr = dateStr.Replace("nd", "");
            dateStr = dateStr.Replace("rd", "");
            dateStr = dateStr.Replace("th", "");
                    
            DateTime date = DateTime.MinValue;
            if (!DateTime.TryParse(dateStr, out date)) {
                MyTrace.Global.WriteMessage(MyTraceCategory.Online, $"Error parsing dmod version release date...", MyTraceLevel.Warning);
            }

            return date;
        }

        

        #endregion
    }
}
