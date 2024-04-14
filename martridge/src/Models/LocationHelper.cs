using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
    }
}
