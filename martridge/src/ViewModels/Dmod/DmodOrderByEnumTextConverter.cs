using Avalonia.Data.Converters;
using Martridge.Models.Localization;
using System;
using System.Globalization;

namespace Martridge.ViewModels.Dmod {
    public class DmodOrderByEnumTextConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value?.GetType() == typeof(DmodOrderBy)) {
                return Localizer.Instance[$"OnlineDmodBrowser/OrderBy/{(DmodOrderBy)value}"];
                // switch ((DmodOrderBy) value) {
                //     case DmodOrderBy.AuthorAsc: return "";
                //     case DmodOrderBy.AuthorDesc: return "";
                //     case DmodOrderBy.DownloadsAsc: return "";
                //     case DmodOrderBy.DownloadsDesc: return "";
                //     case DmodOrderBy.NameAsc: return "";
                //     case DmodOrderBy.NameDesc: return "";
                //     case DmodOrderBy.ScoreAsc: return "";
                //     case DmodOrderBy.ScoreDesc: return "";
                //     case DmodOrderBy.UpdatedAsc: return "";
                //     case DmodOrderBy.UpdatedDesc: return "";
                // }
            }

            return "???";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
