using Avalonia.Media.Imaging;
using Martridge.Models.DirFastFile;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.IO;

namespace Martridge.Models.Dmod {
    public class DmodFileDefinition {

        private static List<string> KnownThumbnailFileNames { get; } = new List<string>() {
            "preview", "title-01", "misc-01", "dinkl-01", 
        };

        private const string FILE_NAME_DINK_INI = "dink.ini";
        private const string FILE_NAME_DMOD_DIZ = "dmod.diz";
        private const string FILE_NAME_DINK_DAT = "dink.dat";
        private const string FILE_NAME_HARD_DAT = "hard.dat";
        private const string FILE_NAME_MAP_DAT = "map.dat";
        private const string FILE_NAME_DIRFF = "dir.ff";
        private const string DIR_NAME_GRAPHICS = "graphics";


        public bool IsCorrectlyDefined { get; private set; } = false;
        public bool IsCompletelyDefined { get; private set; } = false;

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
                    case FILE_NAME_DINK_DAT:
                        this.DinkDat = file;
                        break;
                    case FILE_NAME_DINK_INI:
                        this.DinkIni = file;
                        break;
                    case FILE_NAME_DMOD_DIZ:
                        this.DmodDiz = file;
                        break;
                    case FILE_NAME_HARD_DAT:
                        this.HardDat = file;
                        break;
                    case FILE_NAME_MAP_DAT:
                        this.MapDat = file;
                        break;
                }
            }

            int fileCount = 0;
            
            if (this.DinkDat != null) fileCount++;
            if (this.MapDat != null) fileCount++;
            if (this.DmodDiz != null) fileCount++;
            if (this.HardDat != null) fileCount++;
            if (this.DinkIni != null) fileCount++;
            
            

            // NOTE: DMOD can be missing dink.ini and hard.dat, in which case the default ones from the core install will be used...
            this.IsCorrectlyDefined = (fileCount >= 2);
            this.IsCompletelyDefined = (fileCount == 5) ;
        }

        public string? GetDescription() {
            try {
                if (this.DmodDiz == null)
                    return null;

                string desc = File.ReadAllText(this.DmodDiz!.FullName);

                return desc;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex, MyTraceLevel.Warning);
                return null;
            }
        }

        public string? GetName() {
            try {
                string[]? lines = null;

                if (this.DmodDiz != null) {
                    try {
                        lines = File.ReadAllLines(this.DmodDiz!.FullName);
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex, MyTraceLevel.Warning);
                    }
                }

                if (lines == null || lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0])) {
                    // fallback to directory name
                    return this.DmodRoot.Name;
                } else {
                    return lines[0];
                }
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
                        MemoryStream? stream = DirFf.LoadImageStream(file, fileLower);
                        if (stream == null) return null;
                        return new Bitmap(stream); 
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
                if (fileLower == FILE_NAME_DIRFF) {
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
                    if (dirInfo.Name.ToLowerInvariant() == DIR_NAME_GRAPHICS) {
                        graphicsRoot = dirInfo;
                        break;
                    }
                }

                Bitmap? bitmap = this.ScanDirectory(this.DmodRoot);

                return bitmap;
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex, MyTraceLevel.Warning);
                return null;
            }
        }

        
    }
}
