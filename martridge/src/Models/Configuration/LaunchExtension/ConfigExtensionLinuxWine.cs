using Martridge.Models.Configuration.LaunchExtension.FileData;
namespace Martridge.Models.Configuration.LaunchExtension {
    public class ConfigExtensionLinuxWine {

        /// <summary>
        /// Value for overriding WINEVERPATH environment variable
        /// </summary>
        public string WINEVERPATH => this._WINEVERPATH;
        private string _WINEVERPATH = string.Empty;
        
        /// <summary>
        /// Path to WINE's /bin/ dir, should be same as parent dir for WINESERVER and WINELOADER... if they are somehow different, should be two paths separated by :
        /// </summary>
        public string WINEBINPATH => this._WINEBINPATH;
        private string _WINEBINPATH = string.Empty;
        
        /// <summary>
        /// Path to WINE's /lib/ dir
        /// </summary>
        public string WINELIBPATH => this._WINELIBPATH;
        private string _WINELIBPATH = string.Empty;
        
        /// <summary>
        /// Value for overriding WINESERVER environment variable
        /// </summary>
        public string WINESERVER => this._WINESERVER;
        private string _WINESERVER = string.Empty;
        
        /// <summary>
        /// Value for overriding WINELOADER environment variable
        /// </summary>
        public string WINELOADER => this._WINELOADER;
        private string _WINELOADER = string.Empty;
        
        /// <summary>
        /// Value for overriding WINEDLLPATH environment variable
        /// </summary>
        public string WINEDLLPATH => this._WINEDLLPATH;
        private string _WINEDLLPATH = string.Empty;
        
        /// <summary>
        /// Value for overriding WINEPREFIX environment variable
        /// </summary>
        public string WINEPREFIX => this._WINEPREFIX;
        private string _WINEPREFIX = string.Empty;
        
        public ConfigExtensionLinuxWine() {}
        
        public ConfigExtensionLinuxWine(ConfigDataExtensionLinuxWine data) {
            this._WINEVERPATH =  data.WINEVERPATH ?? string.Empty;
            this._WINEBINPATH =  data.WINEBINPATH ?? string.Empty;
            this._WINELIBPATH =  data.WINELIBPATH ?? string.Empty;
            this._WINESERVER =  data.WINESERVER ?? string.Empty;
            this._WINELOADER =  data.WINELOADER ?? string.Empty;
            this._WINEDLLPATH =  data.WINEDLLPATH ?? string.Empty;
            this._WINEPREFIX =  data.WINEPREFIX ?? string.Empty;
        }
        
        public ConfigExtensionLinuxWine(string wineVerPath, string wineBinPath, string wineLibPath, string wineServer, string wineLoader, string wineDllPath, string winePrefix) {
            this._WINEVERPATH = wineVerPath;
            this._WINEBINPATH = wineBinPath;
            this._WINELIBPATH = wineLibPath;
            this._WINESERVER = wineServer;
            this._WINELOADER = wineLoader;
            this._WINEDLLPATH = wineDllPath;
            this._WINEPREFIX = winePrefix;
        }
        
        public void SetFromData(ConfigDataExtensionLinuxWine data) {
            this._WINEVERPATH =  data.WINEVERPATH ?? string.Empty;
            this._WINEBINPATH =  data.WINEBINPATH ?? string.Empty;
            this._WINELIBPATH =  data.WINELIBPATH ?? string.Empty;
            this._WINESERVER =  data.WINESERVER ?? string.Empty;
            this._WINELOADER =  data.WINELOADER ?? string.Empty;
            this._WINEDLLPATH =  data.WINEDLLPATH ?? string.Empty;
            this._WINEPREFIX =  data.WINEPREFIX ?? string.Empty;
        }

        public ConfigDataExtensionLinuxWine GetData() {
            return new ConfigDataExtensionLinuxWine() {
                WINEVERPATH =  this._WINEVERPATH,
                WINEBINPATH = this._WINEBINPATH,
                WINELIBPATH = this._WINELIBPATH,
                WINESERVER = this._WINESERVER,
                WINELOADER = this._WINELOADER,
                WINEDLLPATH = this._WINEDLLPATH,
                WINEPREFIX = this._WINEPREFIX,
            };
        }
    }
}
