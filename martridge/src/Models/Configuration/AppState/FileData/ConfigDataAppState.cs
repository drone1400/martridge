using System;
using System.IO;
using Martridge.Models.Configuration.Save;
using Martridge.Models.Localization;
using Martridge.Trace;
using Newtonsoft.Json;
namespace Martridge.Models.Configuration.AppState.FileData {
    public class ConfigDataAppState{
        public ConfigDataRemember? Remember { get; set; }
        
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

                MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationSaved"]);
                MyTrace.Global.WriteMessage($"    \"{path}\"");
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationSaveFailure"]);
                MyTrace.Global.WriteException(ex);

                // remove temporary file if it was partially created...
                if (File.Exists(pathTemp)) {
                    File.Delete(pathTemp);
                }
            }
        }

        public static ConfigDataAppState? LoadFromFile(string path) {
            try {
                ConfigDataAppState? cfg;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (StreamReader sr = new StreamReader(fs)) {
                    cfg = JsonConvert.DeserializeObject<ConfigDataAppState>(sr.ReadToEnd());
                    fs.Flush();
                    fs.Close();
                }

                MyTrace.Global.WriteMessage(Localizer.Instance["General/ConfigurationLoaded"]);
                MyTrace.Global.WriteMessage($"    \"{path}\"");

                return cfg;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage("Could not load config file...");
                MyTrace.Global.WriteMessage($"    \"{path}\"");
                MyTrace.Global.WriteException(ex, MyTraceLevel.Warning);
                return null;
            }
        }
    }
}
