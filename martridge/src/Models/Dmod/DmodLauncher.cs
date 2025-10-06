using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DynamicData.Kernel;
using Martridge.Models.Configuration;
using Martridge.Trace;
using System.Linq;
using Avalonia;

namespace Martridge.Models.Dmod
{
    public static class DmodLauncher
    {
        public static void LaunchDmod(string exePath, string dmodPath, ConfigLaunch launch, bool quitAfterLaunching, string? localization = null)
        {
            try {
                exePath = exePath.Trim();
                dmodPath = dmodPath.Trim();
                
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
                
                string arguments = "";
                
                // appending the classic dink arguments
                if (launch.TrueColor) {
                    arguments += " -truecolor";
                }
                if (launch.Windowed) {
                    arguments += " -window";
                }
                if (launch.Sound == false) {
                    arguments += " -nosound";
                }
                if (launch.Joystick == false) {
                    arguments += " -nojoy";
                }
                if (launch.Debug) {
                    arguments += " -debug";
                }
                if (launch.V107Mode) {
                    arguments += " --v1.07";
                }
                
                // NOTE: the -skip parameter crashes freedink and freedinkedit, so don't automatically pass it there...
                // NOTE: not sure if this is the best idea or the user should just manually disable it if launching freedink?...
                //          maybe a future version of freedink would support this? ah well, i guess i'll just update martridge then...
                if (launch.Skip && isProbablyFreeDink == false && isProbablyYeOldeDink == false) {
                    arguments += " -skip";
                }

                // appending the -game path argument if using one
                if (string.IsNullOrWhiteSpace(dmodPath) == false) {
                    DirectoryInfo dinfo = new DirectoryInfo(dmodPath);
                    if (dinfo.Name.ToLowerInvariant() != "dink") {
                        string finalPath = dmodPath;

                        if (launch.UsePathRelativeToGame && finfo.Exists && finfo.DirectoryName != null) {
                            finalPath = Path.GetRelativePath(finfo.DirectoryName, dmodPath);
                        }

                        finalPath = Path.TrimEndingDirectorySeparator(finalPath);

                        // NOTE: If path contains whitespace, force quotation marks on since otherwise you can't launch the DMOD
                        if (launch.UsePathQuotationMarks || finalPath.Any(Char.IsWhiteSpace)) {
                            finalPath = $"\"{finalPath}\"";
                        }

                        arguments += " -game ";
                        arguments += finalPath;
                    }
                }

                // appending the --refdir path argument if needed
                if (launch.UseRefDir && string.IsNullOrWhiteSpace(launch.RefDirPath) == false) {
                    string refDirPath = Path.TrimEndingDirectorySeparator(launch.RefDirPath);
                    
                    arguments += " --refdir ";
                    if (launch.UsePathQuotationMarks) {
                        arguments += $"\"{refDirPath}\"";
                    }
                    else {
                        arguments += refDirPath;
                    }
                }

                // appending custom user arguments
                if (!string.IsNullOrWhiteSpace(launch.CustomUserArguments)) {
                    arguments += " ";
                    arguments += launch.CustomUserArguments;
                }

                // arguments are done!
                arguments = arguments.Trim();
                
                ProcessStartInfo pinfo = new ProcessStartInfo();
                string finalFileName = "";
                string finalArguments = "";
                
                if (finfo.Exists) {
                    // ---------------------------------------------------
                    // starting from a normal executable
                    // ---------------------------------------------------

                    finalFileName = exePath;
                    finalArguments = arguments;
                    
                    pinfo.FileName = finalFileName;
                    pinfo.Arguments = finalArguments;
                    pinfo.WorkingDirectory = finfo.Directory?.FullName;
                    
                    // localization support for freedink/yeoldedink
                    
                    // try to add localization parameters
                    if (localization != null) {
                        pinfo.Environment.RemoveIfContained("LANGUAGE");
                        pinfo.Environment.RemoveIfContained("LC_ALL");

                        pinfo.Environment.Add("LC_ALL", localization);
                        pinfo.Environment.Add("LANGUAGE", localization);
                    }
                    
                    
                    if (isProbablyFreeDink && finfo.Extension == ".exe")
                    {
                        // this fixes a sound issue regarding playback of WAV files with FreeDink 109.6 under Windows 10 and 11
                        pinfo.Environment.RemoveIfContained("SDL_AUDIODRIVER");
                        pinfo.Environment.Add("SDL_AUDIODRIVER", "winmm");
                    }
                
                    MyTrace.Global.WriteMessage(new List<string>() {
                        Localization.Localizer.Instance["DmodLauncher/LaunchFile"],
                        $"    FileName = \"{finalFileName}\"",
                        $"    Arguments = {finalArguments}",
                    });
                }
                else if (exePathLower.StartsWith("steam://")) {
                    // ---------------------------------------------------
                    // starting from a steam uri
                    // ---------------------------------------------------
                    
                    // have to respect "steam://run/<id>//<args>/" URI format to launch something through steam with arguments....
                    // slashes have to be escaped with %2F
                    string argumentsEscaped = arguments.Trim().Replace("/", "%2F");
                    finalFileName = exePath + "//" + argumentsEscaped + "/";
                    finalArguments = "";
                    pinfo.FileName = finalFileName;
                    pinfo.Arguments = finalArguments;
                    pinfo.UseShellExecute = true;
                    pinfo.Verb = "open";
                    
                    MyTrace.Global.WriteMessage(new List<string>() {
                        Localization.Localizer.Instance["DmodLauncher/LaunchUsingSteam"],
                        $"    FileName = \"{finalFileName}\"",
                        $"    Arguments = {finalArguments}",
                    });
                } else {
                    // ---------------------------------------------------
                    // this is not a file or a steam uri, attempt launching it using shell execute with arguments?...
                    // ---------------------------------------------------

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
                    
                    finalFileName = exePath.Substring(0,stopIdx).Trim();
                    string arguments1 = exePath.Substring(stopIdx).Trim();
                    finalArguments = string.IsNullOrWhiteSpace(arguments1)
                        ? arguments : arguments1 + " " + arguments;
                    
                    pinfo.FileName = finalFileName;
                    pinfo.Arguments = finalArguments;
                    pinfo.UseShellExecute = true;
                    pinfo.Verb = "open";
                    
                    MyTrace.Global.WriteMessage(new List<string>() {
                        Localization.Localizer.Instance["DmodLauncher/LaunchGeneric"],
                        $"    FileName = \"{finalFileName}\"",
                        $"    Arguments = {finalArguments}",
                    });
                }

                Process? proc = Process.Start(pinfo);

                if (quitAfterLaunching) {
                    (Application.Current as App)?.QuitApplication();
                }
                
                proc?.WaitForExit();
                MyTrace.Global.WriteMessage(new List<string>() {
                    Localization.Localizer.Instance["DmodLauncher/LaunchEnd"],
                    $"    FileName = \"{finalFileName}\"",
                    $"    Arguments = {finalArguments}",
                    $"    Exit Code = {proc?.ExitCode}",
                });
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }
    }
}