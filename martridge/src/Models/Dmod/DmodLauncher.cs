using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DynamicData.Kernel;
using Martridge.Models.Configuration;
using Martridge.Trace;
using System.Linq;
using Avalonia;
using Martridge.Models.Configuration.General;
using Martridge.Models.Configuration.Generic;
using Martridge.Models.Configuration.LaunchExtension;
using Martridge.Models.Steam;

namespace Martridge.Models.Dmod
{
    public static class DmodLauncher
    {
        public static void LaunchDmod(string exePath, bool exeIsEditor, string dmodPath, ConfigLaunch launchCfg, ConfigExtensionComponent? extensionCfg = null, string? localization = null)
        {
            try {
                exePath = exePath.Trim();
                dmodPath = dmodPath.Trim();

                if (Directory.Exists(dmodPath) == false)
                    return;
                
                DmodFileDefinition dfd = new DmodFileDefinition(dmodPath);
                // certain engines like DinkHD will fail to launch a DMOD that does not have the dmod.diz file
                // make sure the file exists by generating a placeholder one if there's nothing there
                dfd.EnsureExistsDmodDiz();
                
                bool isProbablyFreeDink = false;
                bool isProbablyYeOldeDink = false;
            
                FileInfo finfo = new FileInfo(exePath);
                string launcherExeNameLower = Path.GetFileNameWithoutExtension(finfo.Exists ? finfo.Name.ToLowerInvariant() : string.Empty);
                string exePathLower = exePath.ToLowerInvariant();

                if (launcherExeNameLower.StartsWith("freedink")         // executable file that's freedink
                   ) {
                    isProbablyFreeDink = true;
                }
                if (launcherExeNameLower.StartsWith("yedink") ||        // executable file that's yeoldedink
                    launcherExeNameLower.StartsWith("yeoldedink") ||    // executable file that's yeoldedink
                    exePathLower.Contains("net.ultraprison.yeoldedink") // for yeoldedink flatpaks
                   ) {
                    isProbablyYeOldeDink = true;
                }

                string arguments = PrepareArguments(exePath, dmodPath, launchCfg, isProbablyFreeDink, isProbablyYeOldeDink);

                ProcessStartInfo? pinfo = null;
                if (File.Exists(exePath)) {
                    // starting from a normal executable
                    pinfo = PrepareProcessFromFile(exePath, arguments, isProbablyFreeDink, extensionCfg);
                }
                else if (exePath.ToLowerInvariant().StartsWith("steam://")) {
                    // starting from a steam uri
                    pinfo = PrepareProcessFromSteamUri(exePath, arguments);
                } else {
                    // this is not a file or a steam uri, attempt launching it using shell execute with arguments?...
                    pinfo = PrepareProcessStartGenericShellExecute(exePath, arguments);
                }

                if (pinfo == null)
                    return;
                
                // localization support for freedink/yeoldedink, but try to use it regardless of app type 
                if (localization != null) {
                    pinfo.Environment.RemoveIfContained("LANGUAGE");
                    pinfo.Environment.RemoveIfContained("LC_ALL");

                    pinfo.Environment.Add("LC_ALL", localization);
                    pinfo.Environment.Add("LANGUAGE", localization);
                }

                if (string.IsNullOrWhiteSpace(pinfo.WorkingDirectory)) {
                    // if the process working directory has not been set previously,
                    // use the DMOD directory as the working directory when launching
                    pinfo.WorkingDirectory = dmodPath;
                }

                Process? proc = Process.Start(pinfo);

                // check if need to quit martridge after launching
                bool quitAfterLaunch =
                    (exeIsEditor && launchCfg.QuitMartridgeOnEditorLaunch) ||
                    (!exeIsEditor && launchCfg.QuitMartridgeOnGameLaunch);
                if (quitAfterLaunch) {
                    (Application.Current as App)?.QuitApplication();
                    proc?.Start();
                }
                else {
                    proc?.WaitForExit();
                    MyTrace.Global.WriteMessage(new List<string>() {
                        Localization.Localizer.Instance["DmodLauncher/LaunchEnd"],
                        $"    FileName = \"{pinfo.FileName}\"",
                        $"    Arguments = {pinfo.Arguments}",
                        $"    Exit Code = {proc?.ExitCode}",
                    });
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        private static string PrepareArguments(string exePath, string dmodPath, ConfigLaunch launchCfg, bool isProbablyFreeDink, bool isProbablyYeOldeDink) {

            FileInfo finfo = new  FileInfo(exePath);
            
            string arguments = "";
            
            // appending the classic dink arguments
            if (launchCfg.TrueColor) {
                arguments += " -truecolor";
            }
            if (launchCfg.Windowed) {
                arguments += " -window";
            }
            if (launchCfg.Sound == false) {
                arguments += " -nosound";
            }
            if (launchCfg.Joystick == false) {
                arguments += " -nojoy";
            }
            if (launchCfg.Debug) {
                arguments += " -debug";
            }
            if (launchCfg.V107Mode) {
                arguments += " --v1.07";
            }
            
            // NOTE: the -skip parameter crashes freedink and freedinkedit, so don't automatically pass it there...
            // NOTE: not sure if this is the best idea or the user should just manually disable it if launching freedink?...
            //          maybe a future version of freedink would support this? ah well, i guess i'll just update martridge then...
            if (launchCfg.Skip && isProbablyFreeDink == false && isProbablyYeOldeDink == false) {
                arguments += " -skip";
            }

            // appending the -game path argument if using one
            if (string.IsNullOrWhiteSpace(dmodPath) == false) {
                DirectoryInfo dinfo = new DirectoryInfo(dmodPath);
                if (dinfo.Name.ToLowerInvariant() != "dink") {
                    string finalPath = dmodPath;

                    if (launchCfg.UsePathRelativeToGame && finfo.Exists && finfo.DirectoryName != null) {
                        finalPath = Path.GetRelativePath(finfo.DirectoryName, dmodPath);
                    }

                    finalPath = Path.TrimEndingDirectorySeparator(finalPath);

                    // NOTE: If path contains whitespace, force quotation marks on since otherwise you can't launch the DMOD
                    if (launchCfg.UsePathQuotationMarks || finalPath.Any(Char.IsWhiteSpace)) {
                        finalPath = $"\"{finalPath}\"";
                    }

                    arguments += " -game ";
                    arguments += finalPath;
                }
            }

            // appending the --refdir path argument if needed
            if (launchCfg.UseRefDir && string.IsNullOrWhiteSpace(launchCfg.RefDirPath) == false) {
                string refDirPath = Path.TrimEndingDirectorySeparator(launchCfg.RefDirPath);
                
                arguments += " --refdir ";
                if (launchCfg.UsePathQuotationMarks) {
                    arguments += $"\"{refDirPath}\"";
                }
                else {
                    arguments += refDirPath;
                }
            }

            // appending custom user arguments
            if (!string.IsNullOrWhiteSpace(launchCfg.CustomUserArguments)) {
                arguments += " ";
                arguments += launchCfg.CustomUserArguments;
            }

            // arguments are done!
            arguments = arguments.Trim();

            return arguments;
        }

        private static ProcessStartInfo? PrepareProcessFromFile(string exePath, string arguments, bool isProbablyFreeDink, ConfigExtensionComponent? extensionCfg = null) {
            FileInfo finfo = new FileInfo(exePath);
            if (finfo.Exists == false)
                return null;

            if (extensionCfg?.SteamData?.PreferLaunchingAsSteamApp == true && extensionCfg.SteamData.SteamId32 != 0) {
                ulong steamId64 = extensionCfg.SteamData.SteamId32;
                steamId64 = steamId64 << 32;
                steamId64 |= 0x0200_0000;
                
                // actually launch exe as steam app..
                string argumentsEscaped = Uri.EscapeDataString(arguments);
                string steamUri = $"steam://rungameid/{steamId64}//{argumentsEscaped}/";
                
                ProcessStartInfo pinfo = new ProcessStartInfo() {
                    FileName = steamUri,
                    Arguments = "",
                    UseShellExecute = true,
                    Verb = "open",
                };
                    
                MyTrace.Global.WriteMessage(new List<string>() {
                    Localization.Localizer.Instance["DmodLauncher/LaunchUsingSteam"],
                    $"    FileName = \"{steamUri}\"",
                    $"    Arguments = \"\"",
                });

                return pinfo;
            }
            else {
                ProcessStartInfo pinfo = new ProcessStartInfo() {
                    FileName = exePath,
                    Arguments = arguments,
                    WorkingDirectory = finfo.Directory?.FullName,
                };
                
                if (isProbablyFreeDink && finfo.Extension == ".exe") {
                    // this fixes a sound issue regarding playback of WAV files with FreeDink 109.6 under Windows 10 and 11
                    pinfo.Environment.RemoveIfContained("SDL_AUDIODRIVER");
                    pinfo.Environment.Add("SDL_AUDIODRIVER", "winmm");
                }

                
                
#if PLATF_WINDOWS
                ConfigWine? wineCfg = null;
#else
                ConfigWine? wineCfg = null;
                if (Config.Instance.GlobalWine.UseWine) wineCfg = Config.Instance.GlobalWine;
                // game specific wine configs override the global ones
                if (extensionCfg?.WineData.UseWine == true) wineCfg = extensionCfg.WineData;
#endif

                if (wineCfg != null) {
                    // actually check that this is a windows exe file..
                    using FileStream fs = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using BinaryReader br = new BinaryReader(fs);
                    byte magic1 = br.ReadByte();
                    byte magic2 = br.ReadByte();

                    if (magic1 != 0x4D && magic2 != 0x5A) {
                        // this does not appear to be a windows executable...
                        wineCfg = null;
                    }
                }

                if (wineCfg != null) {
                    string? wineBinary = null;
                    
                    foreach (var x in wineCfg.EnvVars) {
                        try {
                            if (x.Key == EnvironmentVariableHelper.WINELOADER) {
                                wineBinary = x.Value;
                            }
                            
                            string? crtVal = Environment.GetEnvironmentVariable(x.Key);
                            
                            switch (x.Mode) {
                                case ConfigEnvVarMode.Normal:
                                    pinfo.Environment[x.Key] = x.Value; 
                                    break;
                                case ConfigEnvVarMode.AppendEnd:
                                    pinfo.Environment[x.Key] = crtVal + x.AppendSeparator + x.Value;
                                    break;
                                case ConfigEnvVarMode.AppendStart:
                                    pinfo.Environment[x.Key] = x.Value + x.AppendSeparator + crtVal;
                                    break;
                            }
                        } catch (Exception ex) {
                            MyTrace.Global.WriteException(ex);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(wineBinary)) {
                        wineBinary = "wine"; // fallback to generic wine name
                    }
                    
                    // override some settings...
                    string wineArgs = "\"" + exePath + "\""  + " " + arguments;
                    pinfo.FileName = wineBinary;
                    pinfo.Arguments = wineArgs;
                }
                
                MyTrace.Global.WriteMessage(new List<string>() {
                    Localization.Localizer.Instance["DmodLauncher/LaunchFile"],
                    $"    FileName = \"{exePath}\"",
                    $"    Arguments = {arguments}",
                });

                return pinfo;
            }
        }
        
        //
        // WARNING ABOUT LAUNCHING USING steam://rungameid/<id>//<args>/
        //
        // Although Steam receives the argument list correctly, and gives you a confirmation window
        // with the correct arguments, when launching the app it seems to be overriden by whatever
        // you have configured in the properties for the steam shortcut of the app! 
        //
        // In other words, the passed arguments seem to be ignored when actually starting the app!
        //
        // I have not found a reasonable way to work around this, although it should technically be 
        // possible to forcefully edit the shortcuts.vdf file, replace the stored arguments there,
        // then restart steam to force it to reload the properties from the shortcuts.vdf file...
        //
        // But that seems pretty ridiculous and I don't think killing the steam process, editing the
        // shortcuts.vdf file, then re-launching steam should be within Martridge's scope at all...
        
        private static ProcessStartInfo? PrepareProcessFromSteamUri(string steamUri, string arguments) {
            if (steamUri.ToLowerInvariant().StartsWith("steam://") == false)
                return null;
            
            // steam://rungameid/<id>//<args>/
            // slashes in arguments have to be escaped with %2F
            //string argumentsEscaped = arguments.Trim().Replace("/", "%2F");
            string argumentsEscaped = Uri.EscapeDataString(arguments);
            string uri = steamUri + (
                steamUri.EndsWith("//")
                    ? string.Empty
                    : (steamUri.EndsWith("/") ? "/" : "//") 
                ) + argumentsEscaped + "/";

            ProcessStartInfo pinfo = new ProcessStartInfo() {
                FileName = uri,
                Arguments = "",
                UseShellExecute = true,
                Verb = "open",
            };
                    
            MyTrace.Global.WriteMessage(new List<string>() {
                Localization.Localizer.Instance["DmodLauncher/LaunchUsingSteam"],
                $"    FileName = \"{uri}\"",
                $"    Arguments = \"\"",
            });

            return pinfo;
        }

        private static ProcessStartInfo? PrepareProcessStartGenericShellExecute(string exePath, string arguments) {
            // NOTE: using only the first word or phrase as the "FileName", the rest are considered arguments...
            int stopIdx = 0;
            if (exePath.StartsWith('\"') || exePath.StartsWith('\'')) {
                // look for ending " or '
                // note: currently not allowing escaping the " or '...
                char stopChar = exePath[0];
                stopIdx++;
                while (stopIdx < exePath.Length) {
                    if (exePath[stopIdx] == stopChar) break;
                    stopIdx++;
                }
            } else {
                // look for whitespace
                stopIdx++;
                while (stopIdx < exePath.Length) {
                    if (exePath[stopIdx] == ' ' || exePath[stopIdx] == '\t') break;
                    stopIdx++;
                }
            }
                    
            string finalFileName = exePath.Substring(0,stopIdx).Trim();
            string argumentsPrefix = exePath.Substring(stopIdx).Trim();
            string finalArguments = string.IsNullOrWhiteSpace(argumentsPrefix)
                ? arguments : argumentsPrefix + " " + arguments;

            ProcessStartInfo pinfo = new ProcessStartInfo() {
                FileName = finalFileName,
                Arguments = finalArguments,
                UseShellExecute = true,
                Verb = "open",
            };
                    
            MyTrace.Global.WriteMessage(new List<string>() {
                Localization.Localizer.Instance["DmodLauncher/LaunchGeneric"],
                $"    FileName = \"{finalFileName}\"",
                $"    Arguments = {finalArguments}",
            });

            return pinfo;
        }
    }
}