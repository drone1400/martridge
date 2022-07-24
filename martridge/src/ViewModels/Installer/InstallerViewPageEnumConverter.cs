using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Martridge.ViewModels.Installer {

    public class InstallerViewPageEnumConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value?.GetType() == typeof(InstallerViewPage) &&
                parameter?.GetType() == typeof(InstallerViewPage) &&
                targetType == typeof(bool)) {
                if ((InstallerViewPage)value == (InstallerViewPage)parameter) {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
