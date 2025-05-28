using ReactiveUI;
using System;
using System.ComponentModel;
using Martridge.Models.Configuration;
using Martridge.Trace;

namespace Martridge.ViewModels {
    public abstract class ViewModelAppPageWithCfg : ViewModelAppPage{

        public ConfigGeneral? CfgGeneral {
            get => this._cfgGeneral;
            set => this.RaiseAndSetIfChanged(ref this._cfgGeneral, value);
        }
        private ConfigGeneral? _cfgGeneral = null;
        
        protected virtual void OnConfigGeneralChanging() {}
        protected virtual void OnConfigGeneralChanged() {}
        protected virtual void OnCfgGeneralUpdated(object? sender, ConfigUpdateEventArgs e) {}
        
        public ConfigLaunch? CfgLaunch {
            get => this._cfgLaunch;
            set => this.RaiseAndSetIfChanged(ref this._cfgLaunch, value);
        }
        private ConfigLaunch? _cfgLaunch = null;
        
        protected virtual void OnConfigLaunchChanging() {}
        protected virtual void OnConfigLaunchChanged() {}
        protected virtual void OnCfgLaunchUpdated(object? sender, ConfigUpdateEventArgs e) {}
        
        public ConfigRemember? CfgRemember {
            get => this._cfgRemember;
            set => this.RaiseAndSetIfChanged(ref this._cfgRemember, value);
        }
        private ConfigRemember? _cfgRemember = null;
        
        protected virtual void OnConfigRememberChanging() {}
        protected virtual void OnConfigRememberChanged() {}
        protected virtual void OnCfgRememberUpdated(object? sender, ConfigUpdateEventArgs e) {}

        public ViewModelAppPageWithCfg() {
            this.PropertyChanging += this.OnPropertyChanging;
            this.PropertyChanged += this.OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            try {
                switch (e.PropertyName) {
                    case nameof(this.CfgGeneral): {
                        if (this.CfgGeneral != null) {
                            this.CfgGeneral.Updated += this.OnCfgGeneralUpdated;
                        }
                        this.OnConfigGeneralChanged();
                        break;
                    }
                    case nameof(this.CfgLaunch): {
                        if (this.CfgLaunch != null) {
                            this.CfgLaunch.Updated += this.OnCfgLaunchUpdated;
                        }
                        this.OnConfigLaunchChanged();
                        break;
                    }
                    case nameof(this.CfgRemember): {
                        if (this.CfgRemember != null) {
                            this.CfgRemember.Updated += this.OnCfgRememberUpdated;
                        }
                        this.OnConfigRememberChanged();
                        break;
                    }
                    
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e) {
            try {
                switch (e.PropertyName) {
                    case nameof(this.CfgGeneral): {
                        if (this.CfgGeneral != null) {
                            this.CfgGeneral.Updated -= this.OnCfgGeneralUpdated;
                        }
                        this.OnConfigGeneralChanging();
                        break;
                    }
                    case nameof(this.CfgLaunch): {
                        if (this.CfgLaunch != null) {
                            this.CfgLaunch.Updated -= this.OnCfgLaunchUpdated;
                        }
                        this.OnConfigLaunchChanging();
                        break;
                    }
                    case nameof(this.CfgRemember): {
                        if (this.CfgRemember != null) {
                            this.CfgRemember.Updated -= this.OnCfgRememberUpdated;
                        }
                        this.OnConfigRememberChanging();
                        break;
                    }
                }
            } catch (Exception ex) {
                MyTrace.Global.WriteException(ex);
            }
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            this.CfgGeneral = null;
            this.CfgLaunch = null;
            this.CfgRemember = null;
        }
    }
}
