using System;
using System.Collections.Generic;
using Avalonia.Controls;
namespace Martridge.Models.Configuration.Save {
    public class ConfigDataRemember {
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
                [nameof(ConfigRemember.InstallDmodSourcePath)] = this.InstallDmodSourcePath,
                [nameof(ConfigRemember.InstallDmodDestinationBaseDirectory)] = this.InstallDmodDestinationBaseDirectory,
                [nameof(ConfigRemember.PackDmodSourcePath)] = this.PackDmodSourcePath,
                [nameof(ConfigRemember.PackDmodDestinationPath)] = this.PackDmodDestinationPath,
                [nameof(ConfigRemember.DmodBrowserSelectedDmodPath)] = this.DmodBrowserSelectedDmodPath,
                // Main Window
                [nameof(ConfigRemember.MainWindowState)] = mainWindowState,
                [nameof(ConfigRemember.MainWindowWidth)] = this.MainWindowWidth,
                [nameof(ConfigRemember.MainWindowHeight)] = this.MainWindowHeight,
                [nameof(ConfigRemember.MainWindowPositionX)] = this.MainWindowPositionX,
                [nameof(ConfigRemember.MainWindowPositionY)] = this.MainWindowPositionY,
                // Log Window
                [nameof(ConfigRemember.LogWindowState)] = logWindowState,
                [nameof(ConfigRemember.LogWindowWidth)] = this.LogWindowWidth,
                [nameof(ConfigRemember.LogWindowHeight)] = this.LogWindowHeight,
                [nameof(ConfigRemember.LogWindowPositionX)] = this.LogWindowPositionX,
                [nameof(ConfigRemember.LogWindowPositionY)] = this.LogWindowPositionY,
                [nameof(ConfigRemember.LogWindowShowOnStartup)] = this.LogWindowShowOnStartup,
            };
        }
    }
}
