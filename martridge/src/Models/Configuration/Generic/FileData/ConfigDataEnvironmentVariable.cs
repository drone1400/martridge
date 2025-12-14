namespace Martridge.Models.Configuration.Generic.FileData {
    public class ConfigDataEnvironmentVariable {
        public string? Key { get; set; }
        public string? Value { get; set; }
        public bool? IsEnabled { get; set; }
        public string? Mode { get; set; }
        public string? AppendSeparator { get; set; }
        
        public ConfigDataEnvironmentVariable() { }
        public ConfigDataEnvironmentVariable(string key, string value, bool isEnabled, string mode = nameof(ConfigEnvVarMode.Normal), string appendSeparator = ":") {
            this.Key = key;
            this.Value = value;
            this.IsEnabled = isEnabled;
            this.Mode = mode;
            this.AppendSeparator = appendSeparator;
        }
    }
}
