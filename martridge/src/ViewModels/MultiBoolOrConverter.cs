using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Martridge.ViewModels {
    public class MultiBoolOrConverter : IMultiValueConverter {

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {

            foreach (object? obj in values) {
                if (obj is bool boolVal) {
                    if (boolVal) return true;
                } else {
                    return false;
                }
            }

            return false;
        }
    }
}
