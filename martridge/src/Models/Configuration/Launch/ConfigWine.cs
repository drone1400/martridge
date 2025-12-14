using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Martridge.Models.Configuration.Generic;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Configuration.LaunchExtension.FileData;
using Martridge.Models.Steam;
using Tmds.DBus.Protocol;
namespace Martridge.Models.Configuration.LaunchExtension {
    public class ConfigWine {

        /// <summary>
        /// If this is not set to true, will not use wine for the current game. Settings will be saved but not active.
        /// </summary>
        public bool UseWine => this._useWine;
        private bool _useWine = true;

        /// <summary>
        /// If true, will allow users to completely override the default standard WINE environment variables, otherwise some properties for them will not be available in the UI
        /// </summary>
        public bool OverrideDefaultWineEnvVarDefinitions => this._overrideDefaultWineEnvVarDefinitions;
        private bool _overrideDefaultWineEnvVarDefinitions = false;
        
        public IReadOnlyDictionary<string,ConfigEnvironmentVariable> EnvironmentVariables => this._environmentVariables;
        private Dictionary<string,ConfigEnvironmentVariable> _environmentVariables = new Dictionary<string, ConfigEnvironmentVariable>();
        

        public static List<ConfigDataEnvironmentVariable> GetDefaultWineEnvVars() => new List<ConfigDataEnvironmentVariable>() {
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINEVERPATH, "", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINELOADER, "wine", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINESERVER, "wineserver", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINEDLLPATH, "", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINEPREFIX, "", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.LD_LIBRARY_PATH, "", true, true, false, ":"),
        };
        public static Dictionary<string, ConfigDataEnvironmentVariable> GetDefaultWineEnvVarsDictionary() {
            Dictionary<string, ConfigDataEnvironmentVariable> envVars = new Dictionary<string, ConfigDataEnvironmentVariable>();
            var list = GetDefaultWineEnvVars();
            foreach (var x in list) {
                envVars.Add(x.Key, x);
            }
            return envVars;
        }
        
        public ConfigWine() {
            this._useWine = true;
            this._overrideDefaultWineEnvVarDefinitions = false;

            this._environmentVariables =  new Dictionary<string, ConfigEnvironmentVariable>();

            List<ConfigDataEnvironmentVariable> defaults = ConfigWine.GetDefaultWineEnvVars();
            foreach (ConfigDataEnvironmentVariable x in defaults) {
                ConfigEnvironmentVariable envVar = new ConfigEnvironmentVariable(x);
                this._environmentVariables[envVar.Key] = envVar;
            }
        }
        
        public ConfigWine(ConfigDataWine data) {
            this.SetFromData(data);
        }
        
        public void SetFromData(ConfigDataWine data) {
            this._useWine = data.UseWine ??  true;
            this._overrideDefaultWineEnvVarDefinitions = data.OverrideDefaultWineEnvVarDefinitions ?? false;

            if (this._overrideDefaultWineEnvVarDefinitions) {
                // set properties exactly as they are in the config file
                this._environmentVariables = new Dictionary<string, ConfigEnvironmentVariable>();
                if (data.EnvironmentVariables != null) {
                    foreach (ConfigDataEnvironmentVariable x in data.EnvironmentVariables) {
                        // set from user config
                        this._environmentVariables[x.Key ?? string.Empty] = new ConfigEnvironmentVariable(
                            key: x.Key ?? string.Empty,
                            value: x.Value ?? string.Empty,
                            isEnabled: x.IsEnabled ?? false,
                            isAppendMode: x.IsAppendMode ?? false,
                            isAppendAtEnd: x.IsAppendAtEnd ?? false,
                            appendSeparator: x.AppendSeparator ?? ":",
                            description: x.Description ?? string.Empty);
                    }
                }
            } else {
                // initialize default env vars
                List<ConfigDataEnvironmentVariable> defaultsList = ConfigWine.GetDefaultWineEnvVars();
                Dictionary<string,ConfigEnvironmentVariable> defaults = new Dictionary<string, ConfigEnvironmentVariable>();
                foreach (ConfigDataEnvironmentVariable x in defaultsList) {
                    ConfigEnvironmentVariable envVar = new ConfigEnvironmentVariable(x);
                    this._environmentVariables[envVar.Key] = envVar;
                    defaults[envVar.Key] = envVar;
                }
                
                if (data.EnvironmentVariables != null) {
                    foreach (ConfigDataEnvironmentVariable x in data.EnvironmentVariables) {
                        if (defaults.TryGetValue(x.Key ?? string.Empty, out ConfigEnvironmentVariable? old)) {
                            // override only the value
                            this._environmentVariables[x.Key ?? string.Empty] = new ConfigEnvironmentVariable(
                                key: old.Key,
                                value: x.Value ?? string.Empty,
                                isEnabled: old.IsEnabled,
                                isAppendMode: old.IsAppendMode,
                                isAppendAtEnd: old.IsAppendAtEnd,
                                appendSeparator: old.AppendSeparator,
                                description: old.Description);
                        } else {
                            // add new environment variable
                            this._environmentVariables[x.Key ?? string.Empty] = new ConfigEnvironmentVariable(
                                key: x.Key ?? string.Empty,
                                value: x.Value ?? string.Empty,
                                isEnabled: x.IsEnabled ?? false,
                                isAppendMode: x.IsAppendMode ?? false,
                                isAppendAtEnd: x.IsAppendAtEnd ?? false,
                                appendSeparator: x.AppendSeparator ?? ":",
                                description: x.Description ?? string.Empty);
                        }
                    }
                }
            }
        }

