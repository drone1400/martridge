using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class DoubleCompareBoolConverter : IValueConverter {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is double size && parameter is double threshold) {
                return size > threshold;
            }
            // default to true?
            return true;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
