using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Martridge {
    public class GenericTemplateSelector : IDataTemplate {
        
        [Content]
        public Dictionary<string, IDataTemplate> Templates { get; } = new Dictionary<string, IDataTemplate>();
        
        

        public Control? Build(object? param) {
            string key = param?.ToString()?.ToLowerInvariant() ?? string.Empty;
            if (this.Templates.TryGetValue(key, out IDataTemplate? template)) {
                return template.Build(param);
            }
            return null;
        }
        public bool Match(object? data) => true;
    }
}
