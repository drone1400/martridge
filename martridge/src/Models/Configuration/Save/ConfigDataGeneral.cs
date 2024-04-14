using System.Collections.Generic;

namespace Martridge.Models.Configuration.Save {
    
    /// <summary>
    /// Class used for JSON serialization
    /// </summary>
    public class ConfigDataGeneral {
        /// <summary>
        /// The name of the localization table to use int he application.
        /// </summary>
        /// <remarks>
        /// Example: "en-US"
        /// </remarks>
        public string? LocalizationName { get; set; }
        
        /// <summary>
        /// Name of the current Citrus.Avalonia theme
        /// </summary>
        public string? ThemeName { get; set; }
        
        /// <summary>
        /// Indicates if the application should automatically update the existing 'configInstallerList.json' file
        /// </summary>
        public bool? AutoUpdateInstallerList { get; set; }

        /// <summary>
        /// Indicates if the application should enable certain advanced features...
        /// </summary>
        /// <remarks>NOTE: Not really used much right now...</remarks>
        public bool? ShowDmodDevFeatures { get; set; }
        
        /// <summary>
        /// Indicates if the application should show the info/error log window when starting
        /// </summary>
        public bool? ShowLogWindowOnStartup { get; set; }
        
        /// <summary>
        /// Indicates if the application should use relative paths for paths related to its subfolders.
        /// This helps with keeping the application portable...
        /// </summary>
        public bool? UseRelativePathForSubfolders { get; set; }
        
        /// <summary>
        /// Last selected game executable for launching Dink
        /// </summary>
        public int? ActiveGameExeIndex { get; set; }
        
        /// <summary>
        /// Last selected editor executable for editing DMODs
        /// </summary>
        public int? ActiveEditorExeIndex { get; set; }

        /// <summary>
        /// List of paths to Dink executable files
        /// </summary>
        public List<string>? GameExePaths { get; set; }
        
        /// <summary>
        /// List of paths to Dink editor executable files
        /// </summary>
        public List<string>? EditorExePaths { get; set; }
        
        /// <summary>
        /// The default location where DMODS should get installed by the application
        /// </summary>
        public string? DefaultDmodLocation { get; set; }
        
        /// <summary>
        /// List of additional directories to scan for DMODS
        /// </summary>
        public List<string>? AdditionalDmodLocations { get; set; }

        public Dictionary<string, object?> GetValues() {
            return new Dictionary<string, object?>() {
                [nameof(ConfigGeneral.ThemeName)] = this.ThemeName,
                [nameof(ConfigGeneral.LocalizationName)] = this.LocalizationName,
                [nameof(ConfigGeneral.AutoUpdateInstallerList)] = this.AutoUpdateInstallerList,
                [nameof(ConfigGeneral.ShowDmodDevFeatures)] = this.ShowDmodDevFeatures,
                [nameof(ConfigGeneral.ShowLogWindowOnStartup)] = this.ShowLogWindowOnStartup,
                [nameof(ConfigGeneral.UseRelativePathForSubfolders)] = this.UseRelativePathForSubfolders,
                [nameof(ConfigGeneral.ActiveGameExeIndex)] = this.ActiveGameExeIndex,
                [nameof(ConfigGeneral.ActiveEditorExeIndex)] = this.ActiveEditorExeIndex,
                [nameof(ConfigGeneral.GameExePaths)] = this.GameExePaths,
                [nameof(ConfigGeneral.EditorExePaths)] = this.EditorExePaths,
                [nameof(ConfigGeneral.DefaultDmodLocation)] = this.DefaultDmodLocation,
                [nameof(ConfigGeneral.AdditionalDmodLocations)] = this.AdditionalDmodLocations,
            };
        }

        
        public void ConvertPathsToRelative() {
            if (this.DefaultDmodLocation != null) {
                this.DefaultDmodLocation = LocationHelper.TryGetRelativeSubdirectory(this.DefaultDmodLocation);
            }

            if (this.AdditionalDmodLocations != null) {
                List<string> newDmodLocations = new List<string>();
                foreach (string path in this.AdditionalDmodLocations) {
                    newDmodLocations.Add(LocationHelper.TryGetRelativeSubdirectory(path));
                }
                this.AdditionalDmodLocations = newDmodLocations;
            }
            
            if (this.GameExePaths != null) {
                List<string> newGameExePaths = new List<string>();
                foreach (string path in this.GameExePaths) {
                    newGameExePaths.Add(LocationHelper.TryGetRelativeSubdirectory(path));
                }
                this.GameExePaths = newGameExePaths;
            }
            
            if (this.EditorExePaths != null) {
                List<string> newEditorExePaths = new List<string>();
                foreach (string path in this.EditorExePaths) {
                    newEditorExePaths.Add(LocationHelper.TryGetRelativeSubdirectory(path));
                }
                this.EditorExePaths = newEditorExePaths;
            }
        }

        public void ConvertPathsToAbsolute() {
            if (this.DefaultDmodLocation != null) {
                this.DefaultDmodLocation = LocationHelper.TryGetAbsoluteFromSubdirectoryRelative(this.DefaultDmodLocation);
            }

            if (this.AdditionalDmodLocations != null) {
                List<string> newDmodLocations = new List<string>();
                foreach (string path in this.AdditionalDmodLocations) {
                    newDmodLocations.Add(LocationHelper.TryGetAbsoluteFromSubdirectoryRelative(path));
                }
                this.AdditionalDmodLocations = newDmodLocations;
            }
            
            if (this.GameExePaths != null) {
                List<string> newGameExePaths = new List<string>();
                foreach (string path in this.GameExePaths) {
                    newGameExePaths.Add(LocationHelper.TryGetAbsoluteFromSubdirectoryRelative(path));
                }
                this.GameExePaths = newGameExePaths;
            }
            
            if (this.EditorExePaths != null) {
                List<string> newEditorExePaths = new List<string>();
                foreach (string path in this.EditorExePaths) {
                    newEditorExePaths.Add(LocationHelper.TryGetAbsoluteFromSubdirectoryRelative(path));
                }
                this.EditorExePaths = newEditorExePaths;
            }
        }
    }
}
