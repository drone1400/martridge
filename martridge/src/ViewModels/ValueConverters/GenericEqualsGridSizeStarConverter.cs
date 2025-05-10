using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class GenericEqualsGridSizeStarConverter : IValueConverter{

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (parameter is string pstr) {
                return value?.ToString() == pstr ? GridLength.Star : new GridLength(0, GridUnitType.Auto);
            }
            return value?.Equals(parameter) == true ? GridLength.Star : new GridLength(0, GridUnitType.Auto);
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
