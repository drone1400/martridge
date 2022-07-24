using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Martridge.Models.Configuration;
using Martridge.Models.Dmod;
using Martridge.Models.Localization;
using Martridge.Models.OnlineDmods;
using Martridge.Trace;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Martridge.ViewModels.Dmod {
    public class OnlineDmodBrowserViewModel : ViewModelBase {

        //
        // Config stuff
        //

        public ConfigGeneral? ConfigurationGeneral {
            get => this._cfgGeneral;
            set {
                if (this._cfgGeneral != null) {
                    this._cfgGeneral.Updated -= this.ConfigurationGeneralUpdated;
                    this._cfgGeneral = null;
                }
                this.RaiseAndSetIfChanged(ref this._cfgGeneral, value);

                if (this._cfgGeneral != null) {
                    this._cfgGeneral.Updated += this.ConfigurationGeneralUpdated;
                    //this.LoadFromConfigGeneral();
                }
            }
        }

        private ConfigGeneral? _cfgGeneral = null;

        public MainWindowViewModel? MainVm {
            get => this._mainVm;
            set => this.RaiseAndSetIfChanged(ref this._mainVm, value);
        }
        private MainWindowViewModel? _mainVm;
        
        //
        // Online dmod stuff
        //

        private Dictionary<string, OnlineUserViewModel> _cachedUserViewModels = new Dictionary<string, OnlineUserViewModel>();

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
        private Timer _dmodSearchTimer;

        public ObservableCollection<OnlineDmodInfoViewModel> DmodDefinitionsFiltered {
            get => this._dmodDefinitionsFiltered;
            set => this.RaiseAndSetIfChanged(ref this._dmodDefinitionsFiltered, value);
        }
        private ObservableCollection<OnlineDmodInfoViewModel> _dmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>();
        public ObservableCollection<OnlineDmodInfoViewModel> DmodDefinitions { get => this._dmodDefinitions; }
        private ObservableCollection<OnlineDmodInfoViewModel> _dmodDefinitions = new ObservableCollection<OnlineDmodInfoViewModel>();

        public OnlineDmodInfoViewModel? SelectedDmodDefinition {
            get => this._selectedDmodDefinition;
            set {
                if (this._selectedDmodDefinition != null) {
                    // unload previous view models that are no longer needed...
                    this._selectedDmodDefinition.UnloadOnlineData();
                }
                this.RaiseAndSetIfChanged(ref this._selectedDmodDefinition, value);
            }
        }
        private OnlineDmodInfoViewModel? _selectedDmodDefinition;
        
        public OnlineDmodScreenshotViewModel? SelectedDmodScreenshotVm {
            get => this._selectedDmodScreenshotVm;
            set => this.RaiseAndSetIfChanged(ref this._selectedDmodScreenshotVm, value);
        }
        private OnlineDmodScreenshotViewModel? _selectedDmodScreenshotVm;

        public Bitmap? SelectedDmodScreenshot {
            get => this._selectedDmodScreenshot;
            set => this.RaiseAndSetIfChanged(ref this._selectedDmodScreenshot, value);
        }
        private Bitmap? _selectedDmodScreenshot = null;

        public bool DmodDefinitionsFilteredHasItems {
            get => this.DmodDefinitionsFiltered.Count > 0;
        }
        
        public ObservableCollection<DmodOrderBy> DmodOrderByList { get => this._dmodOrderByList; }
        private ObservableCollection<DmodOrderBy> _dmodOrderByList = new ObservableCollection<DmodOrderBy>() {
            DmodOrderBy.NameAsc,
            DmodOrderBy.NameDesc,
            DmodOrderBy.ScoreAsc,
            DmodOrderBy.ScoreDesc,
            DmodOrderBy.DownloadsAsc,
            DmodOrderBy.DownloadsDesc,
            DmodOrderBy.UpdatedAsc,
            DmodOrderBy.UpdatedDesc,
            DmodOrderBy.AuthorAsc,
            DmodOrderBy.AuthorDesc,
        };

        public DmodOrderBy DmodOrderBy {
            get => this._dmodOrderBy;
            set => this.RaiseAndSetIfChanged(ref this._dmodOrderBy, value);
        }
        private DmodOrderBy _dmodOrderBy = DmodOrderBy.NameAsc;

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
        
        public bool IsReloadingDmodList {
            get => this._isReloadingDmodList;
            private set => this.RaiseAndSetIfChanged(ref this._isReloadingDmodList, value);
        }
        private bool _isReloadingDmodList = false;
        
        //
        // CONSTRUCTOR
        //

        public OnlineDmodBrowserViewModel() {
            this._dmodSearchTimer = new Timer() {
                Interval = 200,
                AutoReset = true,
            };
            this._dmodSearchTimer.Elapsed += DmodSearchTimerOnElapsed;
            
            this.PropertyChanged += OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.DmodSearchString)) {
                if (this._dmodSearchTimer.Enabled == false) {
                    this._dmodSearchTimer.Start();
                }
            }

            if (e.PropertyName == nameof(this.DmodOrderBy)) {
                this.InitializeDmods();
            }

            if (e.PropertyName == nameof(this.SelectedDmodDefinition)) {
                this.SelectedDmodDefinitionInitialize(false);
            }

            if (e.PropertyName == nameof(this.SelectedDmodScreenshotVm)) {
                this.DmodScreenshotVmChanged();
            }
        }
        private void DmodScreenshotVmChanged() {
            try {
                if (this.SelectedDmodScreenshotVm == null) {
                    this.SelectedDmodScreenshot = null;
                    return;
                }
                
                OnlineDmodCachedResource resScreenshot = OnlineDmodCachedResource.FromRelativeFileUrl(this.SelectedDmodScreenshotVm.DmodScreenshot.RelativeScreenshotUrl);
                if (File.Exists(resScreenshot.Local)) {
                    this.SelectedDmodScreenshot = new Bitmap(resScreenshot.Local);
                } else {
                    this.SelectedDmodScreenshot = null;
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                this.SelectedDmodScreenshot = null;
            }
        }

        private async Task SelectedDmodDefinitionInitialize(bool forceReloadFromWeb) {
            if (this.SelectedDmodDefinition != null) {
                try {
                    this.ProgressBarPercent = 0;
                    this.ProgressIsIndeterminate = true;
                    this.ProgressMessage = Localizer.Instance[@"OnlineDmodBrowser/Progress/DownloadingData"];
                    this.ProgressIsVisible = true;
                    this.SelectedDmodScreenshotVm = null;
                    this.SelectedDmodScreenshot = null;

                    await this.DmodCrawler?.UpdateDmodData(this.SelectedDmodDefinition.DmodInfo, forceReloadFromWeb);

                    foreach (OnlineDmodReview rev in this.SelectedDmodDefinition.DmodInfo.DmodReviews) {
                        if (this._cachedUserViewModels.ContainsKey(rev.User.Name) == false) {
                            // user view model was not encountered before...
                            await this.DmodCrawler.CacheUserData(rev.User, false);
                            this._cachedUserViewModels.Add(rev.User.Name, new OnlineUserViewModel(rev.User));
                        }
                    }

                    foreach (OnlineDmodScreenshot scr in this.SelectedDmodDefinition.DmodInfo.DmodScreenshots) {
                        OnlineDmodCachedResource resPreview = OnlineDmodCachedResource.FromRelativeFileUrl(scr.RelativePreviewUrl);
                        if (File.Exists(resPreview.Local) == false) {
                            HttpStatusCode result = await this.DmodCrawler.DownloadWebContent(resPreview);
                        }

                        OnlineDmodCachedResource resScreenshot = OnlineDmodCachedResource.FromRelativeFileUrl(scr.RelativeScreenshotUrl);
                        if (File.Exists(resScreenshot.Local) == false) {
                            HttpStatusCode result = await this.DmodCrawler.DownloadWebContent(resScreenshot);
                        }
                    }

                    this.SelectedDmodDefinition.RefreshOnlineData(this._cachedUserViewModels);

                    if (this.SelectedDmodDefinition.Screenshots.Count > 0) {
                        this.SelectedDmodScreenshotVm = this.SelectedDmodDefinition.Screenshots.First();
                    }

                    this.ProgressIsVisible = false;
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.Online, ex);
                    this.ProgressIsVisible = false;
                }
            }
        }

        //
        // Configuration stuff
        //
        
        private void DmodCrawler_DmodListInitialized(object? sender, EventArgs e) {
            this.InitializeDmods();
        }

        private void ConfigurationGeneralUpdated(object? sender, EventArgs e) {
        }

      
        //
        // Other stuff
        //

        private void InitializeDmods() {
            this.DmodDefinitions.Clear();
            if (this.DmodCrawler == null) { return; }

            foreach (OnlineDmodInfo dmod in this.DmodCrawler.DmodList) {
                this.DmodDefinitions.Add(new OnlineDmodInfoViewModel(dmod));
            }


            this.InitializeOrderedFilteredDmods(this.DmodDefinitions);
            this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));

            this.SelectedDmodDefinition = null;
        }

        private void DmodSearchTimerOnElapsed(object? sender, ElapsedEventArgs e) {
            this._dmodSearchTimer.Stop();
            this.InitializeFilteredDmods();
        }

        private void InitializeFilteredDmods() {
            if (this.DmodSearchString == null) {
                this.InitializeOrderedFilteredDmods(this.DmodDefinitions);
                this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));
                return;
            }
            
            if (this.DmodSearchString.Length >= 2) {
                string searchStr = this.DmodSearchString.ToLowerInvariant();
                
                var filtered = this._dmodDefinitions.Where( definition => 
                    // DMOD name matches search string, or Author matches search string...
                    definition.Name.ToLowerInvariant().Contains(searchStr) ||
                    definition.Author.ToLowerInvariant().Contains(searchStr) );
                this.InitializeOrderedFilteredDmods(filtered);
                this.RaisePropertyChanged(nameof(this.DmodDefinitionsFilteredHasItems));
            }
        }

        private void InitializeOrderedFilteredDmods(IEnumerable<OnlineDmodInfoViewModel> dmods) {
            switch (this.DmodOrderBy) {
                default: 
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(dmods);
                    break;
                
                case DmodOrderBy.NameAsc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderBy(x => x.Name));
                    break;
                case DmodOrderBy.NameDesc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderByDescending(x => x.Name));
                    break;
                
                case DmodOrderBy.ScoreAsc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderBy(x => x.DmodInfo.Score));
                    break;
                case DmodOrderBy.ScoreDesc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderByDescending(x => x.DmodInfo.Score));
                    break;
                
                case DmodOrderBy.DownloadsAsc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderBy(x => x.Downloads));
                    break;
                case DmodOrderBy.DownloadsDesc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderByDescending(x => x.Downloads));
                    break;
                
                case DmodOrderBy.AuthorAsc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderBy(x => x.Author));
                    break;
                case DmodOrderBy.AuthorDesc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderByDescending(x => x.Author));
                    break;
                
                case DmodOrderBy.UpdatedAsc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderBy(x => x.DmodInfo.Updated));
                    break;
                case DmodOrderBy.UpdatedDesc:
                    this.DmodDefinitionsFiltered = new ObservableCollection<OnlineDmodInfoViewModel>(
                        dmods.OrderByDescending(x => x.DmodInfo.Updated));
                    break;
            }
            
        }

        public async void CmdRefreshDmods(object? parameter = null) {
            if (this.DmodCrawler == null) return;
            if (this.IsReloadingDmodList) return;
            this.IsReloadingDmodList = true;
            this.SelectedDmodDefinition = null;
            this.SelectedDmodScreenshotVm = null;
            this.SelectedDmodScreenshot = null;
            this.DmodSearchString = null;
            await this.DmodCrawler.InitializeDmodLists(true);
            this.InitializeDmods();
            this.IsReloadingDmodList = false;
        }

        [DependsOn(nameof(DmodManager))]
        [DependsOn(nameof(IsReloadingDmodList))]
        public bool CanCmdRefreshDmods(object? parameter = null) {
            if (this.DmodCrawler == null) return false;
            if (this.IsReloadingDmodList) return false;
            return true;
        }

        public async void CmdInstallDmod(object? parameter = null) {
            if (this.ProgressIsVisible) return;
            if (this.MainVm == null) return;
            if (this.DmodCrawler == null) return;
            if (!(parameter is OnlineDmodVersionViewModel def)) return;

            string url = def.DmodVersion.RelativeDownloadUrl;
            OnlineDmodCachedResource resource = OnlineDmodCachedResource.FromRelativeFileUrl(url);
            if (File.Exists(resource.Local) == false) {
                this.ProgressBarPercent = 0;
                this.ProgressIsIndeterminate = true;
                this.ProgressMessage = Localizer.Instance[@"OnlineDmodBrowser/Progress/DownloadingData"];
                this.ProgressIsVisible = true;

                await this.DmodCrawler.DownloadWebContent(resource);

                this.ProgressIsVisible = false;
            }

            if (File.Exists(resource.Local)) {
                this.MainVm.VmDmodInstaller.SelectedDmodPacakge = resource.Local;
                this.MainVm.CmdShowPageDmodInstaller();
            }
        }
        
        [DependsOn(nameof(ProgressIsVisible))]
        [DependsOn(nameof(MainVm))]
        [DependsOn(nameof(DmodCrawler))]
        public bool CanCmdInstallDmod(object? parameter = null) {
            if (this.ProgressIsVisible) { return false;}
            if (this.MainVm == null) { return false;}
            if (this.DmodCrawler == null) { return false;}
            if (parameter is OnlineDmodVersionViewModel) { return true; }
            return false;
        }

        public void CmdReloadSelectedDmodFromWeb(object ? parameter = null) {
            if (this.SelectedDmodDefinition == null) return;
            if (this.ProgressIsVisible) return;

            this.SelectedDmodDefinitionInitialize(true);
        }
        
        [DependsOn(nameof(SelectedDmodDefinition))]
        [DependsOn(nameof(ProgressIsVisible))]
        public bool CanCmdReloadSelectedDmodFromWeb(object ? parameter = null) {
            if (this.SelectedDmodDefinition == null) return false;
            if (this.ProgressIsVisible) return false;
            return true;
        }
        
    }
}
