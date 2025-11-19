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

        /// <summary>
        /// Get the path where Martridge resides
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathMartridge() => AppBaseDirectory;

        /// <summary>
        /// Get the path where Martridge should point opened File Browser windows to by default
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathDefaultFileBrowser() => PathMartridgeDefaultFileBrowser;

        /// <summary>
        /// Get the path where Martridge should try to install Dink by default (only used for Windows for now...)
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathDefaultDinkInstall() => PathMartridgeDefaultDinkInstall;

        /// <summary>
        /// Get the path where Martridge should write/read config files. Also tries to create the directory if it does not exist.
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathConfig() {
            if (Directory.Exists(PathMartridgeConfig) == false) {
                Directory.CreateDirectory(PathMartridgeConfig);
            }
            return PathMartridgeConfig;
        }

        /// <summary>
        /// Get the path where Martridge should read external localization files
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathLocalization() => PathMartridgeLocalization;

        /// <summary>
        /// Get the path where Martridge should read external custom theme files
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathCustomThemes() => PathMartridgeCustomThemes;

        
        /// <summary>
        /// Get the path where Martridge should write/read log files. Also tries to create the directory if it does not exist.
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathLogs() {
            if (Directory.Exists(PathMartridgeLogs) == false) {
                Directory.CreateDirectory(PathMartridgeLogs);
            }
            return PathMartridgeLogs;
        }

        /// <summary>
        /// Get the path where Martridge should write/read webcache stuff. Also tries to create the directory if it does not exist.
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathWebCache() {
            if (Directory.Exists(PathMartridgeWebCache) == false) {
                Directory.CreateDirectory(PathMartridgeWebCache);
            }
            return PathMartridgeWebCache;
        }

        /// <summary>
        /// Get the path where Martridge should write/read application state config files. Also tries to create the directory if it does not exist.
        /// </summary>
        /// <returns>Path string</returns>
        public static string GetPathMartridgeState() {
            if (Directory.Exists(PathMartridgeState) == false) {
                Directory.CreateDirectory(PathMartridgeState);
            }
            return PathMartridgeState;
        }


        private static string AppBaseDirectory { get; }

        private static string PathMartridgeState { get; } = string.Empty;
        private static string PathMartridgeConfig { get; } = string.Empty;
        private static string PathMartridgeLogs { get; } = string.Empty;
        private static string PathMartridgeWebCache { get; } = string.Empty;
        private static string PathMartridgeLocalization { get; } = string.Empty;
        private static string PathMartridgeCustomThemes { get; } = string.Empty;
        private static string PathMartridgeDefaultDinkInstall { get; } = string.Empty;
        private static string PathMartridgeDefaultFileBrowser { get; } = string.Empty;
        
        
        private const string ENV_VAR_HOME = "%HOME%";
        private const string ENV_VAR_XDG_CONFIG_HOME = "%XDG_CONFIG_HOME%";
        private const string ENV_VAR_XDG_DATA_HOME = "%XDG_DATA_HOME%";
        private const string ENV_VAR_XDG_STATE_HOME = "%XDG_STATE_HOME%";
        private const string ENV_VAR_XDG_CACHE_HOME = "%XDG_CACHE_HOME%";

        static LocationHelper() {
            bool arePathsInitialzied = false;
            
            bool tryLinuxDefaultPaths = false;
            
            #if PLATF_LINUX
            
            tryLinuxDefaultPaths = true;

            #endif

            if (tryLinuxDefaultPaths) {
                string home = TryGetHomeDirectory();
                bool canFallback = string.IsNullOrWhiteSpace(home) == false;
                
                // System.Environment.SpecialFolder.ApplicationData
                string homeConfig = Environment.ExpandEnvironmentVariables(ENV_VAR_XDG_CONFIG_HOME);
                if (canFallback && homeConfig == ENV_VAR_XDG_CONFIG_HOME) homeConfig = Path.Combine(home, ".config");

                // System.Environment.SpecialFolder
                string homeData = Environment.ExpandEnvironmentVariables(ENV_VAR_XDG_DATA_HOME);
                if (canFallback && homeData == ENV_VAR_XDG_DATA_HOME) homeData = Path.Combine(home, ".local","share");
                
                string homeState = Environment.ExpandEnvironmentVariables(ENV_VAR_XDG_STATE_HOME);
                if (canFallback && homeState == ENV_VAR_XDG_STATE_HOME) homeState = Path.Combine(home, ".local","state");

                string homeCache = Environment.ExpandEnvironmentVariables(ENV_VAR_XDG_CACHE_HOME);
                if (canFallback && homeCache == ENV_VAR_XDG_CACHE_HOME) homeCache = Path.Combine(home, ".cache");

                if (string.IsNullOrWhiteSpace(home) == false &&
                    string.IsNullOrWhiteSpace(homeConfig) == false &&
                    string.IsNullOrWhiteSpace(homeData) == false &&
                    string.IsNullOrWhiteSpace(homeState) == false &&
                    string.IsNullOrWhiteSpace(homeCache) == false ) {
                    // can initialize all the paths...

                    PathMartridgeState = Path.Combine(homeState, "martridge");
                    PathMartridgeConfig = Path.Combine(homeConfig, "martridge");
                    //PathMartridgeWebCache = Path.Combine(homeCache, "martridge", "webcache");
                    PathMartridgeWebCache = Path.Combine(homeData, "martridge", "webcache"); // the webcache data seems important enough to actually store in data instead... 
                    PathMartridgeLogs = Path.Combine(homeState, "martridge", "logs");
                    PathMartridgeLocalization = Path.Combine(homeData, "martridge", "localization");
                    PathMartridgeCustomThemes = Path.Combine(homeData, "martridge", "custom-themes");
                    PathMartridgeDefaultDinkInstall = Path.Combine(homeData, "martridge");
                    PathMartridgeDefaultFileBrowser = home;

                    arePathsInitialzied = true;
                }
            }
            
            {
                //
                // initialize AppBaseDirectory
                //
                
                AppBaseDirectory = "";

                string? processFile = Process.GetCurrentProcess().MainModule?.FileName;

                if (processFile == null) {
                    NullReferenceException ex = new NullReferenceException("Could not determine current process start location...");
                    MyTrace.Global.WriteException(ex);
                }
                else {

                    FileInfo finfo = new FileInfo(processFile);

                    if (finfo.DirectoryName == null) {
                        NullReferenceException ex = new NullReferenceException("Could not determine current process start location...");
                        MyTrace.Global.WriteException(ex);
                    }
                    else {
                        AppBaseDirectory = finfo.DirectoryName;
                    }
                }

                if (string.IsNullOrWhiteSpace(AppBaseDirectory)) {
                    AppBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    //AppBaseDirectory = AppContext.BaseDirectory;
                }
            }

            //
            // initialize paths locally if they were not initialized before
            //

            if (arePathsInitialzied == false && string.IsNullOrWhiteSpace(AppBaseDirectory) == false) {
                PathMartridgeState = Path.Combine(AppBaseDirectory, "config");
                PathMartridgeConfig = Path.Combine(AppBaseDirectory, "config");
                PathMartridgeWebCache = Path.Combine(AppBaseDirectory, "webcache");
                PathMartridgeLogs = Path.Combine(AppBaseDirectory, "logs");
                PathMartridgeLocalization = Path.Combine(AppBaseDirectory, "localization");
                PathMartridgeCustomThemes = Path.Combine(AppBaseDirectory, "custom-themes");
                PathMartridgeDefaultDinkInstall = AppBaseDirectory;
                PathMartridgeDefaultFileBrowser = AppBaseDirectory;

                arePathsInitialzied = true;
            }

            if (arePathsInitialzied == false) {
                IOException ex = new IOException("Could not initialize Martridge critical file paths...");
                MyTrace.Global.WriteException(ex);
                throw ex;
            }
        }

        public static string TryGetHomeDirectory() {
            string home = Environment.ExpandEnvironmentVariables(ENV_VAR_HOME);
            if (home != ENV_VAR_HOME) return home;
            
            string homeUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (Directory.Exists(homeUserProfile)) return homeUserProfile;
            
#if !PLATF_WINDOWS
            //  NOTE: on windows this actually resolves to 
            //      C:\Users\<USER_NAME>\Documents
            //  instead of the desired C:\Users\<USER_NAME>
            string homePersonal = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            if (Directory.Exists(homePersonal)) return homePersonal;
#endif
            return string.Empty;
        }


        public static string TryMakePathRelativeToMartridge(string path) {
            // check if we know where Martridge's base directory is
            if (string.IsNullOrWhiteSpace(AppBaseDirectory))
                return path;
            
            // check if the path is rooted
            if (Path.IsPathRooted(path) == false)
                return path;

            if (Path.GetPathRoot(AppBaseDirectory) != Path.GetPathRoot(path))
                return path;
            
            string newPath = Path.GetRelativePath(AppBaseDirectory, path);
            
            if (newPath.StartsWith("..")) {
                // if this is not a subdirectory, return back the original path
                return path;
            }
            
            return "." + Path.DirectorySeparatorChar + newPath;
        }

        public static string TryMakePathAbsoluteBasedOnMartridge(string path) {
            // only make path absolute if it is explicitly relative to the <current directory>
            // this is to prevent interpreting something like a Steam URI or flatpak command or other things as file paths
            if (path.StartsWith("." + Path.DirectorySeparatorChar) == false &&
                path.StartsWith("." + Path.AltDirectorySeparatorChar) == false)
                return path;
            
            // check if we know where Martridge's base directory is
            if (string.IsNullOrWhiteSpace(AppBaseDirectory))
                return path;
            
            // check if path already rooted...
            if (Path.IsPathRooted(path))
                return path;
            
            return Path.Combine(AppBaseDirectory, path);
        }

        public static bool PathIsEqual(string? path1, string? path2, LocationHelperPathCompareFlags flags = LocationHelperPathCompareFlags.None) {
            if (path1 == null || path2 == null) return false;
            if (flags.HasFlag(LocationHelperPathCompareFlags.IgnoreDirectorySeparator)) {
                path1 = Path.TrimEndingDirectorySeparator(path1);
                path2 = Path.TrimEndingDirectorySeparator(path2);
            }
            
            // this should resolve any \ / issues on Windows...
            path1 = Path.GetFullPath(path1);
            path2 = Path.GetFullPath(path2);

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
        
        
        public static bool PathIsDuplicate(IEnumerable<string> list, string path) {
            foreach (string s in list) {
                if (LocationHelper.PathIsEqual(s, path)) return true;
            }
            return false;
        }

        #region FilePicker stuff

        public static IStorageFolder? BrowseFolderPicker(string title, string? suggestedDirectoryPath = null) {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanPickFolder == false)
                return null;

            //
            //          Important NOTE:
            // Under OSX, IStorageFolder.OpenFolderPickerAsync must be called from the UI thread, otherwise 
            // it will crash with this error message:
            //      'NSWindow drag regions should only be invalidated on the Main Thread!'
            //
            // Under Linux, IStorageProvider.TryGetFolderFromPathAsync and IStorageProvider.TryGetFileFromPathAsync
            // also seem to need to run under the UI thread...
            //
            // The funky thing about it is, after calling it on the UI thread I can't just wait for the
            // IStorageFolder.OpenFolderPickerAsync result since that would lock up the whole UI thread,
            // so instead I have to use an async function and properly await the OpenFolderPickerAsync...
            //

            IStorageFolder? result = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Dispatcher.UIThread.Invoke(async () => {
                try {
                    IStorageFolder? baseStorageDirectory = null;
                    if (string.IsNullOrWhiteSpace(suggestedDirectoryPath) == false) {
                        baseStorageDirectory = await storageProvider.TryGetFolderFromPathAsync(suggestedDirectoryPath);
                    }

                    FolderPickerOpenOptions fpo = new FolderPickerOpenOptions() {
                        Title = title,
                        AllowMultiple = false,
                        SuggestedStartLocation = baseStorageDirectory,
                    };

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

        public static IStorageFile? BrowseFileOpen(string title, IReadOnlyList<FilePickerFileType>? fileTypes, string? suggestedFilePath = null) {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanOpen == false)
                return null;

            // call IStorageProvider related functions from the UIThread to prevent crash on OSX/Linux
            IStorageFile? result = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Dispatcher.UIThread.Invoke(async () => {
                try {
                    IStorageFolder? suggestedStorageDirectory = null;
                    string? suggestedFileName = null;

                    if (string.IsNullOrWhiteSpace(suggestedFilePath) == false) {
                        IStorageFile? resultFile = await storageProvider.TryGetFileFromPathAsync(suggestedFilePath);
                        IStorageFolder? resultFolder = resultFile != null
                            ? await resultFile.GetParentAsync()
                            : await storageProvider.TryGetFolderFromPathAsync(suggestedFilePath);

                        if (resultFile != null) {
                            suggestedFileName = resultFile.Name;
                        }

                        if (resultFolder != null) {
                            suggestedStorageDirectory = resultFolder;
                        }
                    }

                    FilePickerOpenOptions fpo = new FilePickerOpenOptions() {
                        Title = title,
                        AllowMultiple = false,
                        FileTypeFilter = fileTypes,
                        SuggestedStartLocation = suggestedStorageDirectory,
                        SuggestedFileName = suggestedFileName,
                    };


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

        public static IStorageFile? BrowseFileSave(string title, IReadOnlyList<FilePickerFileType>? fileTypes, string? suggestedFilePath = null) {
            if (App.Instance?.StorageProvider is not IStorageProvider storageProvider ||
                storageProvider.CanSave == false)
                return null;

            // call IStorageProvider related functions from the UIThread to prevent crash on OSX/Linux
            IStorageFile? result = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Dispatcher.UIThread.Invoke(async () => {
                try {
                    IStorageFolder? suggestedStorageDirectory = null;
                    string? suggestedFileName = null;
                    if (string.IsNullOrWhiteSpace(suggestedFilePath) == false) {
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
