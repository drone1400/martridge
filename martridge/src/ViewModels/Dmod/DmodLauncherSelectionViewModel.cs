using System;
using System.IO;
using Martridge.Models;

namespace Martridge.ViewModels.Dmod; 

public class DmodLauncherSelectionViewModel : ViewModelBase{

    public string PathRaw { get; }

    public string Path { get; }

    public string DisplayName { get; }

    public bool PathIsFile { get; } = false;

    public DmodLauncherSelectionViewModel(string rawPath) {
        this.PathRaw = rawPath;
        this.Path = rawPath;

        if (this.PathRaw.StartsWith("." + System.IO.Path.DirectorySeparatorChar) ||
            this.PathRaw.StartsWith("." + System.IO.Path.AltDirectorySeparatorChar)) {
            // this seems to be a relative file path, try to make it absolute...

            this.Path = LocationHelper.TryMakePathAbsoluteBasedOnMartridge(this.Path);
        }

        this.DisplayName = this.Path;

        try {
            FileInfo fileInfo = new FileInfo(this.Path);

            if (fileInfo.Exists) {
                this.PathIsFile = true;

                string dirName = fileInfo.Directory?.Name ?? string.Empty;
                
                this.DisplayName = string.IsNullOrWhiteSpace(dirName)
                    ? fileInfo.Name
                    : dirName + System.IO.Path.DirectorySeparatorChar +  fileInfo.Name;
            }
        } catch (Exception) {
            // ignore any errors
        }
    }
}
