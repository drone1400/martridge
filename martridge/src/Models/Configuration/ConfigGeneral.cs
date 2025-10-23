using Martridge.Models.Configuration.Save;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Martridge.Models.Configuration {
    public class ConfigGeneral : IConfigGeneric {
        public event EventHandler<ConfigUpdateEventArgs>? Updated;

        /// <summary>
        /// The name of the localization table to use int he application.
        /// </summary>
        /// <remarks>
        /// Example: "en-US"
        /// </remarks>
        public string LocalizationName { get => this._localizationName; }
        private string _localizationName = "en-US";

        /// <summary>
        /// Name of the current Citrus.Avalonia theme
        /// </summary>
        public string ThemeName { get => this._themeName; }
        private string _themeName = "Citrus";

        /// <summary>
        /// Override the default Citrus Dark theme with this
        /// </summary>
        public string DarkThemeOverride => this._darkThemeOverride;
        private string _darkThemeOverride = string.Empty;
        
        /// <summary>
        /// Override the default Citrus Light theme with this
        /// </summary>
        public string LightThemeOverride => this._lightThemeOverride;
        private string _lightThemeOverride = string.Empty;


        /// <summary>
        /// Indicates if the application should enable certain advanced features...
        /// </summary>
        /// <remarks>NOTE: Not really used much right now...</remarks>
        public bool ShowDmodDevFeatures { get => this._ShowDmodDevFeatures; }
        private bool _ShowDmodDevFeatures = true;
        
        /// <summary>
        /// Indicates if the application should enable certain advanced features...
        /// </summary>
        /// <remarks>NOTE: Not really used much right now...</remarks>
        public bool EnableOnlineFeatures { get => this._EnableOnlineFeatures; }
        private bool _EnableOnlineFeatures = true;
        
        
        /// <summary>
        /// If true, displays the --refdir path launch config in the main window in the selected dmod view
        /// </summary>
        public bool ShowLaunchRefDirPathInMainWindow => this._showLaunchRefDirPathInMainWindow;
        private bool _showLaunchRefDirPathInMainWindow = true;

        /// <summary>
        /// If true, displays the custom args launch config in the main window in the selected dmod view
        /// </summary>
        public bool ShowLaunchCustomArgsInMainWindow => this._showLaunchCustomArgsInMainWindow;
        private bool _showLaunchCustomArgsInMainWindow = false;

        /// <summary>
        /// Indicates if the application should show the info/error log window when starting
        /// </summary>
        public bool ShowLogWindowOnStartup { get => this._showLogWindowOnStartup; }
        private bool _showLogWindowOnStartup = true;

        /// <summary>
        /// Indicates if the application should use relative paths for paths related to its subfolders.
        /// This helps with keeping the application portable...
        /// </summary>
        public bool UseRelativePathForSubfolders { get => this._useRelativePathForSubfolders; }
        private bool _useRelativePathForSubfolders = false;

        /// <summary>
        /// Last selected game executable for launching Dink
        /// </summary>
        public int ActiveGameExeIndex { get => this._activeGameExeIndex; }
        private int _activeGameExeIndex = -1;

        /// <summary>
        /// Last selected editor executable for launching Dink
        /// </summary>
        public int ActiveEditorExeIndex { get => this._activeEditorExeIndex; }
        private int _activeEditorExeIndex = -1;


        /// <summary>
        /// List of paths to Dink executable files
        /// </summary>
        public ReadOnlyCollection<string> GameExePaths { get; }
        private readonly List<string> _gameExePaths = new List<string>();

        /// <summary>
        /// List of paths to Dink editor executable files
        /// </summary>
        /// <remarks>
        /// For all intents and purposes, Editors are the same as the Game, but for user convenience, it makes sense to separate them.
        /// </remarks>
        public ReadOnlyCollection<string> EditorExePaths { get; }
        private readonly List<string> _editorExePaths = new List<string>();

        /// <summary>
        /// The default location where DMODS should get installed by the application
        /// </summary>
        public string DefaultDmodLocation { get => this._defaultDmodLocation; }
        private string _defaultDmodLocation = "./DMODS";

        /// <summary>
        /// List of additional directories to scan for DMODS
        /// </summary>
        public ReadOnlyCollection<string> AdditionalDmodLocations { get; }
        private readonly List<string> _additionalDmodLocations = new List<string>();

        /// <summary>
        /// The source URL or file for the DinkInstaller config json file
        /// </summary>
        public string DinkInstallerConfigFileSource { get => this._dinkInstallerConfigFileSource; }
        private string _dinkInstallerConfigFileSource = string.Empty;

        /// <summary>
        /// Maximum number of logs to keep, oldest will be deleted on startup
        /// </summary>
        public int MaxLogsToKeep { get => this._maxLogsToKeep; }
        private int _maxLogsToKeep = 25;

        public ConfigGeneral() {
            this.GameExePaths = new ReadOnlyCollection<string>(this._gameExePaths);
            this.EditorExePaths = new ReadOnlyCollection<string>(this._editorExePaths);
            this.AdditionalDmodLocations = new ReadOnlyCollection<string>(this._additionalDmodLocations);
        }

        private void FireUpdatedEvent(List<string> updatedProperties) {
            this.Updated?.Invoke(this, new ConfigUpdateEventArgs(updatedProperties));
        }

        private void FireUpdatedEvent(string updatedProperty) {
            this.Updated?.Invoke(this, new ConfigUpdateEventArgs(new List<string>() { updatedProperty } ));
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
        

        public void TryAddGameExePath(string? path) {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (LocationHelper.PathIsDuplicate(this._gameExePaths, path)) return;
            if (this.UseRelativePathForSubfolders) {
                path = LocationHelper.TryMakePathRelativeToMartridge(path);
            }
            this._gameExePaths.Add(path);
            this.FireUpdatedEvent(nameof(this.GameExePaths));
        }
        
        public void TryAddEditorExePath(string? path) {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (LocationHelper.PathIsDuplicate(this._editorExePaths, path)) return;
            if (this.UseRelativePathForSubfolders) {
                path = LocationHelper.TryMakePathRelativeToMartridge(path);
            }
            this._editorExePaths.Add(path);
            this.FireUpdatedEvent(nameof(this.EditorExePaths));
        }

        public void TryAddAdditionalDmodPath(string? path) {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (LocationHelper.PathIsDuplicate(this._additionalDmodLocations, path)) return;
            if (this.UseRelativePathForSubfolders) {
                path = LocationHelper.TryMakePathRelativeToMartridge(path);
            }
            this._additionalDmodLocations.Add(path);
            this.FireUpdatedEvent(nameof(this.AdditionalDmodLocations));
        }

        public void UpdateProperties(Dictionary<string, object?> newValues) {
            List<string> updatedProperties = new List<string>();

            void TryUpdateGeneric<T>(KeyValuePair<string, object?> kvp, ref T myValue) {
                if (kvp.Value is T value && myValue!.Equals(value) == false) {
                    myValue = value;
                    updatedProperties.Add(kvp.Key);
                }
            }

            void TryUpdatePathList(KeyValuePair<string, object?> kvp, List<string> myValues, bool relative) {
                if (kvp.Value is not List<string> list)
                    return;

                List<string> newList = new List<string>();

                foreach (string value in list) {
                    if (string.IsNullOrWhiteSpace(value) == false &&
                        LocationHelper.PathIsDuplicate(newList, value) == false) {
                        newList.Add(relative
                            ? LocationHelper.TryMakePathRelativeToMartridge(value)
                            : value
                        );
                    }
                }
                
                if (ListsAreDifferent(myValues, newList)) {
                    myValues.Clear();
                    foreach (string s in newList) {
                        myValues.Add(s);
                    }
                    updatedProperties.Add(kvp.Key);
                }
            }

            if (newValues.TryGetValue(nameof(this.UseRelativePathForSubfolders), out object? value) && value is bool boolValue) {
                this._useRelativePathForSubfolders = boolValue;
                updatedProperties.Add(nameof(this.UseRelativePathForSubfolders));
            }
            
            foreach (var kvp in newValues) {
                switch (kvp.Key) {
                    case nameof(this.ThemeName): TryUpdateGeneric(kvp, ref this._themeName); break;
                    case nameof(this.DarkThemeOverride): TryUpdateGeneric(kvp, ref this._darkThemeOverride); break;
                    case nameof(this.LightThemeOverride): TryUpdateGeneric(kvp, ref this._lightThemeOverride); break;
                    case nameof(this.LocalizationName): TryUpdateGeneric(kvp, ref this._localizationName); break;
                    case nameof(this.ShowDmodDevFeatures): TryUpdateGeneric(kvp, ref this._ShowDmodDevFeatures); break;
                    case nameof(this.EnableOnlineFeatures): TryUpdateGeneric(kvp, ref this._EnableOnlineFeatures); break;
                    case nameof(this.ShowLaunchRefDirPathInMainWindow): TryUpdateGeneric(kvp, ref this._showLaunchRefDirPathInMainWindow); break;
                    case nameof(this.ShowLaunchCustomArgsInMainWindow): TryUpdateGeneric(kvp, ref this._showLaunchCustomArgsInMainWindow); break;
                    case nameof(this.ShowLogWindowOnStartup): TryUpdateGeneric(kvp, ref this._showLogWindowOnStartup); break;
                    // case nameof(this.UseRelativePathForSubfolders): TryUpdateGeneric(kvp, ref this._useRelativePathForSubfolders); break; // already handled...
                    case nameof(this.ActiveGameExeIndex): TryUpdateGeneric(kvp, ref this._activeGameExeIndex); break;
                    case nameof(this.ActiveEditorExeIndex): TryUpdateGeneric(kvp, ref this._activeEditorExeIndex); break;
                    case nameof(this.DinkInstallerConfigFileSource): TryUpdateGeneric(kvp, ref this._dinkInstallerConfigFileSource); break;
                    case nameof(this.MaxLogsToKeep): TryUpdateGeneric(kvp, ref this._maxLogsToKeep); break;
                    
                    case nameof(this.DefaultDmodLocation): {
                        if (kvp.Value is string path) {
                            if (this._useRelativePathForSubfolders) {
                                path = LocationHelper.TryMakePathRelativeToMartridge(path);
                            }
                            this._defaultDmodLocation = path;
                            updatedProperties.Add(kvp.Key);
                        }
                        break;
                    }
                    
                    case nameof(this.GameExePaths): TryUpdatePathList(kvp, this._gameExePaths, this._useRelativePathForSubfolders); break;
                    case nameof(this.EditorExePaths): TryUpdatePathList(kvp, this._editorExePaths, this._useRelativePathForSubfolders); break;
                    case nameof(this.AdditionalDmodLocations): TryUpdatePathList(kvp, this._additionalDmodLocations, this._useRelativePathForSubfolders); break;
                }
            }
            
            if (updatedProperties.Count > 0) {
                this.FireUpdatedEvent(updatedProperties);
            }
        }

        public ConfigDataGeneral GetData() {
                string defaultDmodLocation;
                List<string> gameExePaths = new List<string>();
                List<string> editorExePaths = new List<string>();
                List<string> additionalDmodLocations = new List<string>();
                
                defaultDmodLocation = this._defaultDmodLocation;
                    
                foreach (string s in this._gameExePaths) {
                    gameExePaths.Add(s);
                }
                foreach (string s in this._editorExePaths) {
                    editorExePaths.Add(s);
                }
                foreach (string s in this._additionalDmodLocations) {
                    additionalDmodLocations.Add(s);
                }

                ConfigDataGeneral data = new ConfigDataGeneral()  {
                    ThemeName = this.ThemeName,
                    DarkThemeOverride = this.DarkThemeOverride,
                    LightThemeOverride = this.LightThemeOverride,
                    LocalizationName = this.LocalizationName,
                    ShowDmodDevFeatures = this.ShowDmodDevFeatures,
                    EnableOnlineFeatures = this.EnableOnlineFeatures,
                    ShowLaunchRefDirPathInMainWindow = this.ShowLaunchRefDirPathInMainWindow,
                    ShowLaunchCustomArgsInMainWindow = this.ShowLaunchCustomArgsInMainWindow,
                    ActiveGameExeIndex = this.ActiveGameExeIndex,
                    ActiveEditorExeIndex = this.ActiveEditorExeIndex,
                    GameExePaths = gameExePaths,
                    EditorExePaths = editorExePaths,
                    DefaultDmodLocation = defaultDmodLocation,
                    AdditionalDmodLocations = additionalDmodLocations,
                    DinkInstallerConfigFileSource = this.DinkInstallerConfigFileSource,
                    MaxLogsToKeep = this.MaxLogsToKeep,
                };

                return data;
            }

            /// <summary>
            /// Gets a list of all the DMODS found in the currently defined locations
            /// </summary>
            /// <returns>A list of <see cref="DirectoryInfo"/> defining the locations of valid DMODs</returns>
            public List<DirectoryInfo> GetRealDmodDirectories() {
                Dictionary<string, DirectoryInfo> dict = new Dictionary<string, DirectoryInfo>();

                string defaultDmodLocation = LocationHelper.TryMakePathAbsoluteBasedOnMartridge(this.DefaultDmodLocation);

                try {
                    DirectoryInfo defaultDmods = new DirectoryInfo(defaultDmodLocation);
                    if (defaultDmods.Exists) {
                        dict.Add(defaultDmods.FullName, defaultDmods);
                    }
                    else {
                        try {
                            defaultDmods.Create();
                            defaultDmods.Refresh();
                            if (defaultDmods.Exists) {
                                dict.Add(defaultDmods.FullName, defaultDmods);
                            }
                        } catch (Exception ex) {
                            MyTrace.Global.WriteException(ex, MyTraceLevel.Warning);
                        }
                    }
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(ex, MyTraceLevel.Warning);
                }

                foreach (string locationRaw in this.AdditionalDmodLocations) {
                    try {
                        string location = LocationHelper.TryMakePathAbsoluteBasedOnMartridge(locationRaw);
                        DirectoryInfo dirInfo = new DirectoryInfo(location);
                        if (dirInfo.Exists) {
                            dict.TryAdd(dirInfo.FullName, dirInfo);
                        }
                    } catch (Exception ex) {
                        MyTrace.Global.WriteMessage($"Error evaluating possible DMOD location... \"{locationRaw}\"");
                        MyTrace.Global.WriteException(ex);
                    }
                }

                foreach (string file in this.GameExePaths) {
                    try {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.Directory?.Exists == true && dict.ContainsKey(fileInfo.Directory.FullName) == false) {
                            dict.Add(fileInfo.Directory.FullName, fileInfo.Directory);
                        }
                    } catch (Exception ex) {
                        MyTrace.Global.WriteMessage($"Error evaluating possible DMOD location... \"{file}\"");
                        MyTrace.Global.WriteException(ex);
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
