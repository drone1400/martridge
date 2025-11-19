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
            string steamOs = @"/home/deck/.steam/steam";
            if (Directory.Exists(steamOs)) return steamOs;

            string home = LocationHelper.TryGetHomeDirectory();
            if (string.IsNullOrEmpty(home)) return string.Empty;

            string steamHome1 = Path.Combine(home, ".steam/steam");
            if (Directory.Exists(steamHome1)) return steamHome1;
            
            string steamHome2 = Path.Combine(home, ".local/share/Steam");
            if (Directory.Exists(steamHome2)) return steamHome2;

            string steamSnap = Path.Combine(home, "snap/steam/common/.local/share/Steam/");
            if (Directory.Exists(steamSnap)) return steamSnap;
            
            string steamFlatPak = Path.Combine(home, ".var/app/com.valvesoftware.Steam/data/Steam");
            if (Directory.Exists(steamFlatPak)) return steamFlatPak;

            return string.Empty;
        }
    }
}
