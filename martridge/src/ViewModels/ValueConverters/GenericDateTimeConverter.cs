using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class GenericDateTimeConverter : IValueConverter{
        
        
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                return dt.ToString("G");
            }
            return value?.ToString();
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
