using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels {
    public class GenericEqualsBoolConverter : IValueConverter{

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return value?.Equals(parameter) == true;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
