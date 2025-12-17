using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Martridge.Models.Configuration.AppState;
using Martridge.Models.Configuration.AppState.FileData;
using Martridge.Models.Configuration.General;
using Martridge.Models.Configuration.General.FileData;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Configuration.LaunchExtension;
using Martridge.Models.Configuration.LaunchExtension.FileData;
using Martridge.Models.Localization;
using Martridge.Trace;

namespace Martridge.Models.Configuration {
    public class Config {
        #region globals

#if PLATF_WINDOWS
        public static bool PlatformSupportsWine => false;
#else
        public static bool PlatformSupportsWine => true;
#endif
        public static Config Instance { get; } = new Config();
        public static void InitializeConfiguration()
        {
            Instance.FileNameGeneralConfig = Path.Combine(LocationHelper.GetPathConfig(), "config.json");
            Instance.FileNameExtensionConfig = Path.Combine(LocationHelper.GetPathConfig(), "configExeExtension.json");
            Instance.FileNameAppState = Path.Combine(LocationHelper.GetPathMartridgeState(), "app-state.json");
            Instance.LoadGeneralConfig();
            Instance.LoadConfigExtension();
            Instance.LoadAppState();

            TryAddDefaultDmodPaths();

            // if we have no known dinks, try scanning all known paths!
            if (Instance.General.GameExePaths.Count == 0) {
                TryAddDefaultKnownDinks();
            }
            
            Instance.General.Updated += GeneralOnUpdated;
            Instance.Launch.Updated += LaunchOnUpdated;
        }

