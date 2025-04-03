using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DynamicData;
using Martridge.Models.Localization;
using Martridge.Trace;
namespace Martridge.Models {
    public class DinkTempFileHelper : IDisposable
    {

        public delegate void LogCallback(string message);
        
        
        public IReadOnlyList<FileInfo> TempFileList;
        public IReadOnlyList<DirectoryInfo> TempDirectories;
        
        private readonly List<FileInfo> _tempFiles = new List<FileInfo>();
        private readonly List<DirectoryInfo> _tempDirectories = new List<DirectoryInfo>();
        private DirectoryInfo _tempBaseFolder;
        
        private bool _disposed = false;

        private LogCallback? _logCallback = null;

        public DinkTempFileHelper() {
            this.TempFileList = new ReadOnlyCollection<FileInfo>(this._tempFiles);
            this.TempDirectories = new ReadOnlyCollection<DirectoryInfo>(this._tempDirectories);

            this._tempBaseFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "dink_temp"));
            
            if (this._tempBaseFolder.Exists == false) {
                this._tempBaseFolder.Create();
            }
            
            this._tempDirectories.Add(this._tempBaseFolder);
        }

        public void SetLogCallback(LogCallback callback)
        {
            this._logCallback = callback;
        }
        public void ClearLogCallback() 
        {
            this._logCallback = null;
        }

        public FileInfo? TryCreateTempFile() {
            try {
                string filename = Path.GetRandomFileName();
                string fullPath = Path.Combine(this._tempBaseFolder.FullName, filename);
                this._logCallback?.Invoke(Localizer.Instance["DinkTempFileHelper/Log/CreatingTemporaryFile"] + fullPath);
                FileInfo temp = new FileInfo(fullPath);
                using FileStream tempFs = temp.Create();
                this._tempFiles.Add(temp);
                return temp;
            } catch (Exception) {
                return null;
            }
        }

        public DirectoryInfo? TryCreateTempDirectory()
        {
            try
            {
                string dirName = Path.GetRandomFileName();
                string fullPath = Path.Combine(this._tempBaseFolder.FullName, dirName);
                this._logCallback?.Invoke(Localizer.Instance["DinkTempFileHelper/Log/CreatingTemporaryDirectory"] + fullPath);
                DirectoryInfo temp = new DirectoryInfo(fullPath);
                temp.Create();
                this._tempDirectories.Add(temp);
                return temp;
            } catch (Exception)
            {
                return null;
            }
        }

        public void RegisterTempFile(FileInfo tempFile)
        {
            this._tempFiles.Add(tempFile);
        }

        public void Dispose() {
            // Dispose of unmanaged resources.
            this.Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (this._disposed) return;

            if (disposing) {
                // no managed objects...
            }

            string deleteFileMsg = Localizer.Instance["DinkTempFileHelper/Log/DeletingTemporaryFile"];
            string deleteDirectoryMsg = Localizer.Instance["DinkTempFileHelper/Log/DeletingTemporaryDirectory"];
            

            try {
                foreach (var x in this._tempFiles) {
                    try {
                        this._logCallback?.Invoke(deleteFileMsg + x.FullName);
                        x.Delete();
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                    }
                }
                foreach (var x in this._tempDirectories) {
                    try {
                        this._logCallback?.Invoke(deleteDirectoryMsg + x.FullName);
                        x.Delete();
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
            }

            this._disposed = true;
        }
    }
}
