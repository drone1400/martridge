using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {
    public class TypeCompareToBoolConverter : IValueConverter {
        
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value == null) return false;
            if (parameter is Type type ) {
                return value.GetType() == type;
            } else if (parameter is string typeName) {
                return value.GetType().Name == typeName;
            }
            return false;
        }
        
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
