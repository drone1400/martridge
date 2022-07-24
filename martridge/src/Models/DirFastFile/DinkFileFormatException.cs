using System;

namespace Martridge.Models.DirFastFile {
    public class DinkFileFormatException : Exception {
        public DinkFileFormatException(string message) : base(message) { }
        public DinkFileFormatException(string message, Exception innerException) : base(message, innerException) { }
    }

}
