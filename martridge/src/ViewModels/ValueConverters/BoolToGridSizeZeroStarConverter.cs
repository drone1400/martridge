using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class BoolToGridSizeZeroStarConverter : IValueConverter {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolValue && boolValue) {
                return new GridLength(1, GridUnitType.Star);
            }
            return new GridLength(0);
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
