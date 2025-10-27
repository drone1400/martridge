using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class BoolToGridSizeStarOrXConverter : IValueConverter {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolValue && boolValue) {
                return new GridLength(1, GridUnitType.Star);
            }
            if (parameter == null) 
                return new GridLength(0);
            if (parameter is string s && double.TryParse(s, out double ds)) 
                return new GridLength(ds, GridUnitType.Pixel);
            if (parameter is double d) 
                return new GridLength(d, GridUnitType.Pixel);
            if (parameter is int i) 
                return new GridLength(i, GridUnitType.Pixel);
            
            return new GridLength(0);
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
