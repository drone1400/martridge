using System;
using System.Collections.Generic;
using Avalonia.Metadata;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Configuration.LaunchExtension;
using Martridge.Models.Configuration.LaunchExtension.FileData;
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

#if PLATF_WINDOWS
        public bool ShowWineUi => true;
#else
        public bool ShowWineUi => true;
#endif

        public SettingsWineViewModel WineViewModel { get; } = new SettingsWineViewModel();

        public SettingsExtensionComponentConfigViewModel() {
            this.WineViewModel.AutoDetectWineCommand = ReactiveCommand.Create(this.CmdAutoDetectWineInternal);
        }


        public void Initialize(string targetPath, ConfigWine? configWine, ConfigExtensionSteamInfo? configSteam) {
            this.ExePathOriginal = targetPath;
            this.ExePath = targetPath;
            
            this.SetFromSteamData(configSteam);
            this.SetFromWineData(configWine);
        }

        public ConfigDataWine GetWineData() {
            return this.WineViewModel.GetConfigData();
        }

        public ConfigExtensionSteamInfo? GetSteamData() {
            if (this.SteamId32 == 0 && this.PreferLaunchingAsSteamApp == false)
                return null;

            return new ConfigExtensionSteamInfo(this._steamId32, this._preferLaunchingAsSteamApp);
        }

        private void SetFromWineData(ConfigWine? config) {
            if (config == null) return;
            this.WineViewModel.InitializeFromConfig(config);
        }
        
        private void SetFromWineData(ConfigDataWine? config) {
            if (config == null) return;
            this.WineViewModel.InitializeFromConfig(config);
        }

        private void SetFromSteamData(ConfigExtensionSteamInfo? config) {
            this.SteamId32 = config?.SteamId32 ?? 0;
            this.PreferLaunchingAsSteamApp = config?.PreferLaunchingAsSteamApp ?? false;
        }

        public async void CmdAutoDetectSteamId(object? parameter = null) {
            try {
                List<uint> ids = SteamHelper.FindNonSteamGameSteamIds(this.ExePath);
                if (ids.Count > 0) {
                    this.SteamId32 = ids[0];
                } else {
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

        private void CmdAutoDetectWineInternal() {
            uint idResult = ConfigWine.AutoDetectConfigDataWineFromSteam(this.ExePath, this.SteamId32, out List<ConfigDataEnvironmentVariable>? envVars);

            if (idResult != 0 && envVars != null) {
                this.SteamId32 = idResult;
                this.WineViewModel.CopyValuesFrom(envVars);
            } else {
                ConfigWine.AutoDetectDefaultWine(out List<ConfigDataEnvironmentVariable>? envVars2);
                if (envVars2 != null) {
                    this.WineViewModel.CopyValuesFrom(envVars2);
                }
            }
        }
    }
}
