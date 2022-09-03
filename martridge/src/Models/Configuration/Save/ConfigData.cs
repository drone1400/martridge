using Martridge.Models.Localization;
using Martridge.Trace;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Martridge.Models.Configuration.Save {
    public class ConfigData {
        public ConfigDataGeneral? General { get; set; }
        public ConfigDataLaunch? Launch { get; set; }
        public ConfigDataAlertResults? AlertResults { get; set; }
        
        
        public void SaveToFile(string path) {
            try {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Directory?.Exists == false) {
                    fileInfo.Directory.Create();
                }

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs)) {
                    sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
                    sw.Flush();
                    fs.Flush();
                    sw.Close();
                    fs.Close();
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationSaved"]);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationSaveFailure"]);
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
            }
        }

        public static ConfigData? LoadFromFile(string path) {
            try {
                ConfigData? cfg;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (StreamReader sr = new StreamReader(fs)) {
                    cfg = JsonConvert.DeserializeObject<ConfigData>(sr.ReadToEnd());
                    fs.Flush();
                    fs.Close();
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationLoaded"]);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");

                return cfg;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, "Could not load config file...");
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
                MyTrace.Global.WriteException(MyTraceCategory.General, ex, MyTraceLevel.Warning);
                return null;
            }
        }
    }
}
