using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DynamicData.Kernel;
using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.Trace;
using System.Linq;
using System.Runtime.InteropServices;

namespace Martridge.Models.Dmod
{
    public static class DmodLauncher
    {

        public static bool GetCreateSymbolicLinkCommandWindows(string dmodPath, string dinkPath, out string? symLinkPath, out string? command) {
            symLinkPath = null;
            command = null;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, "Automatic symbolic link creation is only supported on windows for now!....", MyTraceLevel.Error);
                // TODO... maybe implement this for linux?..
                // TODO... check if WDEP runs on linux with Wine or something?... hah
                return false;
            }

            DirectoryInfo dmodDir = new DirectoryInfo(dmodPath);
            DirectoryInfo dinkDir = new DirectoryInfo(dinkPath);
            
            if (dmodDir.Exists == false) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                    Localizer.Instance[@"DmodBrowser/ErrorMissingDirectory"],
                    $"    Path = \"{dmodPath}\""
                }, MyTraceLevel.Error);
                return false;
            }
            
            if (dinkDir.Exists == false) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                    Localizer.Instance[@"DmodBrowser/ErrorMissingDirectory"],
                    $"    Path = \"{dmodPath}\""
                }, MyTraceLevel.Error);
                return false;
            }

            symLinkPath = Path.Combine(dinkDir.FullName, $"SL_{dmodDir.Name}");
            DirectoryInfo symLinkInfo = new DirectoryInfo(symLinkPath);
            int index = 1;
            
            while (symLinkInfo.Exists) {
                if (symLinkInfo.Exists && symLinkInfo.FullName == dmodDir.FullName) {
                    // dmod already in destination?!
                    MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                        Localizer.Instance[@"DmodBrowser/ErrorDmodAlreadyInSymbolicLinkDestination"],
                        $"    Path = \"{dmodPath}\""
                    }, MyTraceLevel.Error);
                    return false;
                }
                if (symLinkInfo.Exists && symLinkInfo.LinkTarget != null && symLinkInfo.LinkTarget == dmodDir.FullName) {
                    // dmod already in destination?!
                    MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                        Localizer.Instance[@"DmodBrowser/ErrorSymbolicLinkAlreadyExists"],
                        $"    Path = \"{dmodPath}\""
                    }, MyTraceLevel.Error);
                    return false;
                }
                
                // a directory with the same name already exists... try a different name
                symLinkPath = Path.Combine(dinkDir.FullName, $"SL_{dmodDir.Name}_{index}" );
                symLinkInfo = new DirectoryInfo(symLinkPath);
                
                // increment index for next attempt
                index++;
            }

            command = $"mklink /d \"{symLinkPath}\" \"{dmodDir.FullName}\"";

            return true;
        }
        public static int LaunchWindowsCmdAsAdmin(string cmdCommand) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, "Operation only supported on windows...", MyTraceLevel.Error);
                // TODO... maybe implement this for linux?..
                // TODO... check if WDEP runs on linux with Wine or something?... hah
                return int.MinValue;
            }

            cmdCommand = $"/C {cmdCommand}";
            
            ProcessStartInfo pinfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "cmd.exe",
                Arguments = cmdCommand,
                UseShellExecute = true,
                Verb = "runas",
            };
            
            Process? proc = Process.Start(pinfo);
                
            proc?.WaitForExit();
            MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                "<Running CMD>",
                $"    Dink      = \"cmd.exe\"",
                $"    Args      = {cmdCommand}",
                $"    Exit Code = {proc?.ExitCode}",
            });

            return proc?.ExitCode ?? int.MinValue;
        }
        
        public static void LaunchDmod(string exePath, string dmodPath, ConfigLaunch launch, bool launchAsAdmin = false, string? localization = null)
        {
            try {
                string arguments = "";
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

                FileInfo finfo = new FileInfo(exePath);
                DirectoryInfo dinfo = new DirectoryInfo(dmodPath);
                
                if (dinfo.Name.ToLowerInvariant() != "dink") {
                    string finalPath = dmodPath;
                    
                    if (launch.UsePathRelativeToGame && finfo.DirectoryName != null) {
                        finalPath = Path.GetRelativePath(finfo.DirectoryName, dmodPath);
                    }

                    // NOTE: If path contains whitespace, force quotation marks on since otherwise you can't launch the DMOD
                    if (launch.UsePathQuotationMarks ||
                        finalPath.Any(c => Char.IsWhiteSpace(c))) {
                        finalPath = $"\"{finalPath}\"";
                    }
                    
                    arguments += " -game ";
                    arguments += finalPath;
                }

                if (!string.IsNullOrWhiteSpace(launch.CustomUserArguments)) {
                    arguments += " ";
                    arguments += launch.CustomUserArguments;
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                    "<Attempting to start dmod>",
                    $"    Dink = \"{exePath}\"",
                    $"    Args = {arguments}",
                    });

                ProcessStartInfo pinfo = new ProcessStartInfo()
                {
                    FileName = exePath,
                    Arguments = arguments,
                    WorkingDirectory = finfo.Directory?.FullName,
                };
                
                // localization support for freedink
                // localization = "es_ES";
                string launcherExeNameLower = Path.GetFileNameWithoutExtension(finfo.Name.ToLowerInvariant());
                if (launcherExeNameLower == "freedink")
                {
                    // try to add localization parameters
                    if (localization != null) {
                        pinfo.Environment.RemoveIfContained("LANGUAGE");
                        pinfo.Environment.RemoveIfContained("LC_ALL");

                        pinfo.Environment.Add("LC_ALL", localization);
                        pinfo.Environment.Add("LANGUAGE", localization);
                    }

                    // this fixes a sound issue regarding playback of WAV files with FreeDink 109.6 under Windows 10 and 11
                    if (finfo.Extension == ".exe") {
                        pinfo.Environment.RemoveIfContained("SDL_AUDIODRIVER");
                        pinfo.Environment.Add("SDL_AUDIODRIVER", "winmm");
                    }
                }

                if (launchAsAdmin) {
                    pinfo.UseShellExecute = true;
                    pinfo.Verb = "runas";
                }
                
                Process? proc = Process.Start(pinfo);
                
                proc?.WaitForExit();
                MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                    "<Dmod process ended>",
                    $"    Dink      = \"{exePath}\"",
                    $"    Args      = {arguments}",
                    $"    Exit Code = {proc?.ExitCode}",
                });

            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex);
            }
        }
    }
}