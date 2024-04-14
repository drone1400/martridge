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
        private string _themeName = ApplicationTheme.Citrus.ToString();

        /// <summary>
        /// Indicates if the application should automatically update the existing 'configInstallerList.json' file
        /// </summary>
        public bool AutoUpdateInstallerList { get => this._autoUpdateInstallerList; }
        private bool _autoUpdateInstallerList = true;


        /// <summary>
        /// Indicates if the application should enable certain advanced features...
        /// </summary>
        /// <remarks>NOTE: Not really used much right now...</remarks>
        public bool ShowDmodDevFeatures { get => this._ShowDmodDevFeatures; }
        private bool _ShowDmodDevFeatures = true;

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
        private bool _useRelativePathForSubfolders = true;

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
        private string _defaultDmodLocation = LocationHelper.TryGetAbsoluteFromSubdirectoryRelative("DMODS");

        /// <summary>
        /// List of additional directories to scan for DMODS
        /// </summary>
        public ReadOnlyCollection<string> AdditionalDmodLocations { get; }
        private readonly List<string> _additionalDmodLocations = new List<string>();

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

        public void AddGameExePath(string path) {
            if (this._gameExePaths.Contains(path) || string.IsNullOrWhiteSpace(path)) return;

            this._gameExePaths.Add(path);
            this.FireUpdatedEvent(nameof(this.GameExePaths));
        }

        public void AddAdditionalDmodPath(string path) {
            if (this._additionalDmodLocations.Contains(path) || string.IsNullOrWhiteSpace(path)) return;

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

            void TryUpdatePathList(KeyValuePair<string, object?> kvp, List<string> myValues) {
                if (kvp.Value is List<string> list) {
                    List<string> listAbs = LocationHelper.TryGetAbsoluteFromSubdirectoryRelative(list);
                    if (ListsAreDifferent(myValues, listAbs)) {
                        myValues.Clear();
                        foreach (string s in list) {
                            if (string.IsNullOrWhiteSpace(s) == false) {
                                myValues.Add(s);
                            }
                        }
                        updatedProperties.Add(kvp.Key);
                    }
                }
            }

            foreach (var kvp in newValues) {
                switch (kvp.Key) {
                    case nameof(this.ThemeName): TryUpdateGeneric(kvp, ref this._themeName); break;
                    case nameof(this.LocalizationName): TryUpdateGeneric(kvp, ref this._localizationName); break;
                    case nameof(this.AutoUpdateInstallerList): TryUpdateGeneric(kvp, ref this._autoUpdateInstallerList); break;
                    case nameof(this.ShowDmodDevFeatures): TryUpdateGeneric(kvp, ref this._ShowDmodDevFeatures); break;
                    case nameof(this.ShowLogWindowOnStartup): TryUpdateGeneric(kvp, ref this._showLogWindowOnStartup); break;
                    case nameof(this.UseRelativePathForSubfolders): TryUpdateGeneric(kvp, ref this._useRelativePathForSubfolders); break;
                    case nameof(this.ActiveGameExeIndex): TryUpdateGeneric(kvp, ref this._activeGameExeIndex); break;
                    case nameof(this.ActiveEditorExeIndex): TryUpdateGeneric(kvp, ref this._activeEditorExeIndex); break;
                    case nameof(this.DefaultDmodLocation): TryUpdateGeneric(kvp, ref this._defaultDmodLocation); break;
                    
                    case nameof(this.GameExePaths): TryUpdatePathList(kvp, this._gameExePaths); break;
                    case nameof(this.EditorExePaths): TryUpdatePathList(kvp, this._editorExePaths); break;
                    case nameof(this.AdditionalDmodLocations): TryUpdatePathList(kvp, this._additionalDmodLocations); break;
                }
            }
            
            if (updatedProperties.Count > 0) {
                this.FireUpdatedEvent(updatedProperties);
            }
        }

        public ConfigDataGeneral GetData() {
                List<string> gameExePaths = new List<string>();
                List<string> editorExePaths = new List<string>();
                List<string> additionalDmodLocations = new List<string>();

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
                    LocalizationName = this.LocalizationName,
                    AutoUpdateInstallerList = this.AutoUpdateInstallerList,
                    ShowDmodDevFeatures = this.ShowDmodDevFeatures,
                    ShowLogWindowOnStartup = this.ShowLogWindowOnStartup,
                    UseRelativePathForSubfolders = this.UseRelativePathForSubfolders,
                    ActiveGameExeIndex = this.ActiveGameExeIndex,
                    GameExePaths = gameExePaths,
                    ActiveEditorExeIndex = this.ActiveEditorExeIndex,
                    EditorExePaths = editorExePaths,
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
                Dictionary<string, DirectoryInfo> dict = new Dictionary<string, DirectoryInfo>();

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
