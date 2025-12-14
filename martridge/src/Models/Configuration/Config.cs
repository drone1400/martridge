using System;
using System.IO;
using System.Runtime.CompilerServices;
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

            // if we have no known dinks, try scanning all known paths!
            if (Instance.General.GameExePaths.Count == 0) {
                TryAddDefaultKnownDinks();
            }
            
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
        public ConfigWine GlobalWine { get; } = new ConfigWine();
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
