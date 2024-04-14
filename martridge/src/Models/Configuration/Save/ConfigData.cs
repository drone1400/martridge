using Martridge.Models.Localization;
using Martridge.Trace;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Martridge.Models.Configuration.Save {
    public class ConfigData {
        public ConfigDataGeneral? General { get; set; }
        public ConfigDataLaunch? Launch { get; set; }
        //public ConfigDataAlertResults? AlertResults { get; set; }
        
        
        public void SaveToFile(string path) {
            string pathTemp = path + ".temp";
            
            try {
                FileInfo fileInfo = new FileInfo(pathTemp);
                if (fileInfo.Directory?.Exists == false) {
                    fileInfo.Directory.Create();
                }

                using (FileStream fs = new FileStream(pathTemp, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs)) {
                    sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
                    sw.Flush();
                    fs.Flush();
                    sw.Close();
                    fs.Close();
                }

                // if we got here, we successfully saved the data to a temporary path, time to move it to the real path
                if (File.Exists(pathTemp)) {
                    File.Move(pathTemp, path, true);
                }

                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationSaved"]);
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"    \"{path}\"");
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationSaveFailure"]);
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);

                // remove temporary file if it was partially created...
                if (File.Exists(pathTemp)) {
                    File.Delete(pathTemp);
                }
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
