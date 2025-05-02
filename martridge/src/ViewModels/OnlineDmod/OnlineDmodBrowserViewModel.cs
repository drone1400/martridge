using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Collections;
using Avalonia.Metadata;
using Avalonia.Threading;
using Martridge.Models.Dmod;
using Martridge.Models.Localization;
using Martridge.Models.OnlineDmods;
using Martridge.Trace;
using ReactiveUI;
namespace Martridge.ViewModels.OnlineDmod {

    public class MyOnlineDmodComparer : IComparer {
        public int Compare(object? x, object? y) {
            if (x is OnlineDmodInfoViewModel x1 && y is OnlineDmodInfoViewModel y1)
            {
                return String.CompareOrdinal(x1.Author, y1.Author);
            }
            return 0;
        }
    }
    
    public class InstallOnlineDmodEventArgs : EventArgs {
        public string Path { get; }
        public InstallOnlineDmodEventArgs(string path) {
            this.Path = path;
        }
    }
    
    public class OnlineDmodBrowserViewModel : ViewModelAppPage {

        public event EventHandler<InstallOnlineDmodEventArgs>? InstallDmodRequested; 

        private readonly Timer _dmodSearchTimer;
        
        //
        // CONSTRUCTOR
        //

        /// <summary>
        /// Constructor for Online Dmod Browser / Installer
        /// </summary>
        public OnlineDmodBrowserViewModel() {
            // Dmod Search Timer
            this._dmodSearchTimer = new Timer() {
                Interval = 200,
                AutoReset = true,
            };
            this._dmodSearchTimer.Elapsed += ( sender,  args) => {
                this._dmodSearchTimer.Stop();
                this.InitializeFilteredDmods(this._lastusedDmodDefinitions);
            };
            
            // Self properties changed
            this.PropertyChanged += ( sender,  args) => {
                if (args.PropertyName == nameof(this.DmodSearchString)) {
                    if (this._dmodSearchTimer.Enabled == false) {
                        this._dmodSearchTimer.Start();
                    }
                }

                if (args.PropertyName == nameof(this.SelectedDmodDefinition)) {
                    _ = this.SelectedDmodDefinitionInitialize(false); // no await
                }

                if (args.PropertyName == nameof(this.SelectedDmodScreenshotVm)) {
                    this.OnDmodScreenshotVmChanged();
                }
            };
        }
        
        
        #region DMOD List - Properties and methods related to the DMOD list are here

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------

        public string LastRefreshedString {
            get => this._lastRefreshedString;
            private set => this.RaiseAndSetIfChanged(ref this._lastRefreshedString, value);
        }
        private string _lastRefreshedString = "";
        
        
        /// <summary>
        /// Handles online DMOD data acquisition
        /// </summary>
        public DmodCrawler? DmodCrawler {
            get => this._dmodCrawler;
            set {
                if (this._dmodCrawler != null) {
                    this._dmodCrawler.DmodListInitialized -= this.DmodCrawler_DmodListInitialized;
                    this._dmodCrawler = null;
                }
                this._dmodCrawler = value;
                this.RaisePropertyChanged(nameof(this.DmodCrawler));

                if (this._dmodCrawler != null) {
                    this._dmodCrawler.DmodListInitialized += this.DmodCrawler_DmodListInitialized;
                    this.InitializeDmods();
                }
            }
        }
        private DmodCrawler? _dmodCrawler = null;
        
        public string? DmodSearchString {
            get => this._dmodSearchString;
            set {
                if (value == "") {
                    this.RaiseAndSetIfChanged(ref this._dmodSearchString, null);
                } else {
                    this.RaiseAndSetIfChanged(ref this._dmodSearchString, value);
                }
            }
        }
        private string? _dmodSearchString = null;

        public DataGridCollectionView? DmodDefinitionsCollection {
            get => this._dmodDefinitionsCollection;
            private set => this.RaiseAndSetIfChanged(ref this._dmodDefinitionsCollection, value);
        }
        private DataGridCollectionView? _dmodDefinitionsCollection = null;
        
