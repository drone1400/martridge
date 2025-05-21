using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace Martridge.ViewModels.ValueConverters {


    public enum DoubleCompareMode {
        Equal,
        GreaterThan,
        LessThan,
        GreaterOrEqualTo,
        LessOrEqualTo,
    }
    
    public class DoubleCompareToBoolConverter : IValueConverter {

        public DoubleCompareMode Mode { get; set; } = DoubleCompareMode.GreaterOrEqualTo;
        public double Epsilon { get; set; } = 0.0001;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            double valueDouble = double.NaN;
            double parameterDouble = double.NaN;
            if (parameter == null) return false;
            if (value == null) return false;
            
            if (parameter is double pd) parameterDouble = pd;
            else if (parameter is string ps && double.TryParse(ps, CultureInfo.InvariantCulture, out double pds)) parameterDouble = pds;
            
            if (value is double vd) valueDouble = vd;
            else if (value is string vs && double.TryParse(vs, CultureInfo.InvariantCulture, out double vds)) valueDouble = vds;
            
            if (double.IsNaN(valueDouble)) return false;
            
            if (double.IsNaN(parameterDouble)) return true; // always return true if value is a double but parameter is not...

            return this.Mode switch {
                DoubleCompareMode.GreaterOrEqualTo => valueDouble >= parameterDouble,
                DoubleCompareMode.LessOrEqualTo => valueDouble <= parameterDouble,
                DoubleCompareMode.GreaterThan => valueDouble > parameterDouble,
                DoubleCompareMode.LessThan => valueDouble < parameterDouble,
                DoubleCompareMode.Equal => Math.Abs(valueDouble - parameterDouble) <= this.Epsilon,
                _ => false
            };
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
