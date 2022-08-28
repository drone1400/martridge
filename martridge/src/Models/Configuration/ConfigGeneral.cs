using Martridge.Models.Configuration.Save;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Martridge.Models.Configuration {
    public class ConfigGeneral  {

        public event EventHandler? Updated;
        
        /// <summary>
        /// The name of the localization table to use int he application.
        /// </summary>
        /// <remarks>
        /// Example: "en-US"
        /// </remarks>
        public string LocalizationName { get; private set; } = "en-US";
        
        /// <summary>
        /// Indicates if the application should automatically update the existing 'configInstallerList.json' file
        /// </summary>
        public bool AutoUpdateInstallerList { get; private set; } = true;


        /// <summary>
        /// Indicates if the application should enable certain advanced features...
        /// </summary>
        /// <remarks>NOTE: Not really used much right now...</remarks>
        public bool ShowAdvancedFeatures { get; private set; } = true;
        
        /// <summary>
        /// Indicates if the application should show the info/error log window when starting
        /// </summary>
        public bool ShowLogWindowOnStartup { get; private set; } = true;
        
        /// <summary>
        /// Indicates if the application should use relative paths for paths related to its subfolders.
        /// This helps with keeping the application portable...
        /// </summary>
        public bool UseRelativePathForSubfolders { get; private set; } = true;

        /// <summary>
        /// Last selected game executable for launching Dink
        /// </summary>
        public int ActiveGameExeIndex {
            get => this._activeGameExeIndex;
            set {
                this._activeGameExeIndex = value;
                this.FireUpdatedEvent();
            }
        }
        private int _activeGameExeIndex = 0;

        /// <summary>
        /// List of paths to Dink executable files
        /// </summary>
        public ReadOnlyCollection<string> GameExePaths {get; }
        private readonly List<string> _gameExePaths = new List<string>();
        
        /// <summary>
        /// The default location where DMODS should get installed by the application
        /// </summary>
        public string DefaultDmodLocation { get; private set; } = LocationHelper.TryGetAbsoluteFromSubdirectoryRelative("DMODS");

        /// <summary>
        /// List of additional directories to scan for DMODS
        /// </summary>
        public ReadOnlyCollection<string> AdditionalDmodLocations { get; }
        private readonly List<string> _additionalDmodLocations = new List<string>();

        public ConfigGeneral() {
            this.GameExePaths = new ReadOnlyCollection<string>(this._gameExePaths);
            this.AdditionalDmodLocations = new ReadOnlyCollection<string>(this._additionalDmodLocations);
        }
        
        private void FireUpdatedEvent() {
            this.Updated?.Invoke(this, EventArgs.Empty);
        }

        private static bool ListsAreDifferent(List<string> list1, List<string> list2) {
            if (list1.Count != list2.Count) {
                return true;
            }
            
            for (int i = 0; i < list1.Count; i++) {
                if (list1[i] != list2[i]) {
                    return true;
                }
            }

            return false;
        }

        public void AddGameExePath(string path) {
            if (this._gameExePaths.Contains(path)) return;
            
            this._gameExePaths.Add(path);
            this.FireUpdatedEvent();
        }
        
        public void AddAdditionalDmodPath(string path) {
            if (this._additionalDmodLocations.Contains(path)) return;
            
            this._additionalDmodLocations.Add(path);
            this.FireUpdatedEvent();
        }

        public void UpdateAll(
            string? localizationName,
            bool showAdvancedFeatures,
            bool autoUpdateInstallerList,
            bool showLogWindowOnStartup,
            bool useRelativePathForSubfolders,
            int activeGameExeIndex,
            List<string> gameExePaths,
            string defaultDmodLocation,
            List<string> additionalDmodsLocations) {

            bool hasChanges = false;
            
            if (localizationName != null && this.LocalizationName != localizationName) {
                this.LocalizationName = localizationName;
                hasChanges = true;
            }
            
            if (this.ShowAdvancedFeatures != showAdvancedFeatures) {
                this.ShowAdvancedFeatures = showAdvancedFeatures;
                hasChanges = true;
            }
            
            if (this.AutoUpdateInstallerList != autoUpdateInstallerList) {
                this.AutoUpdateInstallerList = autoUpdateInstallerList;
                hasChanges = true;
            }
            
            if (this.ShowLogWindowOnStartup != showLogWindowOnStartup) {
                this.ShowLogWindowOnStartup = showLogWindowOnStartup;
                hasChanges = true;
            }
            
            if (this.UseRelativePathForSubfolders != useRelativePathForSubfolders) {
                this.UseRelativePathForSubfolders = useRelativePathForSubfolders;
                hasChanges = true;
            }
            
            
            if (this.ActiveGameExeIndex != activeGameExeIndex) {
                this.ActiveGameExeIndex = activeGameExeIndex;
                hasChanges = true;
            }
            
            if (this.DefaultDmodLocation != defaultDmodLocation) {
                this.DefaultDmodLocation = defaultDmodLocation;
                hasChanges = true;
            }

            if (ListsAreDifferent(this._gameExePaths, gameExePaths)) {
                this._gameExePaths.Clear();
                foreach (string s in gameExePaths) {
                    this._gameExePaths.Add(s);
                }
                hasChanges = true;
            }
            
            if (ListsAreDifferent(this._additionalDmodLocations, additionalDmodsLocations)) {
                this._additionalDmodLocations.Clear();
                foreach (string s in additionalDmodsLocations) {
                    this._additionalDmodLocations.Add(s);
                }
                hasChanges = true;
            }

            if (hasChanges) {
                this.FireUpdatedEvent();
            }
        }

        public void UpdateFromData(ConfigDataGeneral data) {
            bool hasChanges = false;
            
            // make sure paths are absolute before attempting to update
            data.ConvertPathsToAbsolute();
            
            if (data.LocalizationName != null && this.LocalizationName != data.LocalizationName) {
                this.LocalizationName = data.LocalizationName;
                hasChanges = true;
            }
            
            if (data.AutoUpdateInstallerList != null && this.AutoUpdateInstallerList != data.AutoUpdateInstallerList) {
                this.AutoUpdateInstallerList = (bool)data.AutoUpdateInstallerList;
                hasChanges = true;
            }
            
            if (data.ShowAdvancedFeatures != null && this.ShowAdvancedFeatures != data.ShowAdvancedFeatures) {
                this.ShowAdvancedFeatures = (bool)data.ShowAdvancedFeatures;
                hasChanges = true;
            }
            
            if (data.ShowLogWindowOnStartup != null && this.ShowLogWindowOnStartup != data.ShowLogWindowOnStartup) {
                this.ShowLogWindowOnStartup = (bool)data.ShowLogWindowOnStartup;
                hasChanges = true;
            }
            
            if (data.UseRelativePathForSubfolders != null && this.UseRelativePathForSubfolders != data.UseRelativePathForSubfolders) {
                this.UseRelativePathForSubfolders = (bool)data.UseRelativePathForSubfolders;
                hasChanges = true;
            }
            
            if (data.ActiveGameExeIndex != null && this.ActiveGameExeIndex != data.ActiveGameExeIndex) {
                this.ActiveGameExeIndex = (int)data.ActiveGameExeIndex;
                hasChanges = true;
            }
            
            if (data.DefaultDmodLocation != null && this.DefaultDmodLocation != data.DefaultDmodLocation) {
                this.DefaultDmodLocation = data.DefaultDmodLocation;
                hasChanges = true;
            }

            if (data.GameExePaths != null && ListsAreDifferent(this._gameExePaths, data.GameExePaths)) {
                this._gameExePaths.Clear();
                foreach (string s in data.GameExePaths) {
                    this._gameExePaths.Add(s);
                }
                hasChanges = true;
            }
            
            if (data.AdditionalDmodLocations != null && ListsAreDifferent(this._additionalDmodLocations, data.AdditionalDmodLocations)) {
                this._additionalDmodLocations.Clear();
                foreach (string s in data.AdditionalDmodLocations) {
                    this._additionalDmodLocations.Add(s);
                }
                hasChanges = true;
            }
            
            if (hasChanges) {
                this.FireUpdatedEvent();
            }
        }

        public ConfigDataGeneral GetData() {
            List<string> gameExePaths = new List<string>();
            List<string> additionalDmodLocations = new List<string>();
            
            foreach (string s in this._gameExePaths) {
                gameExePaths.Add(s);
            }
            
            foreach (string s in this._additionalDmodLocations) {
                additionalDmodLocations.Add(s);
            }

            ConfigDataGeneral data = new ConfigDataGeneral()  {
                LocalizationName = this.LocalizationName,
                AutoUpdateInstallerList = this.AutoUpdateInstallerList,
                ShowAdvancedFeatures = this.ShowAdvancedFeatures,
                ShowLogWindowOnStartup = this.ShowLogWindowOnStartup,
                UseRelativePathForSubfolders = this.UseRelativePathForSubfolders,
                ActiveGameExeIndex = this.ActiveGameExeIndex,
                GameExePaths = gameExePaths,
                DefaultDmodLocation = this.DefaultDmodLocation,
                AdditionalDmodLocations = additionalDmodLocations,
            };

            if (this.UseRelativePathForSubfolders) {
                data.ConvertPathsToRelative();
            }

            return data;
        }

        /// <summary>
        /// Gets a list of all the DMODS found in the currently defined locations
        /// </summary>
        /// <returns>A list of <see cref="DirectoryInfo"/> defining the locations of valid DMODs</returns>
        public List<DirectoryInfo> GetRealDmodDirectories() {
            Dictionary<string,DirectoryInfo> dict = new Dictionary<string, DirectoryInfo>();

            DirectoryInfo defaultDmods = new DirectoryInfo(this.DefaultDmodLocation);
            if (defaultDmods.Exists) {
                dict.Add(defaultDmods.FullName, defaultDmods);
            } else {
                try {
                    defaultDmods.Create();
                    defaultDmods.Refresh();
                    if (defaultDmods.Exists) {
                        dict.Add(defaultDmods.FullName, defaultDmods);
                    }
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.General, ex, MyTraceLevel.Warning);
                }
            }

            foreach (string location in this.AdditionalDmodLocations) {
                try {
                    DirectoryInfo dirInfo = new DirectoryInfo(location);
                    if (dirInfo.Exists && dict.ContainsKey(dirInfo.FullName) == false) {
                        dict.Add(dirInfo.FullName, dirInfo);
                    }
                } catch (Exception ex) {
                    MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Error evaluating possible DMOD location... \"{location}\"");
                    MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                }
            }

            foreach (string file in this.GameExePaths) {
                try {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Directory?.Exists == true && dict.ContainsKey(fileInfo.Directory.FullName) == false) {
                        dict.Add(fileInfo.Directory.FullName, fileInfo.Directory);
                    }
                } catch (Exception ex) {
                    MyTrace.Global.WriteMessage(MyTraceCategory.General, $"Error evaluating possible DMOD location... \"{file}\"");
                    MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                }
            }

            List<DirectoryInfo> directories = new List<DirectoryInfo>();
            foreach (var kvp in dict) {
                directories.Add(kvp.Value);
            }

            return directories;
        }
    }
}
