using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Martridge.Models;
using Martridge.Models.DmodPacker;
namespace Martridge.ViewModels.ValueConverters {
    public class DmodNodeImageConverter : IValueConverter {
        
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is DmodNodeType res) {
                string resPath = res switch {
                    DmodNodeType.Audio => "avares://martridge/Assets/icons/icons8-audio-file-48.png",
                    DmodNodeType.Data => "avares://martridge/Assets/icons/icons8-binary-file-48.png",
                    DmodNodeType.DinkD => "avares://martridge/Assets/icons/icons8-code-file-48.png",
                    DmodNodeType.DinkC => "avares://martridge/Assets/icons/icons8-code-file-48.png",
                    DmodNodeType.Directory => "avares://martridge/Assets/icons/icons8-folder-48.png",
                    DmodNodeType.DirFF => "avares://martridge/Assets/icons/icons8-box-48.png",
                    DmodNodeType.Image => "avares://martridge/Assets/icons/icons8-image-file-48.png",
                    DmodNodeType.Text => "avares://martridge/Assets/icons/icons8-txt-48.png",
                    _ => "avares://martridge/Assets/icons/icons8-file-48.png",
                };
                Bitmap? bmp = ImageHelper.GetImage(new Uri(resPath));
                if (bmp != null)
                    return bmp;
            }
            return BindingOperations.DoNothing;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
        
    }
}
