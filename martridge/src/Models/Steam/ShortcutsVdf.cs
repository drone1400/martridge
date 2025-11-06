using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Martridge.Trace;
namespace Martridge.Models.Steam {
    public class ShortcutsVdf {

        public VdfObject Root => this._root;
        private VdfObject _root = new VdfObject("");

        public static ShortcutsVdf FromFile(string path) {
            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return FromStream(stream);
        }
        
        private static ShortcutsVdf FromStream(Stream stream) {
            ShortcutsVdf parser = new ShortcutsVdf();
            
            try {
                int objStart = stream.ReadByte();
                if (objStart < 0) throw new EndOfStreamException();
                if (objStart != (byte)Constants.VdfDataType.ObjStart) throw new InvalidDataException($"Expecting {(byte)Constants.VdfDataType.ObjStart} in VDF file, read {objStart} instead!");
                
                parser._root = VdfParser.ReadVdfObject(stream);
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
            
            return parser;
        }

        public void SaveToFile(string path) {
            string tempPath = path + ".tmp";
            bool isFileOk = false;
            try {
                using FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
                VdfParser.WriteVdfObject(stream, this._root);
                // there seems to be an extra object end marker at the end???
                stream.WriteByte((byte)Constants.VdfDataType.ObjEnd);
                isFileOk = true;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }

            try {
                if (isFileOk) {
                    File.Move(tempPath, path, true);
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
            finally {
                try {
                    if (File.Exists(tempPath)) {
                        File.Delete(tempPath);
                    }
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(ex);
                }
            }
        }

        public VdfObject? FindShortcutByExe(string exePath) {
            foreach (var kvp in this._root.Children) {
                if (kvp.Value.Properties.TryGetValue(nameof(Constants.ShortcutsEntryFields.Exe), out VdfData? data) && data.Value is string path) {
                    path = path.Trim('\"');
                    if (LocationHelper.PathIsEqual(exePath, path)) {
                        return kvp.Value;
                    }
                }
            }

            return null;
        }

        public static int GetAppId(VdfObject obj) {
            if (obj.Properties.TryGetValue(nameof(Constants.ShortcutsEntryFields.appid), out VdfData? data) && data.Value is Int32 intValue) {
                return intValue;
            }
            return 0;
        }
    }
}
