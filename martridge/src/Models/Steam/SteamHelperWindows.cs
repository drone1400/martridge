using System.IO;
namespace Martridge.Models.Steam {
    public static partial class SteamHelper {
        private static string FindSteamApps() {
            string steamPath = FindSteamInstallation();
            if (string.IsNullOrWhiteSpace(steamPath)) 
                return string.Empty;
                
            return Path.Combine(steamPath, "steamapps");
        }

        private static string FindSteamUserData() {
            string steamPath = FindSteamInstallation();
            if (string.IsNullOrWhiteSpace(steamPath)) 
                return string.Empty;
                
            return Path.Combine(steamPath, "userdata");
        }
        private static string FindSteamInstallation() {
            object? steamPath = WindowsHelper.ReadRegistryKeyLocalMachine(@"SOFTWARE\Valve\Steam", "SteamPath");
            if (steamPath is string pathHklm && Directory.Exists(pathHklm)) return pathHklm;
            
            steamPath = WindowsHelper.ReadRegistryKeyCurrentUser(@"SOFTWARE\Valve\Steam", "SteamPath");
            if (steamPath is string pathHkcu && Directory.Exists(pathHkcu)) return pathHkcu;
            
            // have some hardcoded fallbacks in case the registry is fucked? would steam even keep working if the registry entry is toast?
            
            string fallback1 = @"C:\Program Files\Steam\";
            if (Directory.Exists(fallback1)) return fallback1;
            
            string fallback2 = @"C:\Program Files (x86)\Steam\";
            if (Directory.Exists(fallback2)) return fallback2;
            // if we got here, path could not be found
            return string.Empty;
        }
    }
}
