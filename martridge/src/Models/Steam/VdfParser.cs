using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace Martridge.Models.Steam {
    public static class VdfParser {
        
        public static VdfObject ReadVdfObject(Stream stream) {
            string objName = ReadStringValue(stream);

            VdfObject obj = new VdfObject(objName);

            while (true) {
                int dataType = stream.ReadByte();
                if (dataType < 0) throw new EndOfStreamException();
                
                // check if done
                if (dataType ==  (byte)Constants.VdfDataType.ObjEnd) return obj;
                
                switch (dataType) {
                    default:  throw new InvalidDataException($"Expecting value<={(byte)Constants.VdfDataType.ObjEnd} in VDF file, read {dataType} instead!");
                    case (byte)Constants.VdfDataType.ObjStart: {
                        VdfObject childObj = ReadVdfObject(stream);
                        obj.Children.Add(childObj.Name, childObj);
                        break;
                    }
                    case (byte)Constants.VdfDataType.String: {
                        string name = ReadStringValue(stream);
                        string value = ReadStringValue(stream);
                        obj.Properties.Add(name, new VdfData(name, Constants.VdfDataType.String, value));
                        break;
                    }
                    case (byte)Constants.VdfDataType.Int32: {
                        string name = ReadStringValue(stream);
                        Int32 value = ReadInt32Value(stream);
                        obj.Properties.Add(name, new VdfData(name, Constants.VdfDataType.Int32, value));
                        break;
                    }
                    case (byte)Constants.VdfDataType.Float32: {
                        string name = ReadStringValue(stream);
                        float value = ReadFloat32Value(stream);
                        obj.Properties.Add(name, new VdfData(name, Constants.VdfDataType.Float32, value));
                        break;
                    }
                    case (byte)Constants.VdfDataType.Ptr32: {
                        string name = ReadStringValue(stream);
                        UInt32 value = ReadUInt32Value(stream);
                        obj.Properties.Add(name, new VdfData(name, Constants.VdfDataType.Ptr32, value));
                        break;
                    }
                    case (byte)Constants.VdfDataType.WString: {
                        string name = ReadStringValue(stream);
                        string value = ReadWStringValue(stream);
                        obj.Properties.Add(name, new VdfData(name, Constants.VdfDataType.WString, value));
                        break;
                    }
                    case (byte)Constants.VdfDataType.Color: {
                        string name = ReadStringValue(stream);
                        UInt32 value = ReadUInt32Value(stream);
                        obj.Properties.Add(name, new VdfData(name, Constants.VdfDataType.Color, value));
                        break;
                    }
                    case (byte)Constants.VdfDataType.UInt64: {
                        string name = ReadStringValue(stream);
                        UInt64 value = ReadUInt64Value(stream);
                        obj.Properties.Add(name, new VdfData(name, Constants.VdfDataType.UInt64, value));
                        break;
                    }
                }
            }
        }

        private static string ReadStringValue(Stream stream) {
            List<byte> bytes = new List<byte>(256);
            
            while (true) {
                int read = stream.ReadByte();
                if (read < 0) throw new EndOfStreamException();
                if (read == 0) {
                    return Encoding.UTF8.GetString(bytes.ToArray());
                }
                bytes.Add((byte)read);
            }
        }

        private static string ReadWStringValue(Stream stream) {
            List<byte> bytes = new List<byte>(512);
            
            // NOTE: from what I understand, Valve's VDF's WString encodes size on 2 bytes?...
            
            int readSize1 = stream.ReadByte();
            int readSize2 = stream.ReadByte();
            
            if (readSize1 < 0 || readSize2 < 0) throw new EndOfStreamException();

            int expectedSize = readSize1 | (readSize2 << 8);

            for (int i = 0; i < expectedSize; ++i) {
                int read1 = stream.ReadByte();
                if (read1 < 0) throw new EndOfStreamException();
                bytes.Add((byte)read1);
                int read2 = stream.ReadByte();
                if (read2 < 0) throw new EndOfStreamException();
                bytes.Add((byte)read2);
            }
            
            return Encoding.GetEncoding("UTF-16LE").GetString(bytes.ToArray());
        }

        private static Int32 ReadInt32Value(Stream stream) {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                int read =  stream.ReadByte();
                if (read < 0) throw new EndOfStreamException();
                bytes[i] = (byte)read;
            }
            if (BitConverter.IsLittleEndian == false) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        
        private static UInt32 ReadUInt32Value(Stream stream) {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                int read =  stream.ReadByte();
                if (read < 0) throw new EndOfStreamException();
                bytes[i] = (byte)read;
            }
            if (BitConverter.IsLittleEndian == false) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
        
        private static UInt64 ReadUInt64Value(Stream stream) {
            byte[] bytes = new byte[8];
            for (int i = 0; i < 8; i++) {
                int read =  stream.ReadByte();
                if (read < 0) throw new EndOfStreamException();
                bytes[i] = (byte)read;
            }
            if (BitConverter.IsLittleEndian == false) Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        private static float ReadFloat32Value(Stream stream) {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                int read =  stream.ReadByte();
                if (read < 0) throw new EndOfStreamException();
                bytes[i] = (byte)read;
            }
            if (BitConverter.IsLittleEndian == false) Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static void WriteVdfObject(Stream stream, VdfObject obj) {
            stream.WriteByte((byte)Constants.VdfDataType.ObjStart);
            WriteStringValue(stream, obj.Name);
            foreach (var kvp in obj.Properties) {
                WriteVdfData(stream, kvp.Value);
            }
            foreach (var kvp in obj.Children) {
                WriteVdfObject(stream, kvp.Value);
            }
            stream.WriteByte((byte)Constants.VdfDataType.ObjEnd);
        }

        private static void WriteVdfData(Stream stream, VdfData vdfData) {
            switch (vdfData.DataType) {
                default: throw new Exception("Tried writing invalid VDF data type!");
                case Constants.VdfDataType.String: {
                    stream.WriteByte((byte)Constants.VdfDataType.String);
                    WriteStringValue(stream, vdfData.Name);
                    WriteStringValue(stream, (string)(vdfData.Value ?? string.Empty));
                    break;
                }
                case Constants.VdfDataType.Int32: {
                    stream.WriteByte((byte)Constants.VdfDataType.Int32);
                    WriteStringValue(stream, vdfData.Name);
                    WriteInt32Value(stream, (Int32)(vdfData.Value ?? 0));
                    break;
                }
                case Constants.VdfDataType.Float32: {
                    stream.WriteByte((byte)Constants.VdfDataType.Float32);
                    WriteStringValue(stream, vdfData.Name);
                    WriteFloat32Value(stream, (float)(vdfData.Value ?? 0));
                    break;
                }
                case Constants.VdfDataType.Ptr32: {
                    stream.WriteByte((byte)Constants.VdfDataType.Ptr32);
                    WriteStringValue(stream, vdfData.Name);
                    WriteUInt32Value(stream, (UInt32)(vdfData.Value ?? 0));
                    break;
                }
                case Constants.VdfDataType.WString: {
                    stream.WriteByte((byte)Constants.VdfDataType.WString);
                    WriteStringValue(stream, vdfData.Name);
                    WriteWStringValue(stream, (string)(vdfData.Value ?? string.Empty));
                    break;
                }
                case Constants.VdfDataType.Color: {
                    stream.WriteByte((byte)Constants.VdfDataType.Color);
                    WriteStringValue(stream, vdfData.Name);
                    WriteUInt32Value(stream, (UInt32)(vdfData.Value ?? 0));
                    break;
                }
                case Constants.VdfDataType.UInt64: {
                    stream.WriteByte((byte)Constants.VdfDataType.UInt64);
                    WriteStringValue(stream, vdfData.Name);
                    WriteUInt64Value(stream, (UInt64)(vdfData.Value ?? 0));
                    break;
                }
            }
        }
        
        private static void WriteStringValue(Stream stream, string value) {
            byte[] bytes =  Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            // make sure to write terminator
            stream.WriteByte(0x00);
        }

        private static void WriteWStringValue(Stream stream, string value) {
            int length = value.Length;
            if (length > 65535) length = 65535;
            byte[] size = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian == false) Array.Reverse(size);
            stream.Write(size, 0, size.Length);
            byte[] bytes =  Encoding.GetEncoding("UTF-16LE").GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            // do we need to write 0x00 0x00 terminator for WString???
        }

        private static void WriteInt32Value(Stream stream, Int32 value) {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == false) Array.Reverse(bytes);
            stream.Write(bytes, 0, bytes.Length);
        }
        
        private static void WriteUInt32Value(Stream stream, UInt32 value) {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == false) Array.Reverse(bytes);
            stream.Write(bytes, 0, bytes.Length);
        }
        
        private static void WriteUInt64Value(Stream stream, UInt64 value) {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == false) Array.Reverse(bytes);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteFloat32Value(Stream stream, float value) {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == false) Array.Reverse(bytes);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
