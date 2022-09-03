using Avalonia.Data.Converters;
using Martridge.Models.Localization;
using System;
using System.Globalization;

namespace Martridge.ViewModels.Dmod {
    public class DmodOrderByEnumTextConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value?.GetType() == typeof(DmodOrderBy)) {
                return Localizer.Instance[$"OnlineDmodBrowser/OrderBy/{(DmodOrderBy)value}"];
            }

            return "???";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException("Reverse conversion not supported");
        }
    }
}
