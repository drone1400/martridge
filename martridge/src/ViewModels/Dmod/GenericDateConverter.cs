using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.Dmod {
    public class GenericDateConverter : IValueConverter{
        
        
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                return dt.ToString("d");
            }
            return value?.ToString();
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
