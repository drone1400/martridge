using Martridge.Models.Configuration;
using Martridge.Models.Localization;
using Martridge.ViewModels.DinkyAlerts;
using Martridge.ViewModels.DinkyGraphics;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Martridge.ViewModels.About {
    public class AboutWindowViewModel : ViewModelBase {

        public List<AboutUsedPackageViewModel> UsedPackages {
            get => this._usedPackages;
            set => this.RaiseAndSetIfChanged(ref this._usedPackages, value);
        }
        private List<AboutUsedPackageViewModel> _usedPackages = new List<AboutUsedPackageViewModel>();
        
        public AnimatedDinkGraphicViewModel? AnimatedMartridgeLeft {
            get => DinkyAlert.AnimatedMartridgeLeft;
        }
        
        public AnimatedDinkGraphicViewModel? AnimatedMartridgeRight {
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

        public AboutWindowViewModel() {
            this.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public void InitializeLocalizedPackageInfo() {
            this._lastInitializedLanguage = this._cfg?.LocalizationName;
            
            this.UsedPackages = new List<AboutUsedPackageViewModel>() {
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/DinkNetwork/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/DinkNetwork/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/DinkGraphics/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/DinkGraphics/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/AvaloniaUI/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/AvaloniaUI/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/Citrus.Avalonia/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/Citrus.Avalonia/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/SevenZipExtractor/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/SevenZipExtractor/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/SharpCompress/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/SharpCompress/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/Bzip2/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/Bzip2/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/NewtonsoftJson/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/NewtonsoftJson/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/GetText/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/GetText/Description"]),
                new AboutUsedPackageViewModel(
                    name: Localizer.Instance[@"AboutWindow/Package/HtmlAgilityPack/Name"],
                    info: Localizer.Instance[@"AboutWindow/Package/HtmlAgilityPack/Description"]),
            };
        }
    }
}
