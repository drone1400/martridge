namespace Martridge.ViewModels.About {
    public class AboutUsedPackageViewModel : ViewModelBase{
        public string PackageName { get; }
        public string PackageInfo { get; }

        public AboutUsedPackageViewModel(string name, string info) {
            this.PackageName = name;
            this.PackageInfo = info;
        }
        
    }
}
