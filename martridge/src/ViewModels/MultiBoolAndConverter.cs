using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Martridge.ViewModels {
    public class MultiBoolAndConverter : IMultiValueConverter {

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {

            foreach (object? obj in values) {
                if (obj is bool boolVal) {
                    if (boolVal == false) return false;
                } else {
                    return false;
                }
            }

            return true;
        }
    }
}
