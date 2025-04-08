using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Martridge.Models {

    public static class LocationHelper {
        public static string AppBaseDirectory { get => _appBaseDirectory; }
        private static readonly string _appBaseDirectory;

        static LocationHelper() {
            // initialize base directory somehow...
            //_AppBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //_AppBaseDirectory = AppContext.BaseDirectory;
            string? processFile = Process.GetCurrentProcess().MainModule?.FileName;
            
            if (processFile == null) {
                NullReferenceException ex = new NullReferenceException("Could not determine current process start location...");
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                throw ex;
            }
            
            FileInfo finfo = new FileInfo(processFile);

            if (finfo.DirectoryName == null) {
                NullReferenceException ex = new NullReferenceException("Could not determine current process start location...");
                MyTrace.Global.WriteException(MyTraceCategory.General, ex);
                throw ex;
            }
            _appBaseDirectory = finfo.DirectoryName;
        }

        public static string WebCache { 
            get {
                string path = Path.Combine(AppBaseDirectory, "webcache");
                if (Directory.Exists(path) == false) {
                    Directory.CreateDirectory(path);
                }
                return path; 
            } 
        }
        
        public static string LocalizationDirectory {
            get => Path.Combine(AppBaseDirectory, "localization");
        }

        public static string LogsDirectory {
            get {
                string path = Path.Combine(AppBaseDirectory, "logs");
                if (Directory.Exists(path) == false) {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }


        public static string TryGetRelativeSubdirectory(string path) {
            string newPath = path;

            if (Path.IsPathRooted(path)) {
                newPath = Path.GetRelativePath(AppBaseDirectory, path);
            }

            if (newPath.StartsWith(".")) {
                // if this is not a subdirectory, return back an absolute path
                newPath = Path.GetFullPath(Path.Combine(AppBaseDirectory, newPath));
            }

            return newPath;
        }
        
        public static List<string> TryGetRelativeSubdirectory(List<string> paths) {
            List<string> newPaths = new List<string>();
            foreach (string path in paths) {
                newPaths.Add(TryGetRelativeSubdirectory(path));
            }
            return newPaths;
        }

        public static string TryGetAbsoluteFromSubdirectoryRelative(string path) {
            if (Path.IsPathRooted(path)) {
                // path already rooted...
                return path;
            }
            
            return Path.Combine(AppBaseDirectory, path);
        }
        
        public static List<string> TryGetAbsoluteFromSubdirectoryRelative(List<string> paths) {
            List<string> newPaths = new List<string>();
            foreach (string path in paths) {
                newPaths.Add(TryGetAbsoluteFromSubdirectoryRelative(path));
            }
            return newPaths;
        }
        
        #region FilePicker stuff


        public static IStorageFolder? BrowseFolderPicker(string title, string? baseDirectory = null)
        {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanPickFolder == false)
                return null;

            IStorageFolder? baseStorageDirectory = null;
            if (string.IsNullOrWhiteSpace(baseDirectory) == false)
            {
                Task<IStorageFolder?> task = storageProvider.TryGetFolderFromPathAsync(baseDirectory);
                task.Wait();
                baseStorageDirectory = task.Result;
            }

            FolderPickerOpenOptions fpo = new FolderPickerOpenOptions() {
                Title = title,
                AllowMultiple = false,
                SuggestedStartLocation = baseStorageDirectory,
            };

            IReadOnlyList<IStorageFolder> results = storageProvider.OpenFolderPickerAsync(fpo).Result;
            if (results.Count > 0)
                return results[0];
            
            return null;
        }

        public static IStorageFile? BrowseFileOpen(string title, IReadOnlyList<FilePickerFileType>? fileTypes, string? baseDirectory = null)
        {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanOpen == false)
                return null;
            
            IStorageFolder? baseStorageDirectory = null;
            if (string.IsNullOrWhiteSpace(baseDirectory) == false)
            {
                Task<IStorageFolder?> task = storageProvider.TryGetFolderFromPathAsync(baseDirectory);
                task.Wait();
                baseStorageDirectory = task.Result;
            }
            
            FilePickerOpenOptions fpo = new FilePickerOpenOptions() {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
                SuggestedStartLocation = baseStorageDirectory,
            };
            

            Task<IReadOnlyList<IStorageFile>> fpoTask = storageProvider.OpenFilePickerAsync(fpo);
            fpoTask.Wait();
            IReadOnlyList<IStorageFile> results = fpoTask.Result;
            
            if (results.Count > 0)
                return results[0];
            
            return null;
        }
        
        public static IStorageFile? BrowseFileSave(string title, IReadOnlyList<FilePickerFileType>? fileTypes, string? baseDirectory = null)
        {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanSave == false)
                return null;
            
            IStorageFolder? baseStorageDirectory = null;
            if (string.IsNullOrWhiteSpace(baseDirectory) == false)
            {
                Task<IStorageFolder?> task = storageProvider.TryGetFolderFromPathAsync(baseDirectory);
                task.Wait();
                baseStorageDirectory = task.Result;
            }
            
            FilePickerSaveOptions fpo = new FilePickerSaveOptions() {
                Title = title,
                FileTypeChoices = fileTypes,
                SuggestedStartLocation = baseStorageDirectory,
            };
            

            Task<IStorageFile?> fpoTask = storageProvider.SaveFilePickerAsync(fpo);
            fpoTask.Wait();
            return fpoTask.Result;
        }
        
        #endregion
    }
}
