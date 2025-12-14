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

        /// <summary>
        /// If true, will append value to existing environment variable value when modifying
        /// </summary>
        public bool IsAppendMode { get; } = false;

        /// <summary>
        /// If true, will append value at end of existing value, otherwise at start
        /// </summary>
        public bool IsAppendAtEnd { get; } = false;
        
        /// <summary>
        /// Will use this string to separate current value and appended value when appending
        /// </summary>
        public string AppendSeparator { get; } = ":";
        
        public ConfigEnvironmentVariable() { }
        public ConfigEnvironmentVariable(string key, string value, bool isEnabled, bool isAppendMode = false, bool isAppendAtEnd = false, string appendSeparator = ":") {
            this.Key = key;
            this.Value = value;
            this.IsEnabled = isEnabled;
            this.IsAppendMode = isAppendMode;
            this.IsAppendAtEnd = isAppendAtEnd;
            this.AppendSeparator = appendSeparator;
        }
        public ConfigEnvironmentVariable(ConfigDataEnvironmentVariable data) {
            this.Key = data.Key ?? string.Empty;
            this.Value = data.Value ?? string.Empty;
            this.IsEnabled = data.IsEnabled ?? true;
            this.IsAppendMode = data.IsAppendMode ?? false;
            this.IsAppendAtEnd = data.IsAppendAtEnd ?? false;
            this.AppendSeparator = data.AppendSeparator ?? string.Empty;
        }

        public ConfigDataEnvironmentVariable GetData() {
            return new ConfigDataEnvironmentVariable() {
                Key = this.Key,
                Value = this.Value,
                IsEnabled = this.IsEnabled,
                IsAppendMode = this.IsAppendMode,
                IsAppendAtEnd = this.IsAppendAtEnd,
                AppendSeparator = this.AppendSeparator,
            };
        }
    }
}
