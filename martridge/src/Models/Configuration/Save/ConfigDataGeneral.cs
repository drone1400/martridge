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
        /// Indicates if the application should enable certain advanced features...
        /// </summary>
        /// <remarks>NOTE: Not really used much right now...</remarks>
        public bool? ShowAdvancedFeatures { get; set; }
        
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
        /// List of paths to Dink executable files
        /// </summary>
        public List<string>? GameExePaths { get; set; }
        
        /// <summary>
        /// The default location where DMODS should get installed by the application
        /// </summary>
        public string? DefaultDmodLocation { get; set; }
        
        /// <summary>
        /// List of additional directories to scan for DMODS
        /// </summary>
        public List<string>? AdditionalDmodLocations { get; set; }


        
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
        }
    }
}
