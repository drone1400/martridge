using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Martridge.ViewModels {
    public class MainViewPageEnumConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value?.GetType() == typeof(MainViewPage) &&
                parameter?.GetType() == typeof(MainViewPage) &&
                targetType == typeof(bool)) {
                if ((MainViewPage)value == (MainViewPage)parameter) {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException("Reverse conversion not supported");
        }
    }
}
