using Avalonia;
using Avalonia.Platform;
using Martridge.Trace;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace Martridge.Models.Localization {
    public class Localizer : INotifyPropertyChanged {
        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";
        private Dictionary<string, string>? _mStrings = null;

        private static Dictionary<string, FileInfo> _availableLocalizationFiles = new Dictionary<string, FileInfo>();
        private static Dictionary<string, Uri> _availableLocalizationInternalAssets = new Dictionary<string, Uri>() {
            ["en-US"] = new Uri($"avares://martridge/Assets/dloc/en-US.json"),
        };

        public static string DataNotAvailable {
            get => Localizer.Instance["Generic/DataNotAvailable"];
        }

        static Localizer() {
            ScanLocalizationFiles();
            
            Instance.LoadLanguageFromInternalAssets();
        }

        private static void ScanLocalizationFiles() {
            //CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            
            _availableLocalizationFiles.Clear();

            DirectoryInfo dirInfo = new DirectoryInfo(LocationHelper.LocalizationDirectory);
            if (!dirInfo.Exists) {
                return;
            }

            FileInfo[] locFiles = dirInfo.GetFiles();
            
            foreach (FileInfo finfo in locFiles) {
                try {
                    if (finfo.Extension.ToLowerInvariant() != ".json") 
                        continue;

                    string name = Path.GetFileNameWithoutExtension(finfo.Name);
                    
                    CultureInfo ci = CultureInfo.GetCultureInfo(name);
                    
                    // if culture info was initialized correctly without throwing any exception, add this to available culture infos...
                    _availableLocalizationFiles.Add(name, finfo);
                }  catch (Exception) {
                    // do nothing
                }
            }
        }

        public List<string> GetAvailableLanguages() {
            List<string> localizations = new List<string>();
            Dictionary<string, bool> tempLoc = new Dictionary<string, bool>();

            foreach (var kvp in _availableLocalizationInternalAssets) {
                tempLoc.Add(kvp.Key, true);
                localizations.Add(kvp.Key);
            }
            foreach (var kvp in _availableLocalizationFiles) {
                if (tempLoc.ContainsKey(kvp.Key) == false) {
                    tempLoc.Add(kvp.Key, true);
                    localizations.Add(kvp.Key);
                }
            }
            return localizations;
        }

        public bool LoadLanguage(string language) {
            // if language file does not exist, but internal asset does, load that
            if (!_availableLocalizationFiles.ContainsKey(language) && 
                _availableLocalizationInternalAssets.ContainsKey(language)) {
                this.LoadLanguageFromInternalAssets(language);
                return true;
            }
            
            // if language does not exist, get out
            if (!_availableLocalizationFiles.ContainsKey(language))
                return false;
            if (_availableLocalizationFiles[language].Exists != true)
                return false;

            try {
                using (FileStream fs = new FileStream(_availableLocalizationFiles[language].FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8)) {
                    this._mStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                }
                this.Language = language;
                this.Invalidate();
                return true;
            } catch (Exception ex) {
                MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Error loading localization {language}", MyTraceLevel.Critical);
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                this.LoadLanguageFromInternalAssets();
                return false;
            }
        }

        private void LoadLanguageFromInternalAssets(string language = "en-US") {
            Uri uri = new Uri($"avares://martridge/Assets/dloc/{language}.json");
            if (AssetLoader.Exists(uri)) {
                using (StreamReader sr = new StreamReader(AssetLoader.Open(uri), Encoding.UTF8)) {
                    this._mStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                }
                this.Language = language;
                this.Invalidate();
            } else {
                Exception ex = new NullReferenceException("Could not load default localization! This should be impossible?...");
                MyTrace.Global.WriteException(MyTraceCategory.General, ex, MyTraceLevel.Critical);
                throw ex;
            }
        }

        public string Language { get; private set; } = "en-US";

        public string this[string key] {
            get {
                string? res;
                if (this._mStrings != null && this._mStrings.TryGetValue(key, out res))
                    return res.Replace("\\n", "\n");

                return $"{this.Language}:{key}";
            }
        }

        public static Localizer Instance { get; set; } = new Localizer();
        public event PropertyChangedEventHandler? PropertyChanged;

        private void Invalidate() {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
    }
}
