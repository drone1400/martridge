using System.Collections.Generic;
namespace Martridge.ViewModels.About {

    public class UsedPackageLinkViewModel {
        public string LinkType { get; }
        public string Link { get; }
        public UsedPackageLinkViewModel() {
            this.LinkType = "Website";
            this.Link = "https://127.0.0.1";
        }
        public UsedPackageLinkViewModel(string linkType, string link) {
            this.LinkType = linkType;
            this.Link = link;
        }
    }
    public class AboutUsedPackageViewModel {
        public string Name { get; }
        public string Description { get; }
        public List<UsedPackageLinkViewModel> Links { get; }

        public AboutUsedPackageViewModel() {
            this.Name = "placeholder";
            this.Description = "...";
            this.Links = new List<UsedPackageLinkViewModel>() { new UsedPackageLinkViewModel() };
        }

        public AboutUsedPackageViewModel(string name, string description, List<UsedPackageLinkViewModel> links) {
            this.Name = name;
            this.Description = description;
            this.Links = links;
        }
        
    }
}
