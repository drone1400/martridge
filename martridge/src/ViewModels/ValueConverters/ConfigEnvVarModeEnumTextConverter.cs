using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Martridge.Models.Configuration.Generic;
using Martridge.Models.Localization;
using Martridge.ViewModels.Dmod;
namespace Martridge.ViewModels.ValueConverters {
    public class ConfigEnvVarModeEnumTextConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value?.GetType() == typeof(ConfigEnvVarMode)) {
                return Localizer.Instance[$"SettingsWineView/ConfigEnvVarMode/{(ConfigEnvVarMode)value}"];
            }

            return "???";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException("Reverse conversion not supported");
        }
    }
}
