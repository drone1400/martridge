using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Martridge.Models.Installer {
    public class DinkTempFileHelper : IDisposable {
        
        public IReadOnlyList<FileInfo> TempFileList;
        public IReadOnlyList<DirectoryInfo> TempDirectories;
        
        private readonly List<FileInfo> _tempFiles = new List<FileInfo>();
        private readonly List<DirectoryInfo> _tempDirectories = new List<DirectoryInfo>();
        private DirectoryInfo _tempBaseFolder;
        
        private bool _disposed = false;

        public DinkTempFileHelper() {
            this.TempFileList = new ReadOnlyCollection<FileInfo>(this._tempFiles);
            this.TempDirectories = new ReadOnlyCollection<DirectoryInfo>(this._tempDirectories);

            this._tempBaseFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "dink_temp"));
            
            if (this._tempBaseFolder.Exists == false) {
                this._tempBaseFolder.Create();
            }
            
            this._tempDirectories.Add(this._tempBaseFolder);
        }

        public FileInfo? TryCreateTempFile() {
            try {
                string filename = Path.GetRandomFileName();
                FileInfo temp = new FileInfo(Path.Combine(this._tempBaseFolder.FullName, filename));
                using FileStream tempFs = temp.Create();
                this._tempFiles.Add(temp);
                return temp;
            } catch (Exception) {
                return null;
            }
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

            try {
                foreach (var x in this._tempFiles) {
                    try {
                        x.Delete();
                    } catch (Exception ex) {
                        // TODO ...
                    }
                }
                foreach (var x in this._tempDirectories) {
                    try {
                        x.Delete();
                    } catch (Exception ex) {
                        // TODO ...
                    }
                }
            } catch (Exception ex) {
                // TODO ...
            }

            this._disposed = true;
        }
    }
}
