using System;
using System.Collections.Generic;
using Avalonia.Controls;
namespace Martridge.Models.Configuration.AppState.FileData {
    public class ConfigDataAppState {
        /// <summary>
        /// Last used DMOD source path for Install DMOD
        /// </summary>
        public string? InstallDmodSourcePath { get; set; }

        /// <summary>
        /// Last used DMOD base destination location for Install DMOD
        /// </summary>
        public string? InstallDmodDestinationBaseDirectory { get; set; }
        
        /// <summary>
        /// Last used DMOD source path for Pack DMOD
        /// </summary>
        public string? PackDmodSourcePath { get; set; }

        /// <summary>
        /// Last used DMOD destination path for Pack DMOD
        /// </summary>
        public string? PackDmodDestinationPath { get; set; }
        
        /// <summary>
        /// Last selected DMOD in the DMOD browser
        /// </summary>
        public string? DmodBrowserSelectedDmodPath { get; set; }
        
        /// <summary>
        /// Last set left panel column width in star units for the DMOD Browser 
        /// </summary>
        public double DmodBrowserLeftPanelColumnWidth { get; set; }
        
        /// <summary>
        /// Last set right panel column width in star units for the DMOD Browser 
        /// </summary>
        public double DmodBrowserRightPanelColumnWidth { get; set; }
        
        /// <summary>
        /// Last set left panel column width in star units for the DMOD Online Browser 
        /// </summary>
        public double OnlineDmodBrowserLeftPanelColumnWidth { get; set; }
        
        /// <summary>
        /// Last set right panel column width in star units for the DMOD Online Browser 
        /// </summary>
        public double OnlineDmodBrowserRightPanelColumnWidth { get; set; }
        
        //
        // Main window state
        //

        public string? MainWindowState { get; set; }
        public double? MainWindowWidth { get; set; }
        public double? MainWindowHeight { get; set; }
        public int? MainWindowPositionX { get; set; }
        public int? MainWindowPositionY { get; set; }

        //
        // Log window state
        //
        
        public string? LogWindowState { get; set; }
        public double? LogWindowWidth { get; set; }
        public double? LogWindowHeight { get; set; }
        public int? LogWindowPositionX { get; set; }
        public int? LogWindowPositionY { get; set; }

        public bool? LogWindowShowOnStartup { get; set; }

        public Dictionary<string, object?> GetValues() {
            if (Enum.TryParse(this.MainWindowState, true, out WindowState mainWindowState) == false) mainWindowState = WindowState.Normal;
            if (Enum.TryParse(this.LogWindowState, true, out WindowState logWindowState) == false) logWindowState = WindowState.Normal;
            
            return new Dictionary<string, object?>() {
                [nameof(ConfigAppState.InstallDmodSourcePath)] = this.InstallDmodSourcePath,
                [nameof(ConfigAppState.InstallDmodDestinationBaseDirectory)] = this.InstallDmodDestinationBaseDirectory,
                [nameof(ConfigAppState.PackDmodSourcePath)] = this.PackDmodSourcePath,
                [nameof(ConfigAppState.PackDmodDestinationPath)] = this.PackDmodDestinationPath,
                [nameof(ConfigAppState.DmodBrowserSelectedDmodPath)] = this.DmodBrowserSelectedDmodPath,
                [nameof(ConfigAppState.DmodBrowserLeftPanelColumnWidth)] = this.DmodBrowserLeftPanelColumnWidth,
                [nameof(ConfigAppState.DmodBrowserRightPanelColumnWidth)] = this.DmodBrowserRightPanelColumnWidth,
                [nameof(ConfigAppState.OnlineDmodBrowserLeftPanelColumnWidth)] = this.OnlineDmodBrowserLeftPanelColumnWidth,
                [nameof(ConfigAppState.OnlineDmodBrowserRightPanelColumnWidth)] = this.OnlineDmodBrowserRightPanelColumnWidth,
                // Main Window
                [nameof(ConfigAppState.MainWindowState)] = mainWindowState,
                [nameof(ConfigAppState.MainWindowWidth)] = this.MainWindowWidth,
                [nameof(ConfigAppState.MainWindowHeight)] = this.MainWindowHeight,
                [nameof(ConfigAppState.MainWindowPositionX)] = this.MainWindowPositionX,
                [nameof(ConfigAppState.MainWindowPositionY)] = this.MainWindowPositionY,
                // Log Window
                [nameof(ConfigAppState.LogWindowState)] = logWindowState,
                [nameof(ConfigAppState.LogWindowWidth)] = this.LogWindowWidth,
                [nameof(ConfigAppState.LogWindowHeight)] = this.LogWindowHeight,
                [nameof(ConfigAppState.LogWindowPositionX)] = this.LogWindowPositionX,
                [nameof(ConfigAppState.LogWindowPositionY)] = this.LogWindowPositionY,
                [nameof(ConfigAppState.LogWindowShowOnStartup)] = this.LogWindowShowOnStartup,
            };
        }
    }
}
