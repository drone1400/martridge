using System.Collections.Generic;
using Martridge.Models.Configuration.Generic.FileData;
namespace Martridge.Models.Configuration.LaunchExtension.FileData {
    public class ConfigDataWine {
        public bool? EnableWine { get; set; }
        public List<ConfigDataEnvironmentVariable>? EnvironmentVariables { get; set; }
    }
}
