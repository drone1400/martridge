using System.IO;
using ReactiveUI;

namespace Martridge.ViewModels.Dmod; 

public class DmodLauncherSelectionViewModel : ViewModelBase{
    public string Path {
        get => this._path;
        set => this.RaiseAndSetIfChanged(ref this._path, value);
    }
    private string _path= "";
    
    public string DisplayName {
        get => this._displayName;
        set => this.RaiseAndSetIfChanged(ref this._displayName, value);
    }
    private string _displayName= "";

    public bool PathExists => File.Exists(this.Path);
}
