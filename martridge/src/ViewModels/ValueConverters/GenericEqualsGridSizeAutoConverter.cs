using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class GenericEqualsGridSizeAutoConverter : IValueConverter{

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return value?.Equals(parameter) == true ? GridLength.Auto : new GridLength(0, GridUnitType.Auto);
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
