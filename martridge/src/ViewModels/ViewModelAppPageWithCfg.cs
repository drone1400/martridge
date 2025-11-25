using Martridge.Models.Configuration;
using Martridge.Models.Configuration.AppState;
using Martridge.Models.Configuration.General;
using Martridge.Models.Configuration.LaunchExtension;

namespace Martridge.ViewModels {
    public abstract class ViewModelAppPageWithCfg : ViewModelAppPage {

        public ConfigGeneral CfgGeneral => Config.Instance.General;
        protected virtual void OnCfgGeneralUpdated(object? sender, ConfigUpdateEventArgs e) {}

        public ConfigLaunch CfgLaunch => Config.Instance.Launch;
        protected virtual void OnCfgLaunchUpdated(object? sender, ConfigUpdateEventArgs e) {}
        
        public ConfigAppState CfgAppState => Config.Instance.AppState;
        protected virtual void OnCfgAppStateUpdated(object? sender, ConfigUpdateEventArgs e) {}


        public ConfigExtension CfgExtension => Config.Instance.LaunchExtension;
        protected virtual void OnCfgExtensionUpdated(object? sender, ConfigUpdateEventArgs e) {}


        protected ViewModelAppPageWithCfg() {
            this.CfgGeneral.Updated += this.OnCfgGeneralUpdated;
            this.CfgLaunch.Updated += this.OnCfgLaunchUpdated;
            this.CfgAppState.Updated += this.OnCfgAppStateUpdated;
            //this.CfgExtension.Updated += this.OnCfgExtensionUpdated;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            this.CfgGeneral.Updated -= this.OnCfgGeneralUpdated;
            this.CfgLaunch.Updated -= this.OnCfgLaunchUpdated;
            this.CfgAppState.Updated -= this.OnCfgAppStateUpdated;
            //this.CfgExtension.Updated -= this.OnCfgExtensionUpdated;
        }
    }
}
