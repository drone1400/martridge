using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Timers;
using Avalonia.Metadata;
using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.Models.Steam;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public class SettingsExtensionComponentConfigViewModel : ViewModelBase {

        public string ExePathOriginal {
            get => this._exePathOriginal;
            private set => this.RaiseAndSetIfChanged(ref this._exePathOriginal, value);
        }
        private string _exePathOriginal = string.Empty;

        public string ExePath {
            get => this._exePath;
            set => this.RaiseAndSetIfChanged(ref this._exePath, value);
        }
        private string _exePath = string.Empty;

        public uint SteamId32 {
            get => this._steamId32;
            set => this.RaiseAndSetIfChanged(ref this._steamId32, value);
        }
        private uint _steamId32 = 0;

        public bool PreferLaunchingAsSteamApp {
            get => this._preferLaunchingAsSteamApp;
            set => this.RaiseAndSetIfChanged(ref this._preferLaunchingAsSteamApp, value);
        }
        private bool _preferLaunchingAsSteamApp = false;

#if PLATF_LINUX
        public bool ShowWineUi => true;
#else
        public bool ShowWineUi => true; // TODO set this to false later :P
#endif
        
        public string WineVerPath {
            get => this._wineVerPath;
            set => this.RaiseAndSetIfChanged(ref this._wineVerPath, value);
        }
        private string _wineVerPath = string.Empty;
        
        public string WineBinPath {
            get => this._wineBinPath;
            set => this.RaiseAndSetIfChanged(ref this._wineBinPath, value);
        }
        private string _wineBinPath = string.Empty;
        
        public string WineLibPath {
            get => this._wineLibPath;
            set => this.RaiseAndSetIfChanged(ref this._wineLibPath, value);
        }
        private string _wineLibPath = string.Empty;

        public string WineServer {
            get => this._wineServer;
            set => this.RaiseAndSetIfChanged(ref this._wineServer, value);
        }
        private string _wineServer = string.Empty;

        public string WineLoader {
            get => this._wineLoader;
            set => this.RaiseAndSetIfChanged(ref this._wineLoader, value);
        }
        private string _wineLoader =  string.Empty;
        
        public string WineDllPath {
            get => this._wineDllPath;
            set => this.RaiseAndSetIfChanged(ref this._wineDllPath, value);
        }
        private string _wineDllPath = string.Empty;
        
        public string WinePrefix {
            get => this._winePrefix;
            set => this.RaiseAndSetIfChanged(ref this._winePrefix, value);
        }
        private string _winePrefix = string.Empty;


        private Timer _wineVerChangedTimer = new Timer() {
            Interval = 330,
            AutoReset = false,
        };

        public SettingsExtensionComponentConfigViewModel() {
            this.PropertyChanged += this.OnPropertyChanged;
            this._wineVerChangedTimer.Elapsed += this.WineVerChangedTimerOnElapsed;
        }
        private void WineVerChangedTimerOnElapsed(object? sender, ElapsedEventArgs e) {
            this._wineVerChangedTimer.Stop();
            this.TryAutoResolveSomeWinePaths();
        }
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.WineVerPath)) {
                this._wineVerChangedTimer.Stop();
                this._wineVerChangedTimer.Start();
            }
        }

        public void Initialize(string targetPath, ConfigExtensionLinuxWine? configWine, ConfigExtensionSteamInfo? configSteam) {
            this.ExePathOriginal = targetPath;
            this.ExePath = targetPath;
            
            this.SetFromWineData(configWine);
            this.SetFromSteamData(configSteam);
            this.TryAutoResolveSomeWinePaths();
        }

        public ConfigExtensionLinuxWine? GetWineData() {
            if (string.IsNullOrWhiteSpace(this._wineVerPath) &&
                string.IsNullOrWhiteSpace(this._wineBinPath) &&
                string.IsNullOrWhiteSpace(this._wineLibPath) &&
                string.IsNullOrWhiteSpace(this._wineServer) &&
                string.IsNullOrWhiteSpace(this._wineLoader) &&
                string.IsNullOrWhiteSpace(this._wineDllPath) &&
                string.IsNullOrWhiteSpace(this._winePrefix))
                return null;
            
            return new ConfigExtensionLinuxWine(this._wineVerPath, this._wineBinPath, this._wineLibPath, this._wineServer, this._wineLoader, this._wineDllPath, this._winePrefix);
        }

        public ConfigExtensionSteamInfo? GetSteamData() {
            if (this.SteamId32 == 0 && this.PreferLaunchingAsSteamApp == false)
                return null;

            return new ConfigExtensionSteamInfo(this._steamId32, this._preferLaunchingAsSteamApp);
        }

        private void SetFromWineData(ConfigExtensionLinuxWine? config) {
            this.WineVerPath = config?.WINEVERPATH ?? string.Empty;
            this.WineBinPath = config?.WINEBINPATH ?? string.Empty;
            this.WineLibPath = config?.WINELIBPATH ?? string.Empty;
            this.WineServer = config?.WINESERVER ?? string.Empty;
            this.WineLoader = config?.WINELOADER ?? string.Empty;
            this.WineDllPath = config?.WINEDLLPATH ?? string.Empty;
            this.WinePrefix = config?.WINEPREFIX ?? string.Empty;
        }

        private void SetFromSteamData(ConfigExtensionSteamInfo? config) {
            this.SteamId32 = config?.SteamId32 ?? 0;
            this.PreferLaunchingAsSteamApp = config?.PreferLaunchingAsSteamApp ?? false;
        }

        private void TryAutoResolveSomeWinePaths() {
            if (string.IsNullOrWhiteSpace(this._wineVerPath)) return;

            if (string.IsNullOrWhiteSpace(this._wineBinPath)) {
                string possiblePath = Path.Combine(this._wineVerPath, "bin");
                if (Directory.Exists(possiblePath)) this.WineBinPath = possiblePath;
            }
            
            if (string.IsNullOrWhiteSpace(this._wineLibPath)) {
                string possiblePath = Path.Combine(this._wineVerPath, "lib");
                if (Directory.Exists(possiblePath)) this.WineLibPath = possiblePath;
            }
            
            if (string.IsNullOrWhiteSpace(this._wineServer)) {
                string possiblePath = Path.Combine(this._wineVerPath, "bin", "wineserver");
                if (File.Exists(possiblePath)) this.WineServer = possiblePath;
            }
            
            if (string.IsNullOrWhiteSpace(this._wineLoader)) {
                string possiblePath = Path.Combine(this._wineVerPath, "bin", "wine");
                if (File.Exists(possiblePath)) this.WineLoader = possiblePath;
            }

            if (string.IsNullOrWhiteSpace(this._wineDllPath)) {
                string possiblePath = Path.Combine(this._wineVerPath, "lib", "wine");
                if (Directory.Exists(possiblePath)) this.WineDllPath = possiblePath;
            }
            
            // prefix can not be auto resolved here... so ignore that
        }

        public async void CmdAutoDetectSteamId(object? parameter = null) {
            try {
                List<uint> ids = SteamHelper.FindNonSteamGameSteamIds(this.ExePath);
                if (ids.Count > 0) {
                    this.SteamId32 = ids[0];
                }
                else {
                    string title = Localizer.Instance["SettingsExtensionConfigView/SteamAutoIdError/Title"];
                    string body = Localizer.Instance["SettingsExtensionConfigView/SteamAutoIdError/BodyPart1"]
                        + Environment.NewLine + this.ExePath + Environment.NewLine + Environment.NewLine +
                        Localizer.Instance["SettingsExtensionConfigView/SteamAutoIdError/BodyPart2"];
                    await DinkyAlert.ShowDinkyAlert(title, body, AlertResults.Ok, AlertType.Warning);
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        public bool CanCmdAutoDetectSteamId() {
            return true;
        }

        public async void CmdAutoDetectWine(object? parameter = null) {
            bool usingKnownSteamId = false;

            List<uint> ids = new List<uint>();
            
            // try to autodetect known steam id first...
            if (this.SteamId32 != 0) {
                ids.Add(this.SteamId32);
                usingKnownSteamId = true;
            } else {
                ids = SteamHelper.FindNonSteamGameSteamIds(this.ExePath);
            }

            for (int i = 0; i < ids.Count; i++) {
                SteamHelper.FindProtonWine(ids[i], out string wineVerPath, out string winePfxPath, out string protonVersion);

                if (Directory.Exists(wineVerPath) && Directory.Exists(winePfxPath)) {
                    if (usingKnownSteamId == false) {
                        this.SteamId32 = ids[i];
                    }
                    
                    this.WineVerPath = wineVerPath;
                    this.WinePrefix = winePfxPath;

                    this.WineBinPath = "";
                    this.WineLibPath = "";
                    this.WineDllPath = "";
                    this.WineLoader = "";
                    this.WineServer = "";

                    this.TryAutoResolveSomeWinePaths();
                    return;
                }

                if (usingKnownSteamId) {
                    // failed to find using known steam id, try to scan again?
                    ids = SteamHelper.FindNonSteamGameSteamIds(this.ExePath);
                }
            }
        }

        [DependsOn(nameof(SteamId32))]
        public bool CanCmdAutoDetectWine() {
            return this.SteamId32 != 0;
        }

    }
}
