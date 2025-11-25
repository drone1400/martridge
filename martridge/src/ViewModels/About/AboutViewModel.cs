using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.ViewModels.DinkyAlerts;
using Martridge.ViewModels.DinkyGraphics;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Martridge.Models.Configuration.General;

namespace Martridge.ViewModels.About {
    public class AboutViewModel : ViewModelAppPage
    {
        public List<AboutUsedPackageViewModel> UsedPackages {
            get => this._usedPackages;
            set => this.RaiseAndSetIfChanged(ref this._usedPackages, value);
        }
        private List<AboutUsedPackageViewModel> _usedPackages = new List<AboutUsedPackageViewModel>();
        
        public AnimatedDinkGraphicViewModel AnimatedMartridgeLeft {
            get => DinkyAlert.AnimatedMartridgeLeft;
        }
        
        public AnimatedDinkGraphicViewModel AnimatedMartridgeRight {
            get => DinkyAlert.AnimatedMartridgeRight;
        }

        //
        // General Configuration object...
        //
        public ConfigGeneral? Configuration { 
            get => this._cfg;
            set {
                if (this._cfg != null) {
                    this._cfg.Updated -= this.CfgOnUpdated;
                }
                this.RaiseAndSetIfChanged(ref this._cfg, value);
                
                if (this._cfg != null) {
                    this._cfg.Updated += this.CfgOnUpdated;
                    this.InitializeLocalizedPackageInfo();
                }
            }
        }
        private ConfigGeneral? _cfg = null;

        private string? _lastInitializedLanguage = null;
        private void CfgOnUpdated(object? sender, EventArgs e) {
            if (this._cfg?.LocalizationName != this._lastInitializedLanguage) {
                this.InitializeLocalizedPackageInfo();
            }
        }
        
        public string Version { get; }
        
        public void CmdOpenHyperlink(object? parameter = null) {
            if (parameter is not string str ||
                string.IsNullOrWhiteSpace(str) ||
                str.ToLowerInvariant().StartsWith("https://") == false)
                return;
            
            ProcessStartInfo pinfo = new ProcessStartInfo(str) {
                UseShellExecute = true,
                Verb = "open",
            };
            Process.Start(pinfo);
        }
        public bool CanCmdOpenHyperlink(object? parameter = null) {
            if (parameter is not string str ||
                string.IsNullOrWhiteSpace(str) ||
                str.ToLowerInvariant().StartsWith("https://") == false)
                return false;
            return true;
        }

        public AboutViewModel() {
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            // note, this should be impossible i think?...
            this.Version = version != null ? version.ToString() : "VersionError!!?!";
        }

        private void InitializeLocalizedPackageInfo() {
            this._lastInitializedLanguage = this._cfg?.LocalizationName;

            string linkWebsite = Localizer.Instance["AboutWindow/Package/LinkType/Website"];
            string linkSource = Localizer.Instance["AboutWindow/Package/LinkType/Source"];
            string linkOriginalSource = Localizer.Instance["AboutWindow/Package/LinkType/OriginalSource"];
            string linkModifiedSource = Localizer.Instance["AboutWindow/Package/LinkType/ModifiedSource"];
            
            
            this.UsedPackages = new List<AboutUsedPackageViewModel>() {
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/DinkNetwork/Name"],
                    Localizer.Instance["AboutWindow/Package/DinkNetwork/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkWebsite, "https://www.dinknetwork.com/"),
                    }),
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/DinkGraphics/Name"],
                    Localizer.Instance["AboutWindow/Package/DinkGraphics/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkWebsite, "https://www.dinknetwork.com/file/dink_smallwood_bmp_graphics"),
                    }),
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/Icons8/Name"],
                    Localizer.Instance["AboutWindow/Package/Icons8/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkWebsite, "https://icons8.com/"),
                    }),
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/AvaloniaUI/Name"],
                    Localizer.Instance["AboutWindow/Package/AvaloniaUI/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkWebsite, "https://avaloniaui.net/"),
                        new UsedPackageLinkViewModel(linkSource, "https://github.com/AvaloniaUI/Avalonia"),
                    }),
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/Citrus.Avalonia/Name"],
                    Localizer.Instance["AboutWindow/Package/Citrus.Avalonia/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkOriginalSource, "https://github.com/AvaloniaUI/Citrus.Avalonia"),
                        new UsedPackageLinkViewModel(linkModifiedSource, "https://github.com/drone1400/Citrus.Avalonia"),
                    }),
#if PLATF_WINDOWS && ENABLE_FEATURE_DINK_INSTALLER
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/SevenZipExtractor/Name"],
                    Localizer.Instance["AboutWindow/Package/SevenZipExtractor/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkSource, "https://github.com/adoconnection/SevenZipExtractor"),
                    }),
#endif
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/SharpCompress/Name"],
                    Localizer.Instance["AboutWindow/Package/SharpCompress/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkOriginalSource, "https://github.com/adamhathcock/sharpcompress"),
                        new UsedPackageLinkViewModel(linkModifiedSource, "https://github.com/drone1400/sharpcompress/tree/drone-modifications-2024"),
                    }),
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/Bzip2/Name"],
                    Localizer.Instance["AboutWindow/Package/Bzip2/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkOriginalSource,"https://github.com/jaime-olivares/bzip2"),
                        new UsedPackageLinkViewModel(linkModifiedSource, "https://github.com/drone1400/bzip2"),
                    }),
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/NewtonsoftJson/Name"],
                    Localizer.Instance["AboutWindow/Package/NewtonsoftJson/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkWebsite, "https://www.newtonsoft.com/json"),
                        new UsedPackageLinkViewModel(linkSource, "https://github.com/JamesNK/Newtonsoft.Json"),
                    }),
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/GetText/Name"],
                    Localizer.Instance["AboutWindow/Package/GetText/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkSource, "https://github.com/perpetualKid/GetText.NET"),
                    }),
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/Ignore/Name"],
                    Localizer.Instance["AboutWindow/Package/Ignore/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkSource, "https://github.com/goelhardik/ignore"),
                    }),
#if ENABLE_FEATURE_ONLINE
                new AboutUsedPackageViewModel(
                    Localizer.Instance["AboutWindow/Package/HtmlAgilityPack/Name"],
                    Localizer.Instance["AboutWindow/Package/HtmlAgilityPack/Description"],
                    new List<UsedPackageLinkViewModel>() {
                        new UsedPackageLinkViewModel(linkWebsite, "https://html-agility-pack.net/"),
                        new UsedPackageLinkViewModel(linkSource, "https://github.com/zzzprojects/html-agility-pack"),
                    }),
#endif
            };
        }
    }
}
