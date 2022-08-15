using Avalonia.Media.Imaging;
using Martridge.Models.DirFastFile;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.IO;

namespace Martridge.Models.Dmod {
    public class DmodFileDefinition {

        private static List<string> KnownThumbnailFileNames { get; } = new List<string>() {
          "title-01", "misc-01", "dinkl-01",
        };

        private const string FileNameDinkIni = "dink.ini";
        private const string FileNameDmodDiz = "dmod.diz";
        private const string FileNameDinkDat = "dink.dat";
        private const string FileNameHardDat = "hard.dat";
        private const string FileNameMapDat = "map.dat";
        private const string FileNameDirff = "dir.ff";
        private const string DirNameGraphics = "graphics";


        public bool IsCorrectlyDefined { get; private set; } = false;

        public DirectoryInfo DmodRoot { get; private set; }

        public FileInfo? DinkIni { get; private set; }
        public FileInfo? DinkDat { get; private set; }
        public FileInfo? HardDat { get; private set; }
        public FileInfo? MapDat { get; private set; }
        public FileInfo? DmodDiz { get; private set; }

        public List<FileInfo> LocalizationFiles { get; private set; } = new List<FileInfo>();

        public DmodFileDefinition(string dmodRootPath) {
            this.DmodRoot = new DirectoryInfo(dmodRootPath);
            this.ScanDmodFiles();
            this.ScanLocalizationFiles();
        }

        public DmodFileDefinition(DirectoryInfo dirInfo) {
            this.DmodRoot = dirInfo;
            this.ScanDmodFiles();
            this.ScanLocalizationFiles();
        }

        private void ScanLocalizationFiles() {
            this.LocalizationFiles = new List<FileInfo>();
            if (this.DmodRoot.Exists != true) {
                // can't do jack if the directory isn't real, Jack
                return;
            }

            DirectoryInfo locDirRoot = new DirectoryInfo(
                Path.Combine(this.DmodRoot.FullName, "l10n"));
            // check if localization directory exists
            if (!locDirRoot.Exists) return;

            Stack<DirectoryInfo> dirStack = new Stack<DirectoryInfo>();
            
            dirStack.Push(locDirRoot);
            while (dirStack.Count > 0) {
                DirectoryInfo dir = dirStack.Pop();
                DirectoryInfo[] subDirs = dir.GetDirectories();
                foreach (DirectoryInfo sd in subDirs) {
                    dirStack.Push(sd);
                }

                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo f in files) {
                    if (f.Extension.ToLowerInvariant() == ".mo") {
                        this.LocalizationFiles.Add(f);
                    }
                }
            }
        }


        private void ScanDmodFiles() {
            if (this.DmodRoot.Exists != true) {
                // can't do jack if the directory isn't real, Jack
                return;
            }

            FileInfo[] files = this.DmodRoot.GetFiles();

            foreach (FileInfo file in files) {
                string fileLower = file.Name.ToLowerInvariant();

                switch (fileLower) {
                    case FileNameDinkDat:
                        this.DinkDat = file;
                        break;
                    case FileNameDinkIni:
                        this.DinkIni = file;
                        break;
                    case FileNameDmodDiz:
                        this.DmodDiz = file;
                        break;
                    case FileNameHardDat:
                        this.HardDat = file;
                        break;
                    case FileNameMapDat:
                        this.MapDat = file;
                        break;
                }
            }

            if (this.DinkDat != null &&
                this.DinkIni != null &&
                this.DmodDiz != null &&
                this.HardDat != null &&
                this.MapDat != null) {
                this.IsCorrectlyDefined = true;
            }
        }

        public string? GetDescription() {
            try {
                if (this.IsCorrectlyDefined == false) {
                    return null;
                }

                string desc = File.ReadAllText(this.DmodDiz!.FullName);

                return desc;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex, MyTraceLevel.Warning);
                return null;
            }
        }

        public string? GetName() {
            try { 
                if (this.IsCorrectlyDefined == false) {
                    return null;
                }

                string[] lines = File.ReadAllLines(this.DmodDiz!.FullName);

                return lines[0];
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex, MyTraceLevel.Warning);
                return null;
            }
        }


        private Bitmap? ScanDirFf(FileInfo file) {
            // this should be impossible but just in case
            if (file.Directory == null) return null;
            
            List<DirFfBmpMetaData>  meta = DirFf.LoadMetaDataFromDirectory(file.Directory.FullName);
            foreach (DirFfBmpMetaData metaData in meta) {
                string fileLower = metaData.FileName.ToLowerInvariant();
                string fileLowerNoExt = Path.GetFileNameWithoutExtension(fileLower);
                foreach (string str in KnownThumbnailFileNames) {
                    if (str == fileLowerNoExt) {
                        return new Bitmap(DirFf.LoadImageStream(file,fileLower)); 
                    }
                }
            }
            return null;
        }

        private Bitmap? ScanDirectory(DirectoryInfo? dirInfo) {
            if (dirInfo == null) {
                return null;
            }

            DirectoryInfo[] subDirs = dirInfo.GetDirectories();
            FileInfo[] files = dirInfo.GetFiles();

            foreach (FileInfo file in files) {
                // scan known image files..
                string fileLower = file.Name.ToLowerInvariant();
                string fileLowerNoExt = Path.GetFileNameWithoutExtension(fileLower);
                foreach (string str in KnownThumbnailFileNames) {
                    if (str == fileLowerNoExt) {
                        return new Bitmap(file.FullName);
                    }
                }

                // scan dir.ff
                if (fileLower == FileNameDirff) {
                    Bitmap? bmp = this.ScanDirFf(file);
                    if (bmp != null) {
                        return bmp;
                    }
                }
            }

            // try scanning subdirectories...
            foreach (DirectoryInfo dir in subDirs) {
                Bitmap? bmp = this.ScanDirectory(dir);
                if (bmp != null) {
                    return bmp;
                }
            }

            return null;
        }


        public Bitmap? GetThumbnail() {
            try {
                DirectoryInfo? graphicsRoot = null;
                DirectoryInfo[] lvl1dirs = this.DmodRoot.GetDirectories();
                foreach (DirectoryInfo dirInfo in lvl1dirs) {
                    if (dirInfo.Name.ToLowerInvariant() == DirNameGraphics) {
                        graphicsRoot = dirInfo;
                        break;
                    }
                }

                Bitmap? bitmap = this.ScanDirectory(graphicsRoot);

                return bitmap;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex, MyTraceLevel.Warning);
                return null;
            }
        }

        
    }
}
