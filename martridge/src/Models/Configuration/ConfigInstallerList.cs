using Martridge.Models.Localization;
using Martridge.Trace;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Martridge.Models.Configuration {
    public class ConfigInstallerList {
        public List<ConfigInstaller> InstallableVersions { get; private set; } = new List<ConfigInstaller>();

        public static ConfigInstallerList GetDefaultInstallers() {
            return new ConfigInstallerList() {
                InstallableVersions = new List<ConfigInstaller>() {
                    ConfigInstaller.GetDefaultInstaller_DinkHD_LatestRTSoft(),
                    ConfigInstaller.GetDefaultInstaller_FreeDinkWith108DataAndLocalizations(),
                    ConfigInstaller.GetDefaultInstaller_FreeDinkWith108Data(),
                    ConfigInstaller.GetDefaultInstaller_DinkHD_193(),
                    ConfigInstaller.GetDefaultInstaller_Dink_108(),
                },
            };
        }

        public void SaveToFile(string path) {
            try { 
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Directory?.Exists == false) {
                    fileInfo.Directory.Create();
                }

                using (FileStream fs = new FileStream(path, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs)) {
                    sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
                    sw.Flush();
                    fs.Flush();
                    sw.Close();
                    fs.Close();
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerSaved"]);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerSaveFailure"]);
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }

        public static ConfigInstallerList? LoadFromFile(string path) {
            try {
                ConfigInstallerList? cfg;
                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (StreamReader sr = new StreamReader(fs)) {
                    cfg = JsonConvert.DeserializeObject<ConfigInstallerList>(sr.ReadToEnd());
                    fs.Flush();
                    fs.Close();
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerLoaded"]);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");

                return cfg;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                return null;
            }
        }
    }
}
