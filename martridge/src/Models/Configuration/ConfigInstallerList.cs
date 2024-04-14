using Martridge.Models.Configuration.Save;
using Martridge.Models.Localization;
using Martridge.Trace;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Martridge.Models.Configuration {
    public class ConfigInstallerList {
        public Dictionary<string, Dictionary<string,ConfigInstaller>> Installables { get; private set; } = new Dictionary<string, Dictionary<string,ConfigInstaller>>();

        public void SaveToFile(string path) {
            string pathTemp = path + ".temp";
            
            try { 
                FileInfo fileInfo = new FileInfo(pathTemp);
                if (fileInfo.Directory?.Exists == false) {
                    fileInfo.Directory.Create();
                }

                ConfigDataInstallerList listToSave = new ConfigDataInstallerList() {
                    InstallerDefinitions = new List<ConfigDataInstaller>(),
                };
                foreach (var kvp1 in this.Installables) {
                    foreach (var kvp2 in kvp1.Value) {
                        listToSave.InstallerDefinitions.Add(kvp2.Value.ToJsonData());
                    }
                }

                using (FileStream fs = new FileStream(pathTemp, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs)) {
                    sw.Write(JsonConvert.SerializeObject(listToSave, Formatting.Indented));
                    sw.Flush();
                    fs.Flush();
                    sw.Close();
                    fs.Close();
                }
                
                // if we got here, we successfully saved the data to a temporary path, time to move it to the real path
                if (File.Exists(pathTemp)) {
                    File.Move(pathTemp, path, true);
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerSaved"]);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerSaveFailure"]);
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                
                // remove temporary file if it was partially created...
                if (File.Exists(pathTemp)) {
                    File.Delete(pathTemp);
                }
            }
        }

        public static ConfigInstallerList? LoadFromFile(string path) {
            try {
                ConfigDataInstallerList? cfgJ = null;
                
                if (File.Exists(path)) {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    using (StreamReader sr = new StreamReader(fs)) {
                        cfgJ = JsonConvert.DeserializeObject<ConfigDataInstallerList>(sr.ReadToEnd());
                        fs.Flush();
                        fs.Close();
                    }
                }

                if (cfgJ == null || cfgJ.InstallerDefinitions == null) {
                    MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerLoad/Failure"]);
                    MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
                    return null;
                }

                ConfigInstallerList cfg = new ConfigInstallerList();
                bool hasErrors = false;

                foreach (ConfigDataInstaller installerJ in cfgJ.InstallerDefinitions) {
                    ConfigInstaller? installer = ConfigInstaller.FromJsonData(installerJ);
                    if (installer == null) {
                        MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerLoad/FailureComponent"]);
                        MyTrace.Global.WriteMessage(MyTraceCategory.General, "    " + (installerJ.Name ?? "???"));
                        hasErrors = true;
                        
                    } else {
                        if (cfg.Installables.ContainsKey(installer.Category) == false) {
                            cfg.Installables.Add(installer.Category, new Dictionary<string, ConfigInstaller>());
                        }

                        if (cfg.Installables[installer.Category].ContainsKey(installer.Name)) {
                            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerLoad/FailureDuplicateComponent"]);
                            MyTrace.Global.WriteMessage(MyTraceCategory.General, "    " + installer.Name);
                            hasErrors = true;
                        } else {
                            cfg.Installables[installer.Category].Add(installer.Name, installer);
                        }
                    }
                }

                if (hasErrors) {
                    MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerLoad/FailurePartial"]);
                    MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
                } else {
                    MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerLoad/Success"]);
                    MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
                }

                return cfg;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationInstallerLoad/Failure"]);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                return null;
            }
        }
    }
}
