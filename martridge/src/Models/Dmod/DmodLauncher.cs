using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DynamicData.Kernel;
using Martridge.Trace;
using System.Linq;

namespace Martridge.Models.Dmod
{
    public static class DmodLauncher
    {
        public static void LaunchDmod(Configuration.Config config, string? path, string? localization = null)
        {
            try {
                if (path == null) return;
                
                string gameExe = config.General.GameExePaths[config.General.ActiveGameExeIndex];

                string arguments = "";
                if (config.Launch.TrueColor) {
                    arguments += " -truecolor";
                }
                if (config.Launch.Windowed) {
                    arguments += " -window";
                }
                if (config.Launch.Sound == false) {
                    arguments += " -nosound";
                }
                if (config.Launch.Joystick == false) {
                    arguments += " -nojoy";
                }
                if (config.Launch.Debug) {
                    arguments += " -debug";
                }
                if (config.Launch.V107Mode) {
                    arguments += " --v1.07";
                }

                FileInfo finfo = new FileInfo(gameExe);
                DirectoryInfo dinfo = new DirectoryInfo(path);
                
                if (dinfo.Name.ToLowerInvariant() != "dink") {
                    string finalPath = path;
                    
                    if (config.Launch.UsePathRelativeToGame && finfo.DirectoryName != null) {
                        finalPath = Path.GetRelativePath(finfo.DirectoryName, path);
                    }

                    // NOTE: If path contains whitespace, force quotation marks on since otherwise you can't launch the DMOD
                    if (config.Launch.UsePathQuotationMarks ||
                        finalPath.Any(c => Char.IsWhiteSpace(c))) {
                        finalPath = $"\"{finalPath}\"";
                    }
                    
                    arguments += " -game ";
                    arguments += finalPath;
                }

                if (!string.IsNullOrWhiteSpace(config.Launch.CustomUserArguments)) {
                    arguments += " ";
                    arguments += config.Launch.CustomUserArguments;
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                    "<Attempting to start dmod>",
                    $"    Dink = \"{gameExe}\"",
                    $"    Args = {arguments}",
                    });

                ProcessStartInfo pinfo = new ProcessStartInfo()
                {
                    FileName = gameExe,
                    Arguments = arguments,
                };
                
                // localization support for freedink
                // localization = "es_ES";
                if (Path.GetFileNameWithoutExtension(finfo.Name.ToLowerInvariant()) == "freedink")
                {
                    if (localization != null) {
                        pinfo.Environment.RemoveIfContained("LANGUAGE");
                        pinfo.Environment.RemoveIfContained("LC_ALL");

                        pinfo.Environment.Add("LC_ALL", localization);
                        pinfo.Environment.Add("LANGUAGE", localization);
                    }

                    if (finfo.Extension == ".exe") {
                        pinfo.Environment.RemoveIfContained("SDL_AUDIODRIVER");
                        // this fixes a sound issue regarding playback of WAV files with FreeDink 109.6 under Windows 10 and 11
                        pinfo.Environment.Add("SDL_AUDIODRIVER", "winmm");
                    }
                }
                
                Process? proc = Process.Start(pinfo);
                
                proc?.WaitForExit();
                MyTrace.Global.WriteMessage(MyTraceCategory.DmodBrowser, new List<string>() {
                    "<Dmod process ended>",
                    $"    Dink      = \"{gameExe}\"",
                    $"    Args      = {arguments}",
                    $"    Exit Code = {proc?.ExitCode}",
                });

            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex);
            }
        }
    }
}