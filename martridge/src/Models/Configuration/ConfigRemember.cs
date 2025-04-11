using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Martridge.Models.Configuration.Save;
namespace Martridge.Models.Configuration {
    public class ConfigRemember : IConfigGeneric{

        public event EventHandler<ConfigUpdateEventArgs>? Updated;

        /// <summary>
        /// Last used DMOD source path for Install DMOD
        /// </summary>
        public string InstallDmodSourcePath => this._installDmodSourcePath;
        private string _installDmodSourcePath = string.Empty;

        /// <summary>
        /// Last used DMOD base destination location for Install DMOD
        /// </summary>
        public string InstallDmodDestinationBaseDirectory => this._installDmodDestinationBaseDirectory;
        private string _installDmodDestinationBaseDirectory = string.Empty;
        
        /// <summary>
        /// Last used DMOD source path for Pack DMOD
        /// </summary>
        public string PackDmodSourcePath => this._packDmodSourcePath;
        private string _packDmodSourcePath = string.Empty;

        /// <summary>
        /// Last used DMOD destination path for Pack DMOD
        /// </summary>
        public string PackDmodDestinationPath => this._packDmodDestinationPath;
        private string _packDmodDestinationPath = string.Empty;
        
        /// <summary>
        /// Last selected DMOD in the DMOD browser
        /// </summary>
        public string DmodBrowserSelectedDmodPath => this._dmodBrowserSelectedDmodPath;
        private string _dmodBrowserSelectedDmodPath = string.Empty;

        //
        // Main window state
        //
        
        public WindowState MainWindowState => this._mainWindowState;
        private WindowState _mainWindowState = WindowState.Normal;

        public double MainWindowWidth => this._mainWindowWidth;
        private double _mainWindowWidth = 0;
        
        public double MainWindowHeight => this._mainWindowHeight;
        private double _mainWindowHeight = 0;

        public int MainWindowPositionX => this._mainWindowPositionX;
        private int _mainWindowPositionX = 0;
        
        public int MainWindowPositionY => this._mainWindowPositionY;
        private int _mainWindowPositionY = 0;
        
        //
        // Log window state
        //
        
        public WindowState LogWindowState => this._logWindowState;
        private WindowState _logWindowState = WindowState.Normal;

        public double LogWindowWidth => this._logWindowWidth;
        private double _logWindowWidth = 0;
        
        public double LogWindowHeight => this._logWindowHeight;
        private double _logWindowHeight = 0;

        public int LogWindowPositionX => this._logWindowPositionX;
        private int _logWindowPositionX = 0;
        
        public int LogWindowPositionY => this._logWindowPositionY;
        private int _logWindowPositionY = 0;

        public bool LogWindowShowOnStartup => this._logWindowShowOnStatup; 
        private bool _logWindowShowOnStatup = false;
        
        public void UpdateProperties(Dictionary<string, object?> newValues) {
            List<string> updatedProperties = new List<string>();

            void TryUpdateGeneric<T>(KeyValuePair<string, object?> kvp, ref T myValue) {
                if (kvp.Value is T value && myValue!.Equals(value) == false) {
                    myValue = value;
                    updatedProperties.Add(kvp.Key);
                }
            }

            foreach (var kvp in newValues) {
                switch (kvp.Key) {
                    case nameof(this.InstallDmodSourcePath): TryUpdateGeneric(kvp, ref this._installDmodSourcePath); break;
                    case nameof(this.InstallDmodDestinationBaseDirectory): TryUpdateGeneric(kvp, ref this._installDmodDestinationBaseDirectory); break;
                    case nameof(this.PackDmodSourcePath): TryUpdateGeneric(kvp, ref this._packDmodSourcePath); break;
                    case nameof(this.PackDmodDestinationPath): TryUpdateGeneric(kvp, ref this._packDmodDestinationPath); break;
                    case nameof(this.DmodBrowserSelectedDmodPath): TryUpdateGeneric(kvp, ref this._dmodBrowserSelectedDmodPath); break;
                    // main window
                    case nameof(this.MainWindowState): TryUpdateGeneric(kvp, ref this._mainWindowState); break;
                    case nameof(this.MainWindowWidth): TryUpdateGeneric(kvp, ref this._mainWindowWidth); break;
                    case nameof(this.MainWindowHeight): TryUpdateGeneric(kvp, ref this._mainWindowHeight); break;
                    case nameof(this.MainWindowPositionX): TryUpdateGeneric(kvp, ref this._mainWindowPositionX); break;
                    case nameof(this.MainWindowPositionY): TryUpdateGeneric(kvp, ref this._mainWindowPositionY); break;
                    // log window
                    case nameof(this.LogWindowState): TryUpdateGeneric(kvp, ref this._logWindowState); break;
                    case nameof(this.LogWindowWidth): TryUpdateGeneric(kvp, ref this._logWindowWidth); break;
                    case nameof(this.LogWindowHeight): TryUpdateGeneric(kvp, ref this._logWindowHeight); break;
                    case nameof(this.LogWindowPositionX): TryUpdateGeneric(kvp, ref this._logWindowPositionX); break;
                    case nameof(this.LogWindowPositionY): TryUpdateGeneric(kvp, ref this._logWindowPositionY); break;
                    case nameof(this.LogWindowShowOnStartup): TryUpdateGeneric(kvp, ref this._logWindowShowOnStatup); break;
                }
            }
            
            if (updatedProperties.Count > 0) {
                this.FireUpdatedEvent(updatedProperties);
            }
        }
        
        private void FireUpdatedEvent(List<string> updatedProperties) {
            this.Updated?.Invoke(this, new ConfigUpdateEventArgs(updatedProperties));
        }

        public ConfigDataRemember GetData() {
            return new ConfigDataRemember() {
                InstallDmodSourcePath = this.InstallDmodSourcePath,
                InstallDmodDestinationBaseDirectory = this.InstallDmodDestinationBaseDirectory,
                PackDmodSourcePath = this.PackDmodSourcePath,
                PackDmodDestinationPath = this.PackDmodDestinationPath,
                DmodBrowserSelectedDmodPath = this.DmodBrowserSelectedDmodPath,
                
                MainWindowState = this.MainWindowState.ToString(),
                MainWindowWidth = this.MainWindowWidth,
                MainWindowHeight = this.MainWindowHeight,
                MainWindowPositionX = this.MainWindowPositionX,
                MainWindowPositionY = this.MainWindowPositionY,
                
                LogWindowState = this.LogWindowState.ToString(),
                LogWindowWidth = this.LogWindowWidth,
                LogWindowHeight = this.LogWindowHeight,
                LogWindowPositionX = this.LogWindowPositionX,
                LogWindowPositionY = this.LogWindowPositionY,
                LogWindowShowOnStartup = this.LogWindowShowOnStartup,
            };
        }
    }
}
