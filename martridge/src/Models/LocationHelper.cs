using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace Martridge.Models {

    [Flags]
    public enum LocationHelperPathCompareFlags : int {
        None = 0,
        IgnoreCaseAlways = 1,
        IgnoreCaseNever = 2,
        IgnoreDirectorySeparator = 4,
    }

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

        public static bool PathIsEqual(string? path1, string? path2, LocationHelperPathCompareFlags flags = LocationHelperPathCompareFlags.None) {
            if (path1 == null || path2 == null) return false;
            
            if (flags.HasFlag(LocationHelperPathCompareFlags.IgnoreDirectorySeparator)) {
                path1 = Path.TrimEndingDirectorySeparator(path1);
                path2 = Path.TrimEndingDirectorySeparator(path2);
            }
            
            if (flags.HasFlag(LocationHelperPathCompareFlags.IgnoreCaseAlways)) {
                return path1.Equals(path2, StringComparison.InvariantCultureIgnoreCase);
            }
            if (flags.HasFlag(LocationHelperPathCompareFlags.IgnoreCaseNever)) {
                return path1.Equals(path2, StringComparison.InvariantCulture);
            }
#if PLATF_WINDOWS
            return path1.Equals(path2, StringComparison.InvariantCultureIgnoreCase);
#else
            return path1.Equals(path2, StringComparison.InvariantCulture);
#endif
        }
        
        #region FilePicker stuff


        public static IStorageFolder? BrowseFolderPicker(string title, string? suggestedDirectoryPath = null)
        {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanPickFolder == false)
                return null;

            IStorageFolder? baseStorageDirectory = null;
            if (string.IsNullOrWhiteSpace(suggestedDirectoryPath) == false)
            {
                Task<IStorageFolder?> task = storageProvider.TryGetFolderFromPathAsync(suggestedDirectoryPath);
                task.Wait();
                baseStorageDirectory = task.Result;
            }

            FolderPickerOpenOptions fpo = new FolderPickerOpenOptions() {
                Title = title,
                AllowMultiple = false,
                SuggestedStartLocation = baseStorageDirectory,
            };
            
            //
            //          Important NOTE:
            // Under OSX, IStorageFolder.OpenFolderPickerAsync must be called from the UI thread, otherwise 
            // it will crash with this error message:
            //      'NSWindow drag regions should only be invalidated on the Main Thread!'
            //
            // The funky thing about it is, after calling it on the UI thread I can't just wait for the
            // IStorageFolder.OpenFolderPickerAsync result since that would lock up the whole UI thread,
            // so instead I have to use an async function and properly await the OpenFolderPickerAsync...
            //
            
            IStorageFolder? result = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Dispatcher.UIThread.Invoke(async () => {
                try {
                    IReadOnlyList<IStorageFolder> results = await storageProvider.OpenFolderPickerAsync(fpo);
                    if (results.Count > 0)
                        result = results[0];
                } catch (Exception) {
                    result = null;
                }
                finally {
                    cancellationTokenSource.Cancel();
                }
            });
            while (cancellationTokenSource.IsCancellationRequested == false) {
                Thread.Sleep(50);
            }
            return result;
        }

        public static IStorageFile? BrowseFileOpen(string title, IReadOnlyList<FilePickerFileType>? fileTypes, string? suggestedFilePath = null)
        {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanOpen == false)
                return null;
            
            IStorageFolder? suggestedStorageDirectory = null;
            string? suggestedFileName = null;
            if (string.IsNullOrWhiteSpace(suggestedFilePath) == false)
            {
                Task<IStorageFile?> taskFile = storageProvider.TryGetFileFromPathAsync(suggestedFilePath);
                taskFile.Wait();
                
                Task<IStorageFolder?> taskDir = taskFile.Result != null
                    ? taskFile.Result.GetParentAsync()
                    : storageProvider.TryGetFolderFromPathAsync(suggestedFilePath);
                taskDir.Wait();

                if (taskFile.Result != null) { 
                    suggestedFileName = taskFile.Result.Name;
                } 
                
                if (taskDir.Result != null) {
                    suggestedStorageDirectory = taskDir.Result;
                }
            }
            
            FilePickerOpenOptions fpo = new FilePickerOpenOptions() {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
                SuggestedStartLocation = suggestedStorageDirectory,
                SuggestedFileName = suggestedFileName,
            };

            // call OpenFilePickerAsync fromt he UIThread to prevent crash on OSX
            IStorageFile? result = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Dispatcher.UIThread.Invoke(async () => {
                try {
                    IReadOnlyList<IStorageFile> results = await storageProvider.OpenFilePickerAsync(fpo);
                    if (results.Count > 0)
                        result = results[0];
                } catch (Exception) {
                    result = null;
                }
                finally {
                    cancellationTokenSource.Cancel();
                }
            });
            while (cancellationTokenSource.IsCancellationRequested == false) {
                Thread.Sleep(50);
            }
            return result;
        }
        
        public static IStorageFile? BrowseFileSave(string title, IReadOnlyList<FilePickerFileType>? fileTypes, string? suggestedFilePath = null)
        {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanSave == false)
                return null;
            
            IStorageFolder? suggestedStorageDirectory = null;
            string? suggestedFileName = null;
            if (string.IsNullOrWhiteSpace(suggestedFilePath) == false)
            {
                Task<IStorageFile?> taskFile = storageProvider.TryGetFileFromPathAsync(suggestedFilePath);
                taskFile.Wait();
                
                Task<IStorageFolder?> taskDir = taskFile.Result != null
                    ? taskFile.Result.GetParentAsync()
                    : storageProvider.TryGetFolderFromPathAsync(suggestedFilePath);
                taskDir.Wait();

                if (taskFile.Result != null) { 
                    suggestedFileName = taskFile.Result.Name;
                } 
                
                if (taskDir.Result != null) {
                    suggestedStorageDirectory = taskDir.Result;
                }
            }
            
            FilePickerSaveOptions fpo = new FilePickerSaveOptions() {
                Title = title,
                FileTypeChoices = fileTypes,
                SuggestedStartLocation = suggestedStorageDirectory,
                SuggestedFileName = suggestedFileName,
            };
            
            // call SaveFilePickerAsync fromt he UIThread to prevent crash on OSX 
            IStorageFile? result = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Dispatcher.UIThread.Invoke(async () => {
                try {
                    result = await storageProvider.SaveFilePickerAsync(fpo);
                } catch (Exception) {
                    result = null;
                }
                finally {
                    cancellationTokenSource.Cancel();
                }
            });
            while (cancellationTokenSource.IsCancellationRequested == false) {
                Thread.Sleep(50);
            }
            return result;
        }
        
        #endregion
    }
}
