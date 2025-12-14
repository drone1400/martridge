namespace Martridge.Models.Configuration.Generic.FileData {
    public class ConfigDataEnvironmentVariable {
        public string? Key { get; set; }
        public string? Value { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsAppendMode { get; set; }
        public bool? IsAppendAtEnd { get; set; }
        public string? AppendSeparator { get; set; }
        
        public ConfigDataEnvironmentVariable() { }
        public ConfigDataEnvironmentVariable(string key, string value, bool isEnabled, bool isAppendMode = false, bool isAppendAtEnd = false, string appendSeparator = ":") {
            this.Key = key;
            this.Value = value;
            this.IsEnabled = isEnabled;
            this.IsAppendMode = isAppendMode;
            this.IsAppendAtEnd = isAppendAtEnd;
            this.AppendSeparator = appendSeparator;
        }
    }
}
