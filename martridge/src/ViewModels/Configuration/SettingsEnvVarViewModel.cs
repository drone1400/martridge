using Martridge.Models.Configuration.Generic;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Localization;
using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public class SettingsEnvVarViewModel : SettingsEnvVarViewModelBase {
        public SettingsEnvVarViewModel() {}
        
        public SettingsEnvVarViewModel(string key, string value, bool isEnabled) {
            this._key = key;
            this._value = value;
            this._isEnabled = isEnabled;

            this._description = Localizer.Instance.TryGetValue("SettingsWineView/EnvVar/Description/" + this._key, out string description)
                ? description
                : string.Empty;
        }

        public override ConfigDataEnvironmentVariable GetConfigData() {
            return new ConfigDataEnvironmentVariable() {
                Key = this.Key,
                Value = this.Value,
                IsEnabled = this.IsEnabled,
                Mode = nameof(ConfigEnvVarMode.Normal),
            };
        }

        public override ConfigEnvironmentVariable GetConfig() {
            return new ConfigEnvironmentVariable(
                key: this.Key,
                value: this.Value,
                isEnabled: this.IsEnabled,
                mode: ConfigEnvVarMode.Normal);
        }
    }
}
