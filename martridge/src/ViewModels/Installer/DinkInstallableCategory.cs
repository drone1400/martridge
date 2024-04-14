using System.Collections.ObjectModel;
using Martridge.Models.Configuration;
namespace Martridge.ViewModels.Installer {
    public class DinkInstallableCategory : ViewModelBase {
        
        public string CategoryName { get; }

        public ObservableCollection<DinkInstallableEntry> InstallerEntries {
            get;
        } = new ObservableCollection<DinkInstallableEntry>();

        public DinkInstallableCategory(string name) {
            this.CategoryName = name;
        }

        public void AddDinkInstallerdata(ConfigInstaller data) {
            if (this.CategoryName == data.Category) {
                DinkInstallableEntry? oldEntry = null;
                foreach (var x in this.InstallerEntries) {
                    if (x.InstallerData.Name == data.Name) {
                        oldEntry = x;
                        break;
                    }
                }
                if (oldEntry != null) {
                    this.InstallerEntries.Remove(oldEntry);
                }
                
                this.InstallerEntries.Add(new DinkInstallableEntry(data));
            }
        }

    }
}
