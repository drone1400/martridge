using Martridge.Trace;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Martridge.Models.DirFastFile {

    public static class DirFf {

        private static string GetStringFromBytes(byte[] rawFileName) {
            int fileNameLength = 0;
            while (fileNameLength < rawFileName.Length && rawFileName[fileNameLength] != 0x00) { fileNameLength++; }
            string fileName = Encoding.ASCII.GetString(rawFileName, 0, fileNameLength);
            return fileName;
        }

        public static List<DirFfBmpMetaData> LoadMetaDataFromDirectory(string directory) {
            List<DirFfBmpMetaData> headers = new List<DirFfBmpMetaData>();
            if (SafetyChecksAreOk(directory)) {
                string dirffFile = Path.Combine(directory, "dir.ff");

                using (FileStream dirffStream = new FileStream(dirffFile, FileMode.Open)) {
                    using (BinaryReader dirffReader = new BinaryReader(dirffStream)) {
                        // read headers
                        int fileCount = dirffReader.ReadInt32();

                        int offset = dirffReader.ReadInt32();
                        byte[] rawFileName = dirffReader.ReadBytes(DirFfBmpMetaData.MaxBmpFileNameLength + 1);

                        for (int i = 0; i < fileCount - 1; i++) {
                            int offset2 = dirffReader.ReadInt32();
                            string fileName = GetStringFromBytes(rawFileName);
                            DirFfBmpMetaData header = new DirFfBmpMetaData(offset, offset2-offset, fileName);
                            headers.Add(header);
                            offset = offset2;
                            rawFileName = dirffReader.ReadBytes(DirFfBmpMetaData.MaxBmpFileNameLength + 1);
                        }

                        // close streams, normally using should take care of this, but better safe than sorry
                        dirffReader.Close();
                        dirffStream.Close();
                    }
                }
            }

            return headers;
        }

        public static MemoryStream? LoadImageStream(FileInfo dirffFile, string targetFileName, bool compareLowerCaseNames = true) {

            string file = Path.GetFileName(targetFileName);
            string fileLower = file.ToLowerInvariant();

            if (SafetyChecksAreOk(dirffFile.Directory?.FullName)) {
                using (FileStream dirffStream = new FileStream(dirffFile.FullName, FileMode.Open)) {
                    using (BinaryReader dirffReader = new BinaryReader(dirffStream)) {
                        // read headers
                        int fileCount = dirffReader.ReadInt32();

                        int targetOffset = -1;
                        int targetSize = -1;

                        // find target filename...
                        for (int i = 0; i < fileCount - 1; i++) {
                            int offset = dirffReader.ReadInt32();
                            byte[] rawFileName = dirffReader.ReadBytes(DirFfBmpMetaData.MaxBmpFileNameLength + 1);
                            string fileName = GetStringFromBytes(rawFileName);
                            if ((compareLowerCaseNames && fileName.ToLowerInvariant() == fileLower) ||
                                (!compareLowerCaseNames && fileName == file)) {
                                int offset2 = dirffReader.ReadInt32();
                                targetOffset = offset;
                                targetSize = offset2 - offset;
                                break;
                            }
                        }

                        //
                        if (targetOffset != -1) {
                            dirffReader.BaseStream.Position = targetOffset;
                            byte[] data = dirffReader.ReadBytes(targetSize);
                            if (data.Length != targetSize) {
                                MyTrace.Global.WriteMessage(MyTraceCategory.DirFf, $"Target File {file} could not be extracted from \"dir.ff\"...", MyTraceLevel.Error);
                                return null;
                            }
                            return new MemoryStream(data);
                        } else {
                            MyTrace.Global.WriteMessage(MyTraceCategory.DirFf, $"Target File {file} not found in \"dir.ff\"...", MyTraceLevel.Error);
                            return null;
                        }
                    }
                }
            }

            return null;
        }

        private static bool SafetyChecksAreOk(string? directory) {
            if (directory == null) return false;
            
            string filePath = Path.Combine(directory, "dir.ff");
            // check file exists
            if (Directory.Exists(directory) == false || File.Exists(filePath) == false) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DirFf, $"Could not find \"dir.ff\" at: \"{ filePath }\"", MyTraceLevel.Error);
                return false;
            }

            // check file min length
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Length < 2 * DirFfBmpMetaData.HeaderLength + 4) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DirFf, $"File too small to be processed... \"{ filePath }\"", MyTraceLevel.Error);
                return false;
            }

            return true;
        }

        public static void CreateDirFf(string directory) {
            if (Directory.Exists(directory) == false) {
                MyTrace.Global.WriteMessage(MyTraceCategory.DirFf, $"Could not find directory to create \"dir.ff\"...", MyTraceLevel.Error);
                return;
            }

            string dirffFile = Path.Combine(directory, "dir.ff");

            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            FileInfo[] files = dirInfo.GetFiles();
            List<FileInfo> legitBmps = new List<FileInfo>();
            List<FileInfo> invalidBmps = new List<FileInfo>();

            // look for bmp files with the correct file name length
            for (int i = 0; i < files.Length; i++) {
                if (files[i].Extension.ToLowerInvariant() == ".bmp") {
                    if (files[i].Name.Length <= DirFfBmpData.MaxBmpFileNameLength) {
                        legitBmps.Add(files[i]);
                    } else {
                        invalidBmps.Add(files[i]);
                        MyTrace.Global.WriteMessage(MyTraceCategory.DirFf, $"BMP name exceeds max length for \"dir.ff\", ignoring: \"{ files[i].FullName}\"", MyTraceLevel.Warning);
                    }
                }
            }

            IOrderedEnumerable<FileInfo> orderedBmps = legitBmps.OrderBy(x => x.Name);
            List<DirFfBmpMetaData> headers = new List<DirFfBmpMetaData>();
            int blockCount = files.Count() + 1;

            // generate headers...
            int baseOffset = 4 + blockCount * DirFfBmpMetaData.HeaderLength;
            int offset = baseOffset;
            foreach (FileInfo file in orderedBmps) {
                headers.Add(new DirFfBmpMetaData(offset, (int)file.Length, file.Name));
                offset += (int)file.Length;
            }

            using (FileStream dirffStream = new FileStream(dirffFile, FileMode.Create)) {
                using (BinaryWriter dirffWriter = new BinaryWriter(dirffStream)) {
                    // write block count as 4 bytes
                    dirffWriter.Write(blockCount);

                    // write headers
                    for (int i = 0; i < headers.Count; i++) {
                        dirffWriter.Write(headers[i].GetHeaderBytes());
                    }

                    // write final header marking end offset...
                    DirFfBmpMetaData last = new DirFfBmpMetaData(headers[headers.Count-1].Offset + headers[headers.Count-1].Size, 0, "");
                    dirffWriter.Write(last.GetHeaderBytes());

                    // write actual data...
                    foreach (FileInfo file in orderedBmps) {
                        using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open)) {
                            using (BinaryReader binaryReader = new BinaryReader(fileStream)) {
                                dirffWriter.Write(binaryReader.ReadBytes((int)file.Length));
                            }
                        }
                    }

                    // done, close streams
                    dirffWriter.Close();
                    dirffStream.Close();
                }
            }

        }



        //public void SaveBmps() {
        //    for (int i = 0; i < this._BmpData.Count; i++) {
        //        string filePath = Path.Combine(this._DirectoryPath, this._BmpData[i].FileName);

        //        using (FileStream bmpStream = new FileStream(filePath, FileMode.Create)) {
        //            using (BinaryWriter bmpWriter = new BinaryWriter(bmpStream)) {
        //                bmpWriter.Write(this._BmpData[i].RawData);
        //                bmpWriter.Close();
        //                bmpStream.Close();
        //            }
        //        }
        //    }
        //}
    }
}
