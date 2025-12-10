using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Martridge.Models.Localization;
using Martridge.Trace;

#if PLATF_WINDOWS
using WindowsShortcutFactory;
#endif

namespace Martridge.Models {
    public static class DesktopShortcutHelper {

        public static void CreateDesktopShortcut() {
#if PLATF_LINUX
            CreateLinuxShortcuts();
#endif
            
#if PLATF_WINDOWS
            CreateWindowsShortcuts();
#endif
        }

        private static void CreateWindowsShortcuts() {
            CreateWindowsVisualElementsManifest();
            CreateWindowsFileShortcut();
        }

        private static void CreateWindowsFileShortcut() {
            try {
#if PLATF_WINDOWS
                string martridgeRoot = LocationHelper.GetPathMartridge();
                string martridgeExe = LocationHelper.GetPathMartridgeExecutable();

                if (string.IsNullOrWhiteSpace(martridgeRoot))
                    return;
                if (string.IsNullOrWhiteSpace(martridgeExe))
                    return;

                WindowsShortcut shortcut = new WindowsShortcut() {
                    Path = martridgeExe,
                    Description = Localizer.Instance["AboutWindow/Description"],
                };
                
                string shortcutName = "Martridge.lnk";
#if DEBUG
                shortcutName = "Martridge (Debug).lnk";
#endif

                // save to start menu
                string appDataRoaming = LocationHelper.TryGetWindowsAppDataRoaming();
                if (string.IsNullOrWhiteSpace(appDataRoaming) == false) {
                    string startMenuShortcutDir = Path.Combine(appDataRoaming, "Microsoft", "Windows", "Start Menu", "Programs", "custom");
                    
                    if (Directory.Exists(startMenuShortcutDir)) {
                        shortcut.Save(Path.Combine(startMenuShortcutDir, shortcutName));
                    }
                }

                // save to desktop
                string desktopShortcutDir = LocationHelper.TryGetWindowsDesktop();
                if (Directory.Exists(desktopShortcutDir)) {
                    shortcut.Save(Path.Combine(desktopShortcutDir, shortcutName));
                }
#endif
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        private static void CreateWindowsVisualElementsManifest() {
            void EnsureWindowsIconBitmapExists() {
                string iconName150 = Path.Combine(LocationHelper.GetPathMartridge(), "assets", "martridge_icon_150.png");
                string iconName70 = Path.Combine(LocationHelper.GetPathMartridge(), "assets", "martridge_icon_70.png");
                string assetsDir =  Path.Combine(LocationHelper.GetPathMartridge(), "assets");
                if (Directory.Exists(assetsDir) == false) {
                    Directory.CreateDirectory(assetsDir);
                }
                if (File.Exists(iconName150) == false || File.Exists(iconName70) == false) {
                    using Bitmap icon = new Bitmap(AssetLoader.Open(new Uri("avares://martridge/Assets/martridge_new.ico")));
                    if (File.Exists(iconName150) == false) {
                        using Bitmap icon150 = icon.CreateScaledBitmap(new PixelSize(150, 150), BitmapInterpolationMode.HighQuality);
                        icon150.Save(iconName150);
                    }

                    if (File.Exists(iconName70) == false) {
                        using Bitmap icon70 = icon.CreateScaledBitmap(new PixelSize(70, 70), BitmapInterpolationMode.HighQuality);
                        icon70.Save(iconName70);
                    }
                }
            }
            
            try {
                string martridgeRoot = LocationHelper.GetPathMartridge();
                string martridgeName = LocationHelper.GetPathMartridgeExecutable();

                if (string.IsNullOrWhiteSpace(martridgeRoot))
                    return;
                if (string.IsNullOrWhiteSpace(martridgeName))
                    return;
                
                EnsureWindowsIconBitmapExists();
                
                FileInfo fileInfoMartridge = new FileInfo(martridgeName);
                string prefix = Path.GetFileNameWithoutExtension(fileInfoMartridge.Name);
                string vemFile = Path.Combine(martridgeRoot, prefix + ".VisualElementsManifest.xml");
                using FileStream fs = new FileStream(vemFile, FileMode.Create, FileAccess.Write);
                using StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine("<Application xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>");
                sw.WriteLine("  <VisualElements");
                sw.WriteLine("      ShowNameOnSquare150x150Logo='on'");
                sw.WriteLine("      Square150x150Logo='assets/martridge_icon_150.png'");
                sw.WriteLine("      Square70x70Logo='assets/martridge_icon_70.png'");
                sw.WriteLine("      ForegroundText='light'");
                sw.WriteLine("      BackgroundColor='#2D2D2D' />");
                sw.WriteLine("</Application>");
                sw.Close();
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        private static void CreateLinuxShortcuts() {
            CreateLinuxDesktopShortcut();
        }

        private static void CreateLinuxDesktopShortcut() {
            try {
                string myName = LocationHelper.GetPathMartridgeExecutable();
                if (string.IsNullOrWhiteSpace(myName))
                    return;

                string homeDir = LocationHelper.TryGetHomeDirectory();
                if (string.IsNullOrWhiteSpace(homeDir))
                    return;

                string fileDesktop = "martridge_app.desktop";
                
#if DEBUG
                fileDesktop = "martridge_app_debug.desktop";
#endif
                
                string fileName = Path.Combine(homeDir, ".local", "share", "applications", fileDesktop);

                string iconName = EnsureLinuxIconBitmapExists();

                FileInfo fileInfo = new FileInfo(fileName);
                if (fileInfo.Directory?.Exists == false) {
                    fileInfo.Directory?.Create();
                }

                using FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                using StreamWriter sw = new StreamWriter(fs);


                sw.WriteLine("[Desktop Entry]");
#if DEBUG
                sw.WriteLine("Name=Martridge (Debug)");
#else
                sw.WriteLine("Name=Martridge");
#endif
                sw.WriteLine($"Comment={Localizer.Instance["AboutWindow/Description"]}");
                sw.WriteLine($"Exec={myName}");
                if (string.IsNullOrWhiteSpace(iconName) == false) {
                    sw.WriteLine($"Icon={iconName}");
                }
                sw.WriteLine("Terminal=false");
                sw.WriteLine("Type=Application");
                sw.WriteLine("Categories=Games;");
                sw.WriteLine("StartupNotify=true");
                sw.WriteLine("NoDisplay=false");

                sw.Close();
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        private static string EnsureLinuxIconBitmapExists() {
            string iconName = Path.Combine(LocationHelper.GetPathMartridge(), "assets", "martridge_icon.png");
            string assetsDir =  Path.Combine(LocationHelper.GetPathMartridge(), "assets");
            if (Directory.Exists(assetsDir) == false) {
                Directory.CreateDirectory(assetsDir);
            }
            if (File.Exists(iconName) == false) {
                using Bitmap icon = new Bitmap(AssetLoader.Open(new Uri("avares://martridge/Assets/martridge_new.ico")));
                // apparently this should do the PNG conversion?... hmmm
                icon.Save(iconName);
            }
            return iconName;
        }
    }
}
