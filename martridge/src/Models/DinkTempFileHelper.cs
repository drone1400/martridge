using System;
using System.Collections.Generic;
using System.IO;
using Martridge.Models.Localization;
using Martridge.Trace;
namespace Martridge.Models {
    public class DinkTempFileHelper : IDisposable
    {

        public delegate void LogCallback(string line1, string line2);
        
        private readonly Stack<FileInfo> _tempFiles = new Stack<FileInfo>();
        private readonly Stack<DirectoryInfo> _tempDirectories = new Stack<DirectoryInfo>();
        private DirectoryInfo _tempBaseFolder;
        
        private bool _disposed = false;

        private LogCallback? _logCallback = null;

        public DinkTempFileHelper() {

            this._tempBaseFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "dink_temp"));
            
            if (this._tempBaseFolder.Exists == false) {
                this._tempBaseFolder.Create();
            }
            
            this._tempDirectories.Push(this._tempBaseFolder);
        }

        public void SetLogCallback(LogCallback callback)
        {
            this._logCallback = callback;
        }
        public void ClearLogCallback() 
        {
            this._logCallback = null;
        }

        private void LogInvoke(string message, string filePath) {
            try {
                this._logCallback?.Invoke(message, $"    \"{filePath}\"");
            } catch (Exception) {
                // ignored
            }
        }

        public FileInfo? TryCreateTempFile() {
            try {
                string filename = Path.GetRandomFileName();
                string fullPath = Path.Combine(this._tempBaseFolder.FullName, filename);
                this.LogInvoke(Localizer.Instance["DinkTempFileHelper/Log/CreatingTemporaryFile"], fullPath);
                FileInfo temp = new FileInfo(fullPath);
                using FileStream tempFs = temp.Create();
                this._tempFiles.Push(temp);
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
                this.LogInvoke(Localizer.Instance["DinkTempFileHelper/Log/CreatingTemporaryDirectory"], fullPath);
                DirectoryInfo temp = new DirectoryInfo(fullPath);
                temp.Create();
                this._tempDirectories.Push(temp);
                return temp;
            } catch (Exception)
            {
                return null;
            }
        }

        public void RegisterTempFile(FileInfo tempFile)
        {
            this._tempFiles.Push(tempFile);
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
                while (this._tempFiles.Count > 0) {
                    try {
                        FileInfo x = this._tempFiles.Pop();
                        try {
                            this.LogInvoke(deleteFileMsg, x.FullName);
                        } catch (Exception) {
                            // ignored
                        }
                        x.Delete();
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(ex);
                    }
                }

                while (this._tempDirectories.Count > 0) {
                    try {
                        DirectoryInfo x = this._tempDirectories.Pop();
                        this.LogInvoke(deleteDirectoryMsg, x.FullName);
                        x.Delete(true);
                    } catch (Exception ex) {
                        MyTrace.Global.WriteException(ex);
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }

            this._disposed = true;
        }
    }
}
