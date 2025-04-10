using System;
using System.Collections.Generic;
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
            };
        }
    }
}