        public ConfigDataWine GetData() {
            
            List<ConfigDataEnvironmentVariable> list = new List<ConfigDataEnvironmentVariable>();

            foreach (var kvp in this._environmentVariables) {
                list.Add(new ConfigDataEnvironmentVariable() {
                    Key = kvp.Key,
                    Value = kvp.Value.Value,
                    IsEnabled = kvp.Value.IsEnabled,
                    IsAppendMode = kvp.Value.IsAppendMode,
                    IsAppendAtEnd = kvp.Value.IsAppendAtEnd,
                    AppendSeparator = kvp.Value.AppendSeparator,
                    Description =  kvp.Value.Description,
                });
            }
            
            return new ConfigDataWine() {
                UseWine = this._useWine,
                OverrideDefaultWineEnvVarDefinitions = this._overrideDefaultWineEnvVarDefinitions,
                EnvironmentVariables = list,
            };
        }

        public static void AutoDetectDefaultWine(out Dictionary<string, ConfigDataEnvironmentVariable>? envVars) {
            envVars = null;

            // try to set from default wine install
            if (File.Exists("/usr/bin/wine")) {
                envVars = GetDefaultWineEnvVarsDictionary();
                
                // set loader binary path
                envVars[EnvironmentVariableHelper.WINELOADER].Value = "/usr/bin/wine";

                // set server binary path if possible
                if (File.Exists("/usr/bin/wineserver")) {
                    envVars[EnvironmentVariableHelper.WINESERVER].Value = "/usr/bin/wineserver";
                }
                
                // set prefix path if possible
                string home = LocationHelper.TryGetHomeDirectory();
                if (string.IsNullOrWhiteSpace(home) == false) {
                    string path = Path.Combine(home, ".wine");
                    if (Directory.Exists(path)) {
                        envVars[EnvironmentVariableHelper.WINEPREFIX].Value = path;
                    }
                }
                
                return;
            }
            
            // try to set from default wine64 install
            if (File.Exists("/usr/bin/wine64")) {
                envVars = GetDefaultWineEnvVarsDictionary();
                
                // set loader binary path
                envVars[EnvironmentVariableHelper.WINELOADER].Value = "/usr/bin/wine64";

                // set server binary path if possible
                if (File.Exists("/usr/bin/wineserver")) {
                    envVars[EnvironmentVariableHelper.WINESERVER].Value = "/usr/bin/wineserver";
                }
                
                // set prefix path if possible
                string home = LocationHelper.TryGetHomeDirectory();
                if (string.IsNullOrWhiteSpace(home) == false) {
                    string path = Path.Combine(home, ".wine64");
                    if (Directory.Exists(path)) {
                        envVars[EnvironmentVariableHelper.WINEPREFIX].Value = path;
                    }
                }
                
                return;
            }
        }
        
        public static uint AutoDetectConfigDataWineFromSteam(string exePath, uint knownSteamId, out Dictionary<string, ConfigDataEnvironmentVariable>? envVars) {
            envVars = null;
            
            bool usingKnownSteamId = false;

            List<uint> ids = new List<uint>();
            
            // try to autodetect known steam id first...
            if (knownSteamId != 0) {
                ids.Add(knownSteamId);
                usingKnownSteamId = true;
            } else {
                ids = SteamHelper.FindNonSteamGameSteamIds(exePath);
            }

            for (int i = 0; i < ids.Count; i++) {
                SteamHelper.FindProtonWine(ids[i], out string wineVerPath, out string winePfxPath, out string protonVersion);
                SteamHelper.AutoDetectWinePathsFromWineVerPath(wineVerPath, out string wineBinPath, out string wineLibPath, out string wineDllPath, out string wineServer, out string wineLoader);

                Dictionary<string, ConfigDataEnvironmentVariable> dictionary = ConfigWine.GetDefaultWineEnvVarsDictionary();

                bool canExit = false;
                
                if (string.IsNullOrWhiteSpace(wineVerPath) == false &&
                    string.IsNullOrWhiteSpace(winePfxPath) == false) {
                    canExit = true;
                    dictionary[EnvironmentVariableHelper.WINEVERPATH].Value = wineVerPath;
                    dictionary[EnvironmentVariableHelper.WINEPREFIX].Value = winePfxPath;
                    dictionary[EnvironmentVariableHelper.WINEDLLPATH].Value = wineDllPath;
                    dictionary[EnvironmentVariableHelper.WINESERVER].Value = wineServer;
                    dictionary[EnvironmentVariableHelper.WINELOADER].Value = wineLoader;
                }

                if (canExit) {
                    return ids[i];
                }


                if (usingKnownSteamId) {
                    // failed to find using known steam id, try to scan again?
                    ids = SteamHelper.FindNonSteamGameSteamIds(exePath);
                    usingKnownSteamId = false;
                }
            }
            
            return 0;
        } 
    }
}
