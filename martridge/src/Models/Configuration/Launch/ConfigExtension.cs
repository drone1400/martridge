using System.Collections.Generic;
using System.Collections.ObjectModel;
using Martridge.Models.Configuration.LaunchExtension.FileData;
namespace Martridge.Models.Configuration.LaunchExtension {
    public class ConfigExtension {

        public IReadOnlyDictionary<string, ConfigExtensionComponent> Extensions {
            get;
        }
        private readonly Dictionary<string, ConfigExtensionComponent> _extensionsBase = new Dictionary<string, ConfigExtensionComponent>();


        public ConfigExtension() {
            this.Extensions = new ReadOnlyDictionary<string, ConfigExtensionComponent>(this._extensionsBase);
        }

        public ConfigExtensionComponent? TryAddOrGetExtension(string targetPath) {
            if (this._extensionsBase.TryGetValue(targetPath, out ConfigExtensionComponent? value))
                return value;

            ConfigExtensionComponent newComponent = new ConfigExtensionComponent(targetPath);
            if (this._extensionsBase.TryAdd(targetPath, newComponent)) 
                return newComponent;
            
            return null;
        }

        public ConfigExtensionComponent? TryGetExtension(string targetPath) {
            if (this._extensionsBase.TryGetValue(targetPath, out ConfigExtensionComponent? value))
                return value;
            
            return null;
        }

        public ConfigExtensionComponent? TryChangeTargetPath(string targetPathOld, string targetPathNew) {
            if (this._extensionsBase.TryGetValue(targetPathOld, out ConfigExtensionComponent? value) == false)
                return null;
            
            ConfigExtensionComponent newComponent = new ConfigExtensionComponent(targetPathNew);
            newComponent.SteamData = value.SteamData;
            newComponent.WineData = value.WineData;

            this._extensionsBase.Remove(targetPathOld);
            this._extensionsBase.Add(targetPathNew, newComponent);
            return newComponent;
        }

        public void SetFromData(ConfigFileDataExtensionV1? data) {
            this._extensionsBase.Clear();
            if (data?.ExtensionDefinitions == null)
                return;
            foreach (var def in  data.ExtensionDefinitions) {
                if (string.IsNullOrWhiteSpace(def.TargetExePath) ||
                    def.SteamData == null && def.WineData == null)
                    continue;
                
                ConfigExtensionComponent component = new ConfigExtensionComponent(def);
                this._extensionsBase.Add(component.TargetExePath, component);
            }
        }

        public ConfigFileDataExtensionV1 GetData() {
            ConfigFileDataExtensionV1 fileData = new ConfigFileDataExtensionV1() {
                ExtensionDefinitions = new List<ConfigDataExtensionComponent>()
            };
            foreach (var pair in this._extensionsBase) {
                if (string.IsNullOrWhiteSpace(pair.Value.TargetExePath) == false &&
                    (pair.Value.SteamData != null || pair.Value.WineData != null)) {
                    // only add if actually has data
                    fileData.ExtensionDefinitions.Add(pair.Value.GetData());
                }
            }
            return fileData;
        }
    }
}
