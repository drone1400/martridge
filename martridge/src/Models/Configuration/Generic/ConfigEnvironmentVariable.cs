using System;
using Martridge.Models.Configuration.Generic.FileData;

namespace Martridge.Models.Configuration.Generic {
    public class ConfigEnvironmentVariable {
        /// <summary>
        /// The name of the environment variable
        /// </summary>
        public string Key { get; } = string.Empty;
        
        /// <summary>
        /// The value of the environment variable
        /// </summary>
        public string Value { get; } = string.Empty;

        /// <summary>
        /// If true, Martridge will use this environment variable when launching
        /// </summary>
        public bool IsEnabled { get; } = true;

        public ConfigEnvVarMode Mode { get; } = ConfigEnvVarMode.Normal;
        
        /// <summary>
        /// Will use this string to separate current value and appended value when appending
        /// </summary>
        public string AppendSeparator { get; } = ":";
        
        public ConfigEnvironmentVariable() { }
        public ConfigEnvironmentVariable(string key, string value, bool isEnabled) {
            this.Key = key;
            this.Value = value;
            this.IsEnabled = isEnabled;
        }
        public ConfigEnvironmentVariable(string key, string value, bool isEnabled, ConfigEnvVarMode mode = ConfigEnvVarMode.Normal, string appendSeparator = ":") {
            this.Key = key;
            this.Value = value;
            this.IsEnabled = isEnabled;
            this.Mode = mode;
            this.AppendSeparator = appendSeparator;
        }
        public ConfigEnvironmentVariable(ConfigDataEnvironmentVariable data) {
            this.Key = data.Key ?? string.Empty;
            this.Value = data.Value ?? string.Empty;
            this.IsEnabled = data.IsEnabled ?? true;
            this.Mode = data.Mode != null && Enum.TryParse(data.Mode, true, out ConfigEnvVarMode mode)
                ? mode
                : ConfigEnvVarMode.Normal;
            this.AppendSeparator = data.AppendSeparator ?? ":";
        }

        public ConfigDataEnvironmentVariable GetData() {
            switch (this.Mode) {
                default:
                case ConfigEnvVarMode.Normal:
                    return new ConfigDataEnvironmentVariable() {
                        Key = this.Key,
                        Value = this.Value,
                        IsEnabled = this.IsEnabled,
                        Mode = this.Mode.ToString(),
                    };
                case ConfigEnvVarMode.AppendStart:
                case ConfigEnvVarMode.AppendEnd:
                    return new ConfigDataEnvironmentVariable() {
                        Key = this.Key,
                        Value = this.Value,
                        IsEnabled = this.IsEnabled,
                        Mode = this.Mode.ToString(),
                        AppendSeparator = this.AppendSeparator,
                    };
            }
        }
    }
}