        public bool DmodDefinitionsFilteredHasItems => this._dmodDefinitionsFiltered.Any();
        private List<OnlineDmodInfoViewModel> _lastusedDmodDefinitions = new List<OnlineDmodInfoViewModel>();
        private IEnumerable<OnlineDmodInfoViewModel> _dmodDefinitionsFiltered = new List<OnlineDmodInfoViewModel>();
        
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        private void DmodCrawler_DmodListInitialized(object? sender, EventArgs e) {
            this.InitializeDmods();
        }
        
        /// <summary>
        /// Initializes the DMOD lists for the view from the <see cref="DmodCrawler"/>
        /// </summary>
        private void InitializeDmods() {
            if (this.DmodCrawler == null) {
                this.LastRefreshedString = DateTime.MinValue.ToString("G");
                return;
            }
            
            List<OnlineDmodInfoViewModel> dmodList = new List<OnlineDmodInfoViewModel>();

            foreach (OnlineDmodInfo dmod in this.DmodCrawler.DmodList) {
                dmodList.Add(new OnlineDmodInfoViewModel(dmod));
            }
            
            this.LastRefreshedString = this.DmodCrawler.DmodPagesLastWriteTime.ToString("G");

            this.InitializeFilteredDmods(dmodList);
        }
        
        /// <summary>
        /// Initializes filtered dmods list for the view using current <see cref="DmodDefinitions"/>
        /// </summary>
        private void InitializeFilteredDmods(List<OnlineDmodInfoViewModel> newDmodList) {
            void SetFilteredDmods(IEnumerable<OnlineDmodInfoViewModel> dmods) {
                string oldSelPath = this.SelectedDmodDefinition?.Name ?? string.Empty;

                if (this.DmodDefinitionsCollection != null) {
                    this.DmodDefinitionsCollection.PropertyChanged -= this.DmodDefinitionsCollectionOnPropertyChanged;
                }

                this._dmodDefinitionsFiltered = dmods.AsEnumerable();
                this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));
                
                DataGridCollectionView collectionView = new DataGridCollectionView(this._dmodDefinitionsFiltered);
                collectionView.PropertyChanged += this.DmodDefinitionsCollectionOnPropertyChanged;
                this.DmodDefinitionsCollection = collectionView;

                // restore selected dmod!
                this.SelectDmodByName(oldSelPath);
            }
            
            this._lastusedDmodDefinitions = newDmodList;

