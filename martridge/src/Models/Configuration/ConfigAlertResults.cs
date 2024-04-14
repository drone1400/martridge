using Martridge.Models.Configuration.Save;
using Martridge.ViewModels.DinkyAlerts;
using System;
using System.Collections.Generic;

namespace Martridge.Models.Configuration {
    
    /// <summary>
    /// This class was used in a previous version to keep track of remembered results for popup-alert prompts...
    /// Those are no longer used currently, but I left it here in case it might be useful in the future I suppose?
    /// </summary>
    public class ConfigAlertResults : IConfigGeneric {
        public event EventHandler<ConfigUpdateEventArgs>? Updated;

        private readonly Dictionary<ConfigAlertResultMap, AlertResults> _rememberedResultMap;
        private readonly Dictionary<ConfigAlertResultMap, string> _propNameMap;
        private readonly Dictionary<string, ConfigAlertResultMap> _propNameMapReverse;

        public AlertResults Placeholder { get => this._rememberedResultMap[ConfigAlertResultMap.Placeholder]; }

        public ConfigAlertResults() {
            this._rememberedResultMap = new Dictionary<ConfigAlertResultMap, AlertResults>() {
                [ConfigAlertResultMap.Placeholder] = AlertResults.None,
            };

            this._propNameMap = new Dictionary<ConfigAlertResultMap, string>() {
                [ConfigAlertResultMap.Placeholder] = nameof(this.Placeholder),
            };

            this._propNameMapReverse = new Dictionary<string, ConfigAlertResultMap>();
            foreach (var kvp in this._propNameMap) {
                this._propNameMapReverse.Add(kvp.Value, kvp.Key);
            }
        }
        
        private void FireUpdatedEvent(List<string> updatedProperties) {
            this.Updated?.Invoke(this, new ConfigUpdateEventArgs(updatedProperties));
        }

        public AlertResults GetResult(ConfigAlertResultMap id) {
            if (this._rememberedResultMap.TryGetValue(id, out AlertResults result)) return result;
            return AlertResults.None;
        }

        public void SaveResult(ConfigAlertResultMap id, AlertResults result) {
            List<string> updatedProperties = new List<string>();

            if (this._rememberedResultMap.ContainsKey(id)) {
                if (this._rememberedResultMap[id] != result) {
                    this._rememberedResultMap[id] = result;
                    updatedProperties.Add(this._propNameMap[id]);
                }
            }

            if (updatedProperties.Count > 0) {
                this.FireUpdatedEvent(updatedProperties);
            }
        }
        
        public void UpdateProperties(Dictionary<string, object?> newValues) {
            List<string> updatedProperties = new List<string>();

            foreach (var kvp in newValues) {
                if (kvp.Value is string resultStr && Enum.TryParse(resultStr, out AlertResults result)) {
                    if (this._propNameMapReverse.TryGetValue(kvp.Key, out ConfigAlertResultMap idVal)) {

                        if (this._rememberedResultMap[idVal] != result) {
                            this._rememberedResultMap[idVal] = result;
                            updatedProperties.Add(this._propNameMap[idVal]);
                        }
                    }
                }
            }
            
            if (updatedProperties.Count > 0) {
                this.FireUpdatedEvent(updatedProperties);
            }
        }

        public ConfigDataAlertResults GetData() {
           
            ConfigDataAlertResults data = new ConfigDataAlertResults()  {
                Placeholder = this.Placeholder.ToString(),
            };

            return data;
        }
    }
}
