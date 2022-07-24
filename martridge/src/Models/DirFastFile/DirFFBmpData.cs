using System.Text;

namespace Martridge.Models.DirFastFile {

    public class DirFfBmpData {
        public const int MaxBmpFileNameLength = 12;
        public const int HeaderLength = 17;

        private int _offset  ;
        public int Offset {
            get => this._offset;
            set => this.SetOffset(value);
        }

        private string _fileName = "";
        public string FileName {
            get => this._fileName;
            set => this.SetFileName(value);
        }

        private readonly byte[] _rawHeader = new byte[17] {
             // 4 bytes for the index
            0x00, 0x00, 0x00, 0x00, 
            // 13 bytes for the filename
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        public byte[] RawHeader { get => this._rawHeader; }



        private byte[] _rawData = new byte[]{ };
        public byte[] RawData {
            get => this._rawData;
            set => this.SetData(value);
        }

        public DirFfBmpData(byte[] rawHeader) {
            if (rawHeader.Length != this._rawHeader.Length) {
                throw new DinkFileFormatException($"[DirFF] Invalid raw header length: {rawHeader.Length})");
            }

            int offset = rawHeader[0] + (rawHeader[1] << 8) + (rawHeader[2] << 16) + (rawHeader[3] << 24);
            int length = 12;
            for (int i = 4; i < rawHeader.Length; i++) {
                if (rawHeader[i] == 0x00) {
                    length = i - 4;
                    break;
                }
            }
            string fileName = Encoding.ASCII.GetString(rawHeader, 4, length);

            this.SetOffset(offset);
            this.SetFileName(fileName);
        }

        public DirFfBmpData(string fileName) {
            this.SetFileName(fileName);
        }

        private void SetOffset(int value) {
            this._offset = value;
            this._rawHeader[0] = (byte)(value & 0xFF);
            this._rawHeader[1] = (byte)((value >> 8) & 0xFF);
            this._rawHeader[2] = (byte)((value >> 16) & 0xFF);
            this._rawHeader[3] = (byte)((value >> 24) & 0xFF);
        }

        private void SetFileName(string fileName) {
            if (fileName.Length > MaxBmpFileNameLength) {
                throw new DinkFileFormatException($"[DirFF] File name \"{fileName}\" exceeds max length (8 characters, +4 for extension).");
            }

            this._fileName = fileName;

            // make sure to clear the header first before writing the file name
            for (int i = 4; i < this._rawHeader.Length; i++) {
                this._rawHeader[i] = 0x00;
            }

            Encoding.ASCII.GetBytes(this._fileName, 0, this._fileName.Length, this._rawHeader, 4);
        }

        private void SetData(byte[] data) {
            this._rawData = data;
        }

    }
}