            if (this.DmodSearchString != null && this.DmodSearchString.Length >= 2) {
                string searchStr = this.DmodSearchString.ToLowerInvariant();
                
                var filtered = newDmodList.Where( definition => 
                    // DMOD name matches search string, or Author matches search string...
                    definition.Name.ToLowerInvariant().Contains(searchStr) ||
                    definition.Author.ToLowerInvariant().Contains(searchStr) );
                
                // filter definitions
                SetFilteredDmods(filtered);
            }
            else {
                // use all the definitions
                SetFilteredDmods(newDmodList);
            }
        }
        
        private void DmodDefinitionsCollectionOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (sender is IDataGridCollectionView dgcv && e.PropertyName == nameof (DataGridCollectionView.CurrentItem)) {
                this.SelectedDmodDefinition = dgcv.CurrentItem as OnlineDmodInfoViewModel;
            }
        }
        
        private void SelectDmodByName(string dmodName) {
            if (string.IsNullOrWhiteSpace(dmodName))
                return;
            
            foreach (var dmod in this._dmodDefinitionsFiltered) {
                if (dmod.Name.Equals(dmodName)) {
                    Dispatcher.UIThread.InvokeAsync(() => {
                        this.DmodDefinitionsCollection?.MoveCurrentTo(dmod);
                    });
                    return;
                }
            }
        }

        #endregion
      
        #region BUSY / PROGRESS - Properties related to the Online Dmod Browser being busy and such

        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
                
        /// <summary>
        /// Used to indicate that application is busy reloading the DMOD list
        /// </summary>
        public bool IsReloadingDmodList {
            get => this._isReloadingDmodList;
            private set => this.RaiseAndSetIfChanged(ref this._isReloadingDmodList, value);
        }
        private bool _isReloadingDmodList = false;

        public string ProgressMessage {
            get => this._progressMessage;
            private set => this.RaiseAndSetIfChanged(ref this._progressMessage, value);
        }
        private string _progressMessage = "";
        
        public double ProgressBarPercent {
            get => this._progressBarPercent;
            private set => this.RaiseAndSetIfChanged(ref this._progressBarPercent, value);
        }
        private double _progressBarPercent = 0;
        
        public bool ProgressIsVisible {
            get => this._progressIsVisible;
            private set => this.RaiseAndSetIfChanged(ref this._progressIsVisible, value);
        }
        private bool _progressIsVisible = false;
        
        public bool ProgressIsIndeterminate {
            get => this._progressIsIndeterminate;
            private set => this.RaiseAndSetIfChanged(ref this._progressIsIndeterminate, value);
        }
        private bool _progressIsIndeterminate = false;

        #endregion
        

        #region DMOD Selected - Properties and methods related to the currently selected online DMOD are here
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        public string LastRefreshedDmodString {
            get => this._lastRefreshedDmodString;
            private set => this.RaiseAndSetIfChanged(ref this._lastRefreshedDmodString, value);
        }
        private string _lastRefreshedDmodString = "";
        
        public OnlineDmodInfoViewModel? SelectedDmodDefinition {
            get => this._selectedDmodDefinition;
            private set {
                if (this._selectedDmodDefinition != null) {
                    // unload previous view models that are no longer needed...
                    this._selectedDmodDefinition.UnloadOnlineData();
                }
                this.RaiseAndSetIfChanged(ref this._selectedDmodDefinition, value);
                this.RaisePropertyChanged(nameof(this.SelectedDmodScreenshotIsFirst));
                this.RaisePropertyChanged(nameof(this.SelectedDmodScreenshotIsLast));
            }
        }
        private OnlineDmodInfoViewModel? _selectedDmodDefinition;
        
        public OnlineDmodScreenshotViewModel? SelectedDmodScreenshotVm {
            get => this._selectedDmodScreenshotVm;
            set => this.RaiseAndSetIfChanged(ref this._selectedDmodScreenshotVm, value);
        }
        private OnlineDmodScreenshotViewModel? _selectedDmodScreenshotVm;

        public int SelectedDmodScreenshotIndex {
            get => this._selectedDmodScreenshotIndex;
            set {
                this.RaiseAndSetIfChanged(ref this._selectedDmodScreenshotIndex, value);
                this.RaisePropertyChanged(nameof(this.SelectedDmodScreenshotIsFirst));
                this.RaisePropertyChanged(nameof(this.SelectedDmodScreenshotIsLast));
            }
        }
        private int _selectedDmodScreenshotIndex = -1;

        public bool SelectedDmodScreenshotIsFirst => this._selectedDmodDefinition == null ||  this._selectedDmodScreenshotIndex <= 0;
        public bool SelectedDmodScreenshotIsLast => this._selectedDmodDefinition == null || this._selectedDmodScreenshotIndex >= this._selectedDmodDefinition.Screenshots.Count;
        
        
        private Dictionary<string, OnlineUserViewModel> _cachedUserViewModels = new Dictionary<string, OnlineUserViewModel>();
        
        // -----------------------------------------------------------------------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Initialize selected DMOD Definition data from <see cref="DmodCrawler"/>, either using locally cached data or from the web
        /// </summary>
        /// <param name="forceReloadFromWeb">If true, will force attempt to get DMOD data from web</param>
        private async Task SelectedDmodDefinitionInitialize(bool forceReloadFromWeb) {
            if (this.SelectedDmodDefinition != null &&
                this.DmodCrawler != null) {
                try {
                    this.ProgressBarPercent = 0;
                    this.ProgressIsIndeterminate = true;
                    this.ProgressMessage = Localizer.Instance[@"OnlineDmodBrowser/Progress/DownloadingData"];
                    this.ProgressIsVisible = true;
                    this.SelectedDmodScreenshotVm = null;

                    await this.DmodCrawler.UpdateDmodData(this.SelectedDmodDefinition.DmodInfo, forceReloadFromWeb);

                    foreach (OnlineDmodReview rev in this.SelectedDmodDefinition.DmodInfo.DmodReviews) {
                        if (this._cachedUserViewModels.ContainsKey(rev.User.Name) == false) {
                            // user view model was not encountered before...
                            await this.DmodCrawler.CacheUserData(rev.User, false);
                            this._cachedUserViewModels.Add(rev.User.Name, new OnlineUserViewModel(rev.User));
                        }
                    }

                    foreach (OnlineDmodScreenshot scr in this.SelectedDmodDefinition.DmodInfo.DmodScreenshots) {
                        OnlineDmodCachedResource? resPreview = OnlineDmodCachedResource.FromRelativeFileUrl(scr.RelativePreviewUrl);
                        if (resPreview != null && File.Exists(resPreview.Local) == false) {
                            HttpStatusCode result = await this.DmodCrawler.DownloadWebContent(resPreview);
                        }

                        OnlineDmodCachedResource? resScreenshot = OnlineDmodCachedResource.FromRelativeFileUrl(scr.RelativeScreenshotUrl);
                        if (resScreenshot != null && File.Exists(resScreenshot.Local) == false) {
                            HttpStatusCode result = await this.DmodCrawler.DownloadWebContent(resScreenshot);
                        }
                    }

                    this.SelectedDmodDefinition.RefreshOnlineData(this._cachedUserViewModels);

                    if (this.SelectedDmodDefinition.Screenshots.Count > 0) {
                        this.SelectedDmodScreenshotVm = this.SelectedDmodDefinition.Screenshots.First();
                    }

                    string testFile = this.SelectedDmodDefinition.DmodInfo.ResMain.Local;
                    FileInfo finfoTest = new FileInfo(testFile);

                    this.LastRefreshedDmodString = finfoTest.LastWriteTime.ToString("G");

                    this.ProgressIsVisible = false;
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                    this.ProgressIsVisible = false;
                }
            }
        }
        
        /// <summary>
        /// Loads currently selected screenshot from local cached file
        /// </summary>
        private void OnDmodScreenshotVmChanged() {
            try {
                this.SelectedDmodScreenshotVm?.ReloadScreenshotFile(false);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
            }
        }

        #endregion
        
        
        #region COMMANDS

        /// <summary>
        /// Reloads all the online DMODs from the online DMOD pages
        /// </summary>
        /// <param name="parameter">N/A</param>
        public async void CmdRefreshDmods(object? parameter = null) {
            if (this.DmodCrawler == null) return;
            if (this.IsReloadingDmodList) return;

            try {
                this.IsReloadingDmodList = true;
                
                this.SelectedDmodDefinition = null;
                this.SelectedDmodScreenshotVm = null;
                this.DmodSearchString = null;
                await this.DmodCrawler.InitializeDmodLists(true);
                this.InitializeDmods();
            } finally {
                this.IsReloadingDmodList = false;
            }
        }
        [DependsOn(nameof(DmodManager))]
        [DependsOn(nameof(IsReloadingDmodList))]
        public bool CanCmdRefreshDmods(object? parameter = null) {
            if (this.DmodCrawler == null) return false;
            if (this.IsReloadingDmodList) return false;
            return true;
        }
        
        public void CmdClearSelectedDmod(object? parameter = null) {
            if (this.SelectedDmodDefinition == null) { return; }
            this.SelectedDmodDefinition = null;
        }

        [DependsOn(nameof(SelectedDmodDefinition))]
        public bool CanCmdClearSelectedDmod(object? parameter = null) {
            if (this.SelectedDmodDefinition == null) { return false; }
            return true;
        }
        
        
        /// <summary>
        /// Reloads currently selected DMOD data from online page
        /// </summary>
        /// <param name="parameter">N/A</param>
        public async void CmdReloadSelectedDmodFromWeb(object ? parameter = null) {
            if (this.SelectedDmodDefinition == null) return;
            if (this.ProgressIsVisible) return;

            await this.SelectedDmodDefinitionInitialize(true);
        }
        [DependsOn(nameof(SelectedDmodDefinition))]
        [DependsOn(nameof(ProgressIsVisible))]
        public bool CanCmdReloadSelectedDmodFromWeb(object ? parameter = null) {
            if (this.SelectedDmodDefinition == null) return false;
            if (this.ProgressIsVisible) return false;
            return true;
        }

        /// <summary>
        /// Downloads an Online DMOD package and starts installing it when done
        /// </summary>
        /// <param name="parameter"><see cref="OnlineDmodVersionViewModel"/></param>
        public async void CmdInstallDmod(object? parameter = null) {
            if (this.ProgressIsVisible) return;
            if (this.DmodCrawler == null) return;
            if (!(parameter is OnlineDmodVersionViewModel def)) return;

            string url = def.RelativeDownloadUrl;
            OnlineDmodCachedResource? resource = OnlineDmodCachedResource.FromRelativeFileUrl(url);
            if (resource != null) {

                if (File.Exists(resource.Local) == false) {
                    this.ProgressBarPercent = 0;
                    this.ProgressIsIndeterminate = true;
                    this.ProgressMessage = Localizer.Instance[@"OnlineDmodBrowser/Progress/DownloadingData"];
                    this.ProgressIsVisible = true;

                    await this.DmodCrawler.DownloadWebContent(resource);

                    this.ProgressIsVisible = false;
                }

                if (File.Exists(resource.Local)) {
                    try {
                        this.InstallDmodRequested?.Invoke(this, new InstallOnlineDmodEventArgs(resource.Local));
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                    }
                }
            }
        }
        [DependsOn(nameof(ProgressIsVisible))]
        [DependsOn(nameof(DmodCrawler))]
        public bool CanCmdInstallDmod(object? parameter = null) {
            if (this.ProgressIsVisible) { return false;}
            if (this.DmodCrawler == null) { return false;}
            if (parameter is OnlineDmodVersionViewModel) { return true; }
            return false;
        }

        public void CmdGoScreenshotPrevious() {
            if (this.SelectedDmodDefinition == null) return;
            if (this.SelectedDmodScreenshotIndex > 0) {
                if (this.SelectedDmodScreenshotIndex >= this.SelectedDmodDefinition.Screenshots.Count) this.SelectedDmodScreenshotIndex = this.SelectedDmodDefinition.Screenshots.Count - 1;
                else this.SelectedDmodScreenshotIndex--;
            }
        }
        
        public void CmdGoScreenshotNext() {
            if (this.SelectedDmodDefinition == null) return;
            if (this.SelectedDmodScreenshotIndex < this.SelectedDmodDefinition.Screenshots.Count - 1) {
                if (this.SelectedDmodScreenshotIndex < 0) this.SelectedDmodScreenshotIndex = 0;
                else this.SelectedDmodScreenshotIndex++;
            }
        }

        #endregion

        private bool _disposed = false;
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (this._disposed)
                return;

            if (disposing) {
                this.DmodDefinitionsCollection = null;

                foreach (OnlineDmodInfoViewModel x in this._lastusedDmodDefinitions) {
                    x.Dispose();
                }

                foreach (KeyValuePair<string, OnlineUserViewModel> x in this._cachedUserViewModels) {
                    x.Value.Dispose();
                }
                
                this._cachedUserViewModels.Clear();
            }
            
            this._disposed = true;
        }
    }
}
