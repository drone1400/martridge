using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
namespace Martridge.ViewModels.ValueConverters {
    
    public class DmodScoreToSolidColorBrushConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value?.GetType() == typeof(double)) {
                double score = (double)value;
                
                if (double.IsNaN(score)) {
                    return new SolidColorBrush(Colors.Transparent);
                } else if (score < 3.0) {
                    return new SolidColorBrush(Colors.Red);
                } else if (score < 5.0) {
                    return new SolidColorBrush(Colors.Orange);
                } else if (score < 7.0) {
                    return new SolidColorBrush(Colors.Yellow);
                } else if (score < 9.0) {
                    return new SolidColorBrush(Colors.LimeGreen);
                } else {
                    return new SolidColorBrush(Colors.Green);
                }
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException("Reverse conversion not supported");
        }
    }
}
