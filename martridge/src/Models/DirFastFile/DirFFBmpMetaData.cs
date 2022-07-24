using System;
using System.Text;

namespace Martridge.Models.DirFastFile {
    public struct DirFfBmpMetaData {
        public const int MaxBmpFileNameLength = 12;
        public const int HeaderLength = 17;

        public int Offset { get; set; }
        public int Size { get; set; }
        public string FileName { get; set; }

        public DirFfBmpMetaData(int offset, int size, string fileName) {
            this.Offset = offset;
            this.Size = size;
            this.FileName = fileName;
        }

        public byte[] GetHeaderBytes() {
            byte[] header = new byte[HeaderLength];

            header[0] = (byte)(this.Offset & 0xFF);
            header[1] = (byte)((this.Offset >> 8) & 0xFF);
            header[2] = (byte)((this.Offset >> 16) & 0xFF);
            header[3] = (byte)((this.Offset >> 24) & 0xFF);

            Encoding.ASCII.GetBytes(this.FileName, 0, Math.Min(this.FileName.Length, MaxBmpFileNameLength), header, 4);

            // string terminator must be 0
            header[16] = 0x00;

            return header;
        }
    }
}
