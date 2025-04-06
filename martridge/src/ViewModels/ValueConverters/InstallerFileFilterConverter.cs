using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Martridge.ViewModels.Installer {
    public class InstallerFileFilterConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value?.GetType() == typeof(List<string>)
                && targetType == typeof(string)) {
                List<string> list = (List<string>)value;
                StringBuilder strBuilder = new StringBuilder();
                foreach (string str in list) {
                    strBuilder.Append(str);
                    strBuilder.Append(Environment.NewLine);
                }
                return strBuilder.ToString();
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value?.GetType() == typeof(string)
                && targetType == typeof(List<string>)) {
                List<string> list = new List<string>();
                string input = (string)value;

                string[] split = input.Split(Environment.NewLine);
                foreach (string str in split) {
                    list.Add(str);
                }
                return list;
            }
            throw new ArgumentException();
        }
    }
}
