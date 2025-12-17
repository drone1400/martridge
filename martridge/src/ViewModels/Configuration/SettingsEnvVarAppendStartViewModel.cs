using Martridge.Models.Configuration.Generic;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Localization;
using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public class SettingsEnvVarAppendStartViewModel : SettingsEnvVarViewModelBase {
        /// <summary>
        /// Will use this string to separate current value and appended value when appending
        /// </summary>
        public string AppendSeparator {
            get => this._appendAppendSeparator;
            set => this.RaiseAndSetIfChanged(ref this._appendAppendSeparator, value);
        }
        private string _appendAppendSeparator = ":";
        
        public SettingsEnvVarAppendStartViewModel() {}
        public SettingsEnvVarAppendStartViewModel(string key, string value, bool isEnabled, string appendSeparator) {
            this._key = key;
            this._value = value;
            this._isEnabled = isEnabled;
            this._appendAppendSeparator = appendSeparator;

            this._description = Localizer.Instance.TryGetValue("SettingsWineView/EnvVar/Description/" + this._key, out string description)
                ? description
                : string.Empty;
        }

        public override ConfigDataEnvironmentVariable GetConfigData() {
            return new ConfigDataEnvironmentVariable() {
                Key = this.Key,
                Value = this.Value,
                IsEnabled = this.IsEnabled,
                Mode = nameof(ConfigEnvVarMode.AppendStart),
                AppendSeparator = this._appendAppendSeparator,
            };
        }

        public override ConfigEnvironmentVariable GetConfig() {
            return new ConfigEnvironmentVariable(
                key: this.Key,
                value: this.Value,
                isEnabled: this.IsEnabled,
                mode: ConfigEnvVarMode.AppendStart,
                appendSeparator: this._appendAppendSeparator);
        }
    }
}
