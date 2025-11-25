using System;
using System.IO;
using Martridge.Models.Configuration.AppState;
using Martridge.Models.Configuration.AppState.FileData;
using Martridge.Models.Configuration.General;
using Martridge.Models.Configuration.General.FileData;
using Martridge.Models.Configuration.LaunchExtension;
using Martridge.Models.Configuration.LaunchExtension.FileData;
using Martridge.Models.Localization;
using Martridge.Trace;

namespace Martridge.Models.Configuration {
    public class Config {
        #region globals 
        public static Config Instance { get; } = new Config();
        public static void InitializeConfiguration()
        {
            Instance.FileNameGeneralConfig = Path.Combine(LocationHelper.GetPathConfig(), "config.json");
            Instance.FileNameExtensionConfig = Path.Combine(LocationHelper.GetPathConfig(), "configExeExtension.json");
            Instance.FileNameAppState = Path.Combine(LocationHelper.GetPathMartridgeState(), "app-state.json");
            Instance.LoadGeneralConfig();
            Instance.LoadConfigExtension();
            Instance.LoadAppState();
            
            #if PLATF_LINUX
            // TODO fix this...
            this.AddDefaultLinuxFreeDinkLocations();
            #endif
            
            Instance.General.Updated += GeneralOnUpdated;
            Instance.Launch.Updated += LaunchOnUpdated;
        }
        
        private static void LaunchOnUpdated(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationChanged"]);
            Instance.SaveGeneralConfig();
        }

        private static void GeneralOnUpdated(object? sender, ConfigUpdateEventArgs e) {
            MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationChanged"]);
            Instance.SaveGeneralConfig();
        }
        
               
        
        
        private static void AddDefaultLinuxFreeDinkLocations()
        {
            // TODO fix this...
            string defaultLinuxFreedinkExe = "/usr/games/freedink";
            string defaultLinuxDinkGameData = "/usr/share/games/dink";
            string? defaultLinuxHome = Environment.GetEnvironmentVariable("HOME");
            string defaultLinuxDmods = Path.Combine(defaultLinuxHome ?? "", "dmods");

            if (File.Exists(defaultLinuxFreedinkExe) && 
                Config.Instance.General.GameExePaths.Contains(defaultLinuxFreedinkExe) == false) {
                Config.Instance.General.TryAddGameExePath(defaultLinuxFreedinkExe);
            }

            if (Directory.Exists(defaultLinuxDinkGameData) &&
                Config.Instance.General.AdditionalDmodLocations.Contains(defaultLinuxDinkGameData) == false) {
                Config.Instance.General.TryAddAdditionalDmodPath(defaultLinuxDinkGameData);
            }

            if (defaultLinuxHome != null &&
                Directory.Exists(defaultLinuxDmods) &&
                Config.Instance.General.AdditionalDmodLocations.Contains(defaultLinuxDmods) == false) {
                Config.Instance.General.TryAddAdditionalDmodPath(defaultLinuxDmods);
            }
        }

        
        #endregion

        
        #region config data
        public ConfigGeneral General { get; } = new ConfigGeneral();
        public ConfigAppState AppState { get; } = new ConfigAppState();
        public ConfigLaunch Launch { get; } = new ConfigLaunch();
        public ConfigExtension LaunchExtension { get; } = new ConfigExtension();
                
        public string FileNameGeneralConfig { get; private set; } = string.Empty;
        public string FileNameAppState { get; private set; } = string.Empty;
        public string FileNameExtensionConfig { get; private set; } = string.Empty;
        
        #endregion

        #region save/load
        
        public void SaveGeneralConfig() {
            if (string.IsNullOrWhiteSpace(this.FileNameGeneralConfig))
                return;
            
            ConfigJsonSerializer.SaveToFile(new ConfigFileDataGeneral(
                    this.General.GetData(),
                    this.Launch.GetData()),
                this.FileNameGeneralConfig);
        }

        public void LoadGeneralConfig() {
            if (string.IsNullOrWhiteSpace(this.FileNameGeneralConfig))
                return;
            
            ConfigFileDataGeneral? data = ConfigJsonSerializer.LoadFromFile<ConfigFileDataGeneral>(this.FileNameGeneralConfig);
            
            if (data?.General != null) {
                this.General.UpdateProperties(data.General.GetValues());
            }

            if (data?.Launch != null) {
                this.Launch.UpdateProperties(data.Launch.GetValues());
            }
        }
        
        public void SaveAppState() {
            if (string.IsNullOrWhiteSpace(this.FileNameAppState))
                return;
            
            ConfigJsonSerializer.SaveToFile(new ConfigFileDataAppState(
                    this.AppState.GetData()), 
                this.FileNameAppState);
        }
        
        public void LoadAppState() {
            if (string.IsNullOrWhiteSpace(this.FileNameAppState))
                return;
            
            ConfigFileDataAppState? state = ConfigJsonSerializer.LoadFromFile<ConfigFileDataAppState>(this.FileNameAppState);
            
            if (state?.AppState != null) {
                this.AppState.UpdateProperties(state.AppState.GetValues());
            }
        }
        
        public void SaveConfigExtension() {
            if (string.IsNullOrWhiteSpace(this.FileNameExtensionConfig))
                return;
            
            ConfigFileDataExtension extension = this.LaunchExtension.GetData();
            if (extension.ExtensionDefinitions?.Count > 0) {
                ConfigJsonSerializer.SaveToFile(extension, this.FileNameExtensionConfig);
            }
        }

        public void LoadConfigExtension() {
            if (string.IsNullOrWhiteSpace(this.FileNameExtensionConfig))
                return;
            
            ConfigFileDataExtension? data  = ConfigJsonSerializer.LoadFromFile<ConfigFileDataExtension>(this.FileNameExtensionConfig);
            this.LaunchExtension.SetFromData(data);
        }
        
        #endregion
    }
}
