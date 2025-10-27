using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class ThemeVariantNameConverter : IValueConverter {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value == null) return string.Empty;
            return value.ToString()?.Replace('_', ' ');
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
