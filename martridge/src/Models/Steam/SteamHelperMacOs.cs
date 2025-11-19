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
            string home = LocationHelper.TryGetHomeDirectory();
            if (string.IsNullOrEmpty(home)) return string.Empty;

            string appSupportSteam = Path.Combine(home, "Library/Application Support/Steam");
            if (Directory.Exists(appSupportSteam)) return appSupportSteam;
            
            return string.Empty;
        }
    }
}
