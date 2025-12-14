using Martridge.Models.Configuration.Generic;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Localization;
using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public class SettingsEnvironmentVariableViewModel : ViewModelBase {
        /// <summary>
        /// The name of the environment variable
        /// </summary>
        public string Key {
            get => this._key;
            set => this.RaiseAndSetIfChanged(ref this._key, value);
        }
        private string _key = string.Empty;

        /// <summary>
        /// The value of the environment variable
        /// </summary>
        public string Value {
            get => this._value;
            set => this.RaiseAndSetIfChanged(ref this._value, value);
        }
        private string _value = string.Empty;

        /// <summary>
        /// If true, Martridge will use this environment variable when launching
        /// </summary>
        public bool IsEnabled {
            get => this._isEnabled;
            set => this.RaiseAndSetIfChanged(ref this._isEnabled, value);
        }
        private bool _isEnabled = true;

        /// <summary>
        /// If true, will append value to existing environment variable value when modifying
        /// </summary>
        public bool IsAppendMode {
            get => this._isAppendMode;
            set => this.RaiseAndSetIfChanged(ref this._isAppendMode, value);
        } 
        private bool _isAppendMode = false;

        /// <summary>
        /// If true, will append value at end of existing value, otherwise at start
        /// </summary>
        public bool IsAppendAtEnd {
            get => this._isAppendAtEnd;
            set => this.RaiseAndSetIfChanged(ref this._isAppendAtEnd, value);
        }
        private bool _isAppendAtEnd = false;

        /// <summary>
        /// Will use this string to separate current value and appended value when appending
        /// </summary>
        public string AppendSeparator {
            get => this._appendSeparator;
            set => this.RaiseAndSetIfChanged(ref this._appendSeparator, value);
        }
        private string _appendSeparator = ":";

        /// <summary>
        /// User description for environment variable
        /// </summary>
        public string Description {
            get => this._description;
            set => this.RaiseAndSetIfChanged(ref this._description, value);
        }
        private string _description = string.Empty;
        
        public SettingsEnvironmentVariableViewModel() {}
        public SettingsEnvironmentVariableViewModel(string name, string value) {
            this._key = name;
            this._value = value;

            this._description = Localizer.Instance.TryGetValue("EnvVar/Description/" + this._key, out string description)
                ? description
                : string.Empty;
        }

        public SettingsEnvironmentVariableViewModel(ConfigEnvironmentVariable data) {
            this._key = data.Key;
            this._value = data.Value;
            this._isEnabled = data.IsEnabled;
            this._isAppendMode = data.IsAppendMode;
            this._isAppendAtEnd = data.IsAppendAtEnd;
            this._appendSeparator = data.AppendSeparator;
            
            this._description = Localizer.Instance.TryGetValue("EnvVar/Description/" + this._key, out string description)
                ? description
                : string.Empty;
        }
        
        public SettingsEnvironmentVariableViewModel(ConfigDataEnvironmentVariable data) {
            this._key = data.Key ?? string.Empty;
            this._value = data.Value ?? string.Empty;
            this._isEnabled = data.IsEnabled ?? false;
            this._isAppendMode = data.IsAppendMode ?? false;
            this._isAppendAtEnd = data.IsAppendAtEnd ?? false;
            this._appendSeparator = data.AppendSeparator ?? ":";
            
            this._description = Localizer.Instance.TryGetValue("EnvVar/Description/" + this._key, out string description)
                ? description
                : string.Empty;
        }

        public ConfigDataEnvironmentVariable GetConfigData() {
            return new ConfigDataEnvironmentVariable() {
                Key = this.Key,
                Value = this.Value,
                IsEnabled = this.IsEnabled,
                IsAppendMode = this.IsAppendMode,
                IsAppendAtEnd = this.IsAppendAtEnd,
                AppendSeparator = this.AppendSeparator,
            };
        }

        public ConfigEnvironmentVariable GetConfig() {
            return new ConfigEnvironmentVariable(
                key: this.Key,
                value: this.Value,
                isEnabled: this.IsEnabled,
                isAppendMode: this.IsAppendMode,
                isAppendAtEnd: this.IsAppendAtEnd,
                appendSeparator: this.AppendSeparator);
        }
    }
}
