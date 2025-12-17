using System.Collections.Generic;
namespace Martridge.Models.Configuration.General.FileData {
    
    /// <summary>
    /// Class used for JSON serialization
    /// </summary>
    public class ConfigDataGeneralV2 {
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
        /// Override the default Citrus Dark theme with this
        /// </summary>
        public string? DarkThemeOverride { get; set; }

        /// <summary>
        /// Override the default Citrus Light theme with this
        /// </summary>
        public string? LightThemeOverride { get; set; }

        /// <summary>
        /// Indicates if the application should enable certain advanced features...
        /// </summary>
        /// <remarks>NOTE: Not really used much right now...</remarks>
        public bool? ShowDmodDevFeatures { get; set; }
        
        /// <summary>
        /// Indicates if the application should enable online features...
        /// </summary>
        public bool? EnableOnlineFeatures { get; set; }

        /// <summary>
        /// If the value is greater than 0 and the online DMOD list is older than this amount of days, it will be auto refreshed.
        /// </summary>
        public double? OnlineDmodListAutoRefreshDays { get; set; }

        /// <summary>
        /// How long it takes in seconds for a HTTP request to time out
        /// </summary>
        public double? OnlineWebCrawlerHttpTimeoutSeconds { get; set; }

        /// <summary>
        /// How long it takes for a 2nd HTTP request to be dropped while waiting for a previous one to complete
        /// </summary>
        public double? OnlineWebCrawlerBusyTimeoutSeconds { get; set; }

        /// <summary>
        /// If true, displays the --refdir path launch config in the main window in the selected dmod view
        /// </summary>
        public bool? ShowLaunchRefDirPathInMainWindow { get; set; }

        /// <summary>
        /// If true, displays the custom args launch config in the main window in the selected dmod view
        /// </summary>
        public bool? ShowLaunchCustomArgsInMainWindow { get; set; }
        
        /// <summary>
        /// Indicates if the application should use relative paths for paths related to its subfolders.
        /// This helps with keeping the application portable...
        /// </summary>
        public bool? UseRelativePathForSubfolders { get; set; }
        
        /// <summary>
        /// The source URL or file for the DinkInstaller config json file
        /// </summary>
        public string? DinkInstallerConfigFileSource { get; set; }
        
        /// <summary>
        /// Maximum number of logs to keep, oldest will be deleted on startup
        /// </summary>
        public int? MaxLogsToKeep { get; set; }
        
        /// <summary>
        /// Last selected game executable for launching Dink
        /// </summary>
        public int? ActiveGameExeIndex { get; set; }
        
        /// <summary>
        /// Last selected editor executable for editing DMODs
        /// </summary>
        public int? ActiveEditorExeIndex { get; set; }

        /// <summary>
        /// List of paths to Dink executable files, Martridge will also scan the `DMOD`/`DMODS` subfolders at these locations 
        /// </summary>
        public List<string>? GameExePaths { get; set; }
        
        /// <summary>
        /// List of paths to Dink editor executable files
        /// </summary>
        public List<string>? EditorExePaths { get; set; }
        
        /// <summary>
        /// List of additional directories to scan for DMODS or install DMODS in (formerly known as AdditionalDmodLocations)
        /// </summary>
        public List<string>? DmodPaths { get; set; }
        
        /// <summary>
        /// If ture, will decompress DMOD tar archives from bzip2 to a memory stream instead of a temporary file
        /// </summary>
        public bool? DecompressDmodsToMemoryStreamInsteadOfTemporaryFile { get; set; }

        public Dictionary<string, object?> GetValues() {
            return new Dictionary<string, object?>() {
                [nameof(ConfigGeneral.ThemeName)] = this.ThemeName,
                [nameof(ConfigGeneral.DarkThemeOverride)] = this.DarkThemeOverride,
                [nameof(ConfigGeneral.LightThemeOverride)] = this.LightThemeOverride,
                [nameof(ConfigGeneral.LocalizationName)] = this.LocalizationName,
                [nameof(ConfigGeneral.ShowDmodDevFeatures)] = this.ShowDmodDevFeatures,
                [nameof(ConfigGeneral.EnableOnlineFeatures)] = this.EnableOnlineFeatures,
                [nameof(ConfigGeneral.OnlineDmodListAutoRefreshDays)] = this.OnlineDmodListAutoRefreshDays,
                [nameof(ConfigGeneral.OnlineWebCrawlerHttpTimeoutSeconds)] = this.OnlineWebCrawlerHttpTimeoutSeconds,
                [nameof(ConfigGeneral.OnlineWebCrawlerBusyTimeoutSeconds)] = this.OnlineWebCrawlerBusyTimeoutSeconds,
                [nameof(ConfigGeneral.ShowLaunchRefDirPathInMainWindow)] = this.ShowLaunchRefDirPathInMainWindow,
                [nameof(ConfigGeneral.ShowLaunchCustomArgsInMainWindow)] = this.ShowLaunchCustomArgsInMainWindow,
                [nameof(ConfigGeneral.UseRelativePathForSubfolders)] = this.UseRelativePathForSubfolders,
                [nameof(ConfigGeneral.ActiveGameExeIndex)] = this.ActiveGameExeIndex,
                [nameof(ConfigGeneral.ActiveEditorExeIndex)] = this.ActiveEditorExeIndex,
                [nameof(ConfigGeneral.GameExePaths)] = this.GameExePaths,
                [nameof(ConfigGeneral.EditorExePaths)] = this.EditorExePaths,
                [nameof(ConfigGeneral.DmodPaths)] = this.DmodPaths,
                [nameof(ConfigGeneral.DinkInstallerConfigFileSource)] = this.DinkInstallerConfigFileSource,
                [nameof(ConfigGeneral.MaxLogsToKeep)] = this.MaxLogsToKeep,
                [nameof(ConfigGeneral.DecompressDmodsToMemoryStreamInsteadOfTemporaryFile)] = this.DecompressDmodsToMemoryStreamInsteadOfTemporaryFile,
            };
        }
    }
}
