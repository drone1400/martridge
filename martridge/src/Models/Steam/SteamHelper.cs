using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Martridge.Models.Localization;
using Martridge.Trace;
namespace Martridge.Models.Steam {
    public static partial class SteamHelper {
        
        public static List<uint> FindNonSteamGameSteamIds(string filePath) {
            List<uint> appIds = new List<uint>();
            
            try {
                string userDataPath = FindSteamUserData();
                if (Directory.Exists(userDataPath) == false) {
                    MyTrace.Global.WriteMessage(Localizer.Instance["SteamHelper/FindNonSteamGameSteamIds/CouldNotFindUserData"]);
                    return appIds;
                }

                string firstUserPath = Directory.EnumerateDirectories(userDataPath).First();
                
                // TODO... what do we do if the user has multiple steam users logged in???
                // should I enumerate through all?
                
                string shortcutsVdfPath =  Path.Combine(firstUserPath, "config", "shortcuts.vdf");

                ShortcutsVdf shortcuts = ShortcutsVdf.FromFile(shortcutsVdfPath);
                List<VdfObject> shortcutObjects = shortcuts.FindAllShortcutsByExe(filePath);
                if (shortcutObjects.Count == 0) {
                    string strFormat = Localizer.Instance["SteamHelper/FindNonSteamGameSteamIds/CouldNotFindShortcuts"];
                    MyTrace.Global.WriteMessage(String.Format(strFormat, filePath));
                    return appIds;
                }
                else {
                    string strFormat = Localizer.Instance["SteamHelper/FindNonSteamGameSteamIds/FoundSteamShortcuts"];
                    MyTrace.Global.WriteMessage(String.Format(strFormat, shortcutObjects.Count, filePath));
                }

                
                foreach (VdfObject obj in shortcutObjects) {
                    uint appId = (uint)ShortcutsVdf.GetAppId(obj);
                    appIds.Add(appId);
                    string strFormat = Localizer.Instance["SteamHelper/FindNonSteamGameSteamIds/FoundAppId"];
                    MyTrace.Global.WriteMessage(String.Format(strFormat, appId));
                }
                
                return appIds;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
                return appIds;
            }
        }

        public static void FindProtonWine(uint nonSteamAppId, out string wineVerPath, out string winePfxPath, out string protonVersion) {
                wineVerPath = string.Empty;
                winePfxPath = string.Empty;
                protonVersion = string.Empty;
                try {
                    string steamApps = FindSteamApps();

                    if (string.IsNullOrWhiteSpace(steamApps)) {
                        MyTrace.Global.WriteMessage(Localizer.Instance["SteamHelper/FindProtonWine/CouldNotFindSteamApps"]);
                        return;
                    }

                    string pathCompatData = Path.Combine(steamApps, "compatdata", nonSteamAppId.ToString(CultureInfo.InvariantCulture));
                    string pfxPath = Path.Combine(pathCompatData, "pfx");
                    string configPath = Path.Combine(pathCompatData, "config_info");

                    // check the pfx folder and version info exist
                    if (Directory.Exists(pfxPath) == false || File.Exists(configPath) == false) {
                        MyTrace.Global.WriteMessage(Localizer.Instance["SteamHelper/FindProtonWine/CouldNotFindCompatData"]);
                        return;
                    }

                    string[] protonConfigs = File.ReadAllLines(configPath);
                    // 1st line should be proton version
                    // 2nd line is the proton /share/fonts/ dir
                    // 3rd line is the proton /lib/ dir
                    // 4th line is the proton /lib64/ dir [NOTE: sometimes this line is missing for 32 bit apps]
                    // 5th line is steam dir
                    // 6th, 7th, 8th lines are float values
                    // 9th line is the proton /share/default_pfx/ dir
                    // 10th line is another float
                    // 11th and 12th lines are bools
                    // 13th line is a list of dlls to use
                    // 14th line is another bool
                    // 15th line is sometimes another bool [NOTE: usually is missing?]

                    string winePfxPathTemp = pfxPath;
                    string protonVersionTemp = protonConfigs[0];
                    string libDir = protonConfigs[2];
                    DirectoryInfo libDirInfo = new DirectoryInfo(libDir);
                    string wineVerPathTemp = libDirInfo.Parent?.FullName ?? string.Empty;

                    if (Directory.Exists(wineVerPathTemp) && Directory.Exists(winePfxPathTemp)) {
                        wineVerPath = wineVerPathTemp;
                        winePfxPath = winePfxPathTemp;
                        protonVersion = protonVersionTemp;
                        
                        MyTrace.Global.WriteMessage(Localizer.Instance["SteamHelper/FindProtonWine/FoundProtonVersion"] + " " + protonVersion);
                        MyTrace.Global.WriteMessage(Localizer.Instance["SteamHelper/FindProtonWine/FoundProtonWinePfx"] + " " + winePfxPath);
                        MyTrace.Global.WriteMessage(Localizer.Instance["SteamHelper/FindProtonWine/FoundProtonWineDir"] + " " + wineVerPath);
                    }
                } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        public static void AutoDetectWinePaths(string wineVerPath, out string wineBinPath, out string wineLibPath, out string wineDllPath, out string wineServer, out string wineLoader) {
            wineBinPath = string.Empty;
            wineLibPath = string.Empty;
            wineServer = string.Empty;
            wineLoader = string.Empty;
            wineDllPath = string.Empty;
            
            if (string.IsNullOrWhiteSpace(wineVerPath)) return;

            string wineBinPathTemp = Path.Combine(wineVerPath, "bin");
            if (Directory.Exists(wineBinPathTemp)) wineBinPath = wineBinPathTemp;
            
            string wineLibPathTemp = Path.Combine(wineVerPath, "lib");
            if (Directory.Exists(wineLibPathTemp)) wineLibPath = wineLibPathTemp;
            
            string wineDllTemp = Path.Combine(wineVerPath, "lib", "wine");
            if (Directory.Exists(wineDllTemp)) wineDllPath = wineDllTemp;
            
            string wineServerTemp = Path.Combine(wineVerPath, "bin", "wineserver");
            if (File.Exists(wineServerTemp)) wineServer = wineServerTemp;
            
            string wineLoaderTemp = Path.Combine(wineVerPath, "bin", "wine");
            if (File.Exists(wineLoaderTemp)) wineLoader = wineLoaderTemp;
        }
    }
}
