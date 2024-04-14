using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DynamicData.Kernel;
using Martridge.Models.Configuration;
using Martridge.Trace;
using System.Linq;

namespace Martridge.Models.Dmod
{
    public static class DmodLauncher
    {
        public static void LaunchDmod(string exePath, string dmodPath, ConfigLaunch launch, string? localization = null)
        {
            try {
                bool isProbablyFreedink = false;
                
                FileInfo finfo = new FileInfo(exePath);
                DirectoryInfo dinfo = new DirectoryInfo(dmodPath);
                string launcherExeNameLower = Path.GetFileNameWithoutExtension(finfo.Name.ToLowerInvariant());

                if (launcherExeNameLower.StartsWith("freedink")) {
                    isProbablyFreedink = true;
                }
                
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
                
                // NOTE: the -skip parameter crashes freedink and freedinkedit, so don't automatically pass it there...
                // NOTE: not sure if this is the best idea or the user should just manually disable it if launching freedink?...
                //          maybe a future version of freedink would support this? ah well, i guess i'll just update martridge then...
                if (launch.Skip && isProbablyFreedink == false) {
                    arguments += " -skip";
                }
                
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
                if (isProbablyFreedink)
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