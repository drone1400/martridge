using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class GenericNotEqualsBoolConverter : IValueConverter{

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return value?.Equals(parameter) == false;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
