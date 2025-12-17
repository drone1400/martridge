using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public bool EnableWine => this._enableWine;
        private bool _enableWine = true;

        public IReadOnlyList<ConfigEnvironmentVariable> EnvVars => this._envVars; 
        private List<ConfigEnvironmentVariable> _envVars = new List<ConfigEnvironmentVariable>(); 
        
        public IReadOnlyDictionary<string,ConfigEnvironmentVariable> EnvVarDictionary => this._envVarDictionary;
        private Dictionary<string, ConfigEnvironmentVariable> _envVarDictionary = new Dictionary<string, ConfigEnvironmentVariable>();
        

        public static List<ConfigDataEnvironmentVariable> GetDefaultWineEnvVars() => new List<ConfigDataEnvironmentVariable>() {
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINEVERPATH, "", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINELOADER, "", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINESERVER, "", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINEDLLPATH, "", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.WINEPREFIX, "", true),
            new ConfigDataEnvironmentVariable(EnvironmentVariableHelper.LD_LIBRARY_PATH, "", true, nameof(ConfigEnvVarMode.AppendStart), ":"),
        };
        
        public ConfigWine() {
            this._enableWine = true;
            
            // initialize default env vars
            var defaults = GetDefaultWineEnvVars();
            foreach (var envVarData in defaults) {
                ConfigEnvironmentVariable envVar = new ConfigEnvironmentVariable(envVarData);
                this._envVars.Add(envVar);
                this._envVarDictionary[envVar.Key] = envVar;
            }
        }
        
        public ConfigWine(ConfigDataWine data) {
            this.SetFromData(data);
        }
        
        public void SetFromData(ConfigDataWine data) {
            this._enableWine = data.EnableWine ??  true;

            this._envVars = new List<ConfigEnvironmentVariable>();
            this._envVarDictionary = new Dictionary<string, ConfigEnvironmentVariable>();
            
            // check if we have any environment variable definitions
            if (data.EnvironmentVariables == null)
                return;
                
            List<ConfigDataEnvironmentVariable> values = new List<ConfigDataEnvironmentVariable>();
            Dictionary<string, ConfigDataEnvironmentVariable> valuesDictionary = new Dictionary<string, ConfigDataEnvironmentVariable>();
                
            foreach (ConfigDataEnvironmentVariable x in data.EnvironmentVariables) {
                if (valuesDictionary.TryGetValue(x.Key ?? string.Empty, out ConfigDataEnvironmentVariable? envVar)) {
                    // modify/override existing entry in case of duplicates...
                    envVar.Value = x.Value;
                    envVar.IsEnabled = x.IsEnabled;
                    envVar.Mode = x.Mode;
                    envVar.AppendSeparator = x.AppendSeparator;
                } else {
                    // add new entry
                    values.Add(x);
                    valuesDictionary.Add(x.Key ?? string.Empty, x);
                }
            }

            foreach (var x in values) {
                // add to list and dictionary
                ConfigEnvironmentVariable envVar = new ConfigEnvironmentVariable(x);
                this._envVars.Add(envVar);
                this._envVarDictionary[envVar.Key] = envVar;
            }
        }

        public ConfigDataWine GetData() {
            
            List<ConfigDataEnvironmentVariable> list = new List<ConfigDataEnvironmentVariable>();

            foreach (var x in this._envVars) {
                list.Add(x.GetData());
            }
            
            return new ConfigDataWine() {
                EnableWine = this._enableWine,
                EnvironmentVariables = list,
            };
        }

        public static void AutoDetectDefaultWine(out List<ConfigDataEnvironmentVariable>? envVars) {
            string wineloader = string.Empty;
            string wineserver = string.Empty;
            string wineprefix = string.Empty;
            
            // try to set from default wine install
            if (File.Exists("/usr/bin/wine")) {
                // set loader binary path
                wineloader = "/usr/bin/wine";
                
                // set server binary path if possible
                if (File.Exists("/usr/bin/wineserver")) {
                    wineserver = "/usr/bin/wineserver";
                }
                
                // set prefix path if possible
                string home = LocationHelper.TryGetHomeDirectory();
                if (string.IsNullOrWhiteSpace(home) == false) {
                    string path = Path.Combine(home, ".wine");
                    if (Directory.Exists(path)) {
                        wineprefix = path;
                    }
                }
            } else
            // try to set from default wine64 install
            if (File.Exists("/usr/bin/wine64")) {
                // set loader binary path
                wineloader = "/usr/bin/wine64";

                // set server binary path if possible
                if (File.Exists("/usr/bin/wineserver")) {
                    wineserver = "/usr/bin/wineserver";
                }
                
                // set prefix path if possible
                string home = LocationHelper.TryGetHomeDirectory();
                if (string.IsNullOrWhiteSpace(home) == false) {
                    string path = Path.Combine(home, ".wine64");
                    if (Directory.Exists(path)) {
                        wineprefix = path;
                    }
                }
            }

            List<ConfigDataEnvironmentVariable> defaults = GetDefaultWineEnvVars();
            envVars = new List<ConfigDataEnvironmentVariable>();
            foreach (var x in defaults) {
                switch (x.Key) {
                    case EnvironmentVariableHelper.WINELOADER:
                        x.Value = wineloader; break;
                    case EnvironmentVariableHelper.WINESERVER:
                        x.Value = wineserver; break;
                    case EnvironmentVariableHelper.WINEPREFIX:
                        x.Value = wineprefix; break;
                }
                if (string.IsNullOrWhiteSpace(x.Key) == false) {
                    envVars.Add(x);
                }
            }
        }
        
        public static uint AutoDetectConfigDataWineFromSteam(string exePath, uint knownSteamId, out List<ConfigDataEnvironmentVariable>? envVars) {
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
                SteamHelper.FindProtonWine(ids[i], out string wineVerPath, out string winePrefix, out string protonVersion);
                SteamHelper.AutoDetectWinePathsFromWineVerPath(wineVerPath, out string wineBinPath, out string wineLibPath, out string wineDllPath, out string wineServer, out string wineLoader);

                if (string.IsNullOrWhiteSpace(wineVerPath) == false &&
                    string.IsNullOrWhiteSpace(winePrefix) == false) {
                    
                    List<ConfigDataEnvironmentVariable> defaults = GetDefaultWineEnvVars();
                    envVars = new List<ConfigDataEnvironmentVariable>();
                    foreach (var x in defaults) {
                        switch (x.Key) {
                            case EnvironmentVariableHelper.WINEVERPATH:
                                x.Value = wineVerPath; break;
                            case EnvironmentVariableHelper.WINELOADER:
                                x.Value = wineLoader; break;
                            case EnvironmentVariableHelper.WINESERVER:
                                x.Value = wineServer; break;
                            case EnvironmentVariableHelper.WINEPREFIX:
                                x.Value = winePrefix; break;
                            case EnvironmentVariableHelper.WINEDLLPATH:
                                x.Value = wineDllPath; break;
                        }
                        if (string.IsNullOrWhiteSpace(x.Key) == false) {
                            envVars.Add(x);
                        }
                    }

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