        private static void TryAddDefaultDmodPaths() {
            try {
                if (Instance.General.DmodPaths.Count > 0)
                    return;
                
                string myPath = LocationHelper.GetPathMartridge();
                if (Directory.Exists(myPath) == false)
                    return;
            
                string defaultDmods = Path.Combine(myPath, "dmods");
                Directory.CreateDirectory(defaultDmods);

                Instance.General.UpdateProperties(new Dictionary<string, object?>() {
                    [nameof(ConfigDataGeneralV2.DmodPaths)] = new List<string>() {
                        defaultDmods,
                    }
                });
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
        
        private static void LaunchOnUpdated(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationChanged"]);
            Instance.SaveGeneralConfig();
        }

        private static void GeneralOnUpdated(object? sender, ConfigUpdateEventArgs e) {
            MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationChanged"]);
            Instance.SaveGeneralConfig();
        }

        public static void TryAddDefaultKnownDinks() {
            TryAddDefaultFreeDink();
            TryAddDefaultDinkClassic();
            TryAddDefaultDinkHd();
        }
        
        public static void TryAddDefaultDinkClassic() {
#if PLATF_WINDOWS
            string dinkModernWindowsLocalMachine = WindowsHelper.ReadRegistryKeyLocalMachine(@"SOFTWARE\WOW6432Node\RTSOFT\Dink Smallwood", "path")?.ToString() ??  string.Empty;
            TryAddDinkPaths(dinkModernWindowsLocalMachine);
            
            string dinkCurrentUser = WindowsHelper.ReadRegistryKeyCurrentUser(@"SOFTWARE\RTSOFT\Dink Smallwood", "path")?.ToString() ?? string.Empty;
            TryAddDinkPaths(dinkCurrentUser);
            
            string dinkLocalMachine = WindowsHelper.ReadRegistryKeyLocalMachine(@"SOFTWARE\RTSOFT\Dink Smallwood", "path")?.ToString() ?? string.Empty;
            TryAddDinkPaths(dinkLocalMachine);
#endif
        }



        public static void TryAddDefaultDinkHd() {
#if PLATF_WINDOWS
            string windowsDinkHdCurrentUser = WindowsHelper.ReadRegistryKeyCurrentUser(@"SOFTWARE\RTSOFT\DINK", "path")?.ToString() ?? string.Empty;
            TryAddDinkPaths(windowsDinkHdCurrentUser);
            
            string windowsDinkHdLocalMachine = WindowsHelper.ReadRegistryKeyLocalMachine(@"SOFTWARE\RTSOFT\DINK", "path")?.ToString() ?? string.Empty;
            TryAddDinkPaths(windowsDinkHdLocalMachine);
#endif
        }

        
        
        private static void TryAddDinkPaths(string dinkRootDir) {
            // NOTE: this technically works for both DinkHD and modern dink...
            
            if (string.IsNullOrWhiteSpace(dinkRootDir))
                return;
            if (Directory.Exists(dinkRootDir) == false)
                return;

            Config.Instance.General.TryAddAdditionalDmodPath(dinkRootDir);

            string dmods = Path.Combine(dinkRootDir, "dmods");
            string dink = Path.Combine(dinkRootDir, "dink.exe");
            string dinkedit = Path.Combine(dinkRootDir, "dinkedit.exe");

            if (Directory.Exists(dmods)) {
                Config.Instance.General.TryAddAdditionalDmodPath(dmods);
            }
            
            if (File.Exists(dink)) {
                Config.Instance.General.TryAddGameExePath(dink);
            }
            
            if (File.Exists(dinkedit)) {
                Config.Instance.General.TryAddEditorExePath(dinkedit);
            }
        }
        
        public static void TryAddDefaultFreeDink()
        {
#if PLATF_LINUX
            // known linux location for freedink... might need updating for different distros :/

            string defaultLinuxFreedinkExe = "/usr/games/freedink";
            string defaultLinuxDinkGameData = "/usr/share/games/dink";
            string? defaultLinuxHome = Environment.GetEnvironmentVariable("HOME");
            string defaultLinuxDmods = Path.Combine(defaultLinuxHome ?? "", "dmods");

            if (File.Exists(defaultLinuxFreedinkExe)) {
                Config.Instance.General.TryAddGameExePath(defaultLinuxFreedinkExe);
            }

            if (Directory.Exists(defaultLinuxDinkGameData)) {
                Config.Instance.General.TryAddAdditionalDmodPath(defaultLinuxDinkGameData);
            }

            if (defaultLinuxHome != null && Directory.Exists(defaultLinuxDmods)) {
                Config.Instance.General.TryAddAdditionalDmodPath(defaultLinuxDmods);
            }
#endif
            
#if PLATF_WINDOWS
            {
                string windowsFreedinkCurrentUser = WindowsHelper.ReadRegistryKeyCurrentUser(@"SOFTWARE\FreeDink", "")?.ToString() ?? string.Empty;
                TryAddFreeDinkWindowsPath(windowsFreedinkCurrentUser);

                string windowsFreedinkLocalMachine = WindowsHelper.ReadRegistryKeyLocalMachine(@"SOFTWARE\FreeDink", "")?.ToString() ?? string.Empty;
                TryAddFreeDinkWindowsPath(windowsFreedinkLocalMachine);
            }
#endif
        }

        private static void TryAddFreeDinkWindowsPath(string freeDinkRootDir) {
            if (string.IsNullOrWhiteSpace(freeDinkRootDir))
                return;
            if (Directory.Exists(freeDinkRootDir) == false)
                return;

            Config.Instance.General.TryAddAdditionalDmodPath(freeDinkRootDir);
            
            string freedink = Path.Combine(freeDinkRootDir, "freedink.exe");
            string freedinkedit = Path.Combine(freeDinkRootDir, "freedinkedit.exe");
            
            if (File.Exists(freedink)) {
                Config.Instance.General.TryAddGameExePath(freedink);
            }
            
            if (File.Exists(freedinkedit)) {
                Config.Instance.General.TryAddEditorExePath(freedinkedit);
            }
        }

        
        #endregion

        
        #region config data
        public ConfigGeneral General { get; } = new ConfigGeneral();
        public ConfigAppState AppState { get; } = new ConfigAppState();
        public ConfigLaunch Launch { get; } = new ConfigLaunch();
        public ConfigWine? WineGlobal { get; private set; } = new ConfigWine();
        public ConfigExtension LaunchExtension { get; } = new ConfigExtension();
                
        public string FileNameGeneralConfig { get; private set; } = string.Empty;
        public string FileNameAppState { get; private set; } = string.Empty;
        public string FileNameExtensionConfig { get; private set; } = string.Empty;
        
        #endregion

        #region save/load
        
        public void SaveGeneralConfig() {
            if (string.IsNullOrWhiteSpace(this.FileNameGeneralConfig))
                return;

            ConfigFileDataGeneralV2 data = new ConfigFileDataGeneralV2() {
                General = this.General.GetData(),
                Launch = this.Launch.GetData(),
                WineGlobal = this.WineGlobal?.GetData(),
            };
            
            ConfigJsonSerializer.SaveToFile(data, this.FileNameGeneralConfig);
        }

        public void LoadGeneralConfig() {
            if (string.IsNullOrWhiteSpace(this.FileNameGeneralConfig))
                return;
            
            ConfigFileGenericVersion? version = ConfigJsonSerializer.LoadFromFile<ConfigFileGenericVersion>(this.FileNameGeneralConfig);

            if (version?.ConfigDataVersion == null ||
                string.CompareOrdinal(version.ConfigDataVersion, "V2") < 0) {
                // load from old version
                ConfigFileDataGeneralV1? data = ConfigJsonSerializer.LoadFromFile<ConfigFileDataGeneralV1>(this.FileNameGeneralConfig);
                
                if (data?.General != null) {
                    this.General.UpdateProperties(data.General.GetValues());
                }

                if (data?.Launch != null) {
                    this.Launch.UpdateProperties(data.Launch.GetValues());
                }

                if (data?.WineGlobal != null) {
                    this.WineGlobal = new ConfigWine();
                    this.WineGlobal.SetFromData(data.WineGlobal);
                }
            }
            else {
                // load from current version
                ConfigFileDataGeneralV2? data = ConfigJsonSerializer.LoadFromFile<ConfigFileDataGeneralV2>(this.FileNameGeneralConfig);
                
                if (data?.General != null) {
                    this.General.UpdateProperties(data.General.GetValues());
                }

                if (data?.Launch != null) {
                    this.Launch.UpdateProperties(data.Launch.GetValues());
                }

                if (data?.WineGlobal != null) {
                    this.WineGlobal = new ConfigWine();
                    this.WineGlobal.SetFromData(data.WineGlobal);
                }
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
            
            ConfigFileDataExtensionV1 extensionV1 = this.LaunchExtension.GetData();
            if (extensionV1.ExtensionDefinitions?.Count > 0) {
                ConfigJsonSerializer.SaveToFile(extensionV1, this.FileNameExtensionConfig);
            }
        }

        public void LoadConfigExtension() {
            if (string.IsNullOrWhiteSpace(this.FileNameExtensionConfig))
                return;
            
            ConfigFileDataExtensionV1? data  = ConfigJsonSerializer.LoadFromFile<ConfigFileDataExtensionV1>(this.FileNameExtensionConfig);
            this.LaunchExtension.SetFromData(data);
        }
        
        #endregion
    }
}
