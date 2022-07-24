using System.Globalization;

namespace Martridge.ViewModels.Dmod {
    public struct DmodLocalizationDefinition {
        public string Header { get; set; }
        public CultureInfo? CultureInfo { get; set; }
    }
}
