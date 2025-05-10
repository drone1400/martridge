using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
namespace Martridge.Models {
    public static class ImageHelper {
        private static readonly Dictionary<string,Bitmap> ImageCache = new Dictionary<string,Bitmap>();
        
        public static Bitmap? GetImage(Uri uri) {
            if (ImageCache.TryGetValue(uri.ToString(), out Bitmap? image)) return image;
            try {
                Bitmap bmp = new Bitmap(AssetLoader.Open(uri));
                ImageCache[uri.ToString()] = bmp;
                return bmp;
            } catch (Exception) {
                return null;
            }
        }
    }
}
