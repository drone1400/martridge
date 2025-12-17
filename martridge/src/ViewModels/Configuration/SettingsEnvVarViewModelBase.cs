using Martridge.Models.Configuration.Generic;
using Martridge.Models.Configuration.Generic.FileData;
using Martridge.Models.Localization;
using ReactiveUI;
namespace Martridge.ViewModels.Configuration {
    public abstract class SettingsEnvVarViewModelBase : ViewModelBase {
        /// <summary>
        /// The name of the environment variable
        /// </summary>
        public string Key {
            get => this._key;
            set => this.RaiseAndSetIfChanged(ref this._key, value);
        }
        protected string _key = string.Empty;

        /// <summary>
        /// The value of the environment variable
        /// </summary>
        public string Value {
            get => this._value;
            set => this.RaiseAndSetIfChanged(ref this._value, value);
        }
        protected string _value = string.Empty;

        /// <summary>
        /// If true, Martridge will use this environment variable when launching
        /// </summary>
        public bool IsEnabled {
            get => this._isEnabled;
            set => this.RaiseAndSetIfChanged(ref this._isEnabled, value);
        }
        protected bool _isEnabled = true;

        /// <summary>
        /// User description for environment variable
        /// </summary>
        public string Description {
            get => this._description;
            set => this.RaiseAndSetIfChanged(ref this._description, value);
        }
        protected string _description = string.Empty;

        public abstract ConfigDataEnvironmentVariable GetConfigData();

        public abstract ConfigEnvironmentVariable GetConfig() ;
    }
}
