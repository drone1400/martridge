using Martridge.Models.Configuration;
using Martridge.Models.Dmod;
using Martridge.Models.Localization;
using Martridge.Models.OnlineDmods;
using Martridge.Trace;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Martridge.Models {
    public class AppLogic {

        public Config Config { get; } = new Config();

        public DmodManager DmodManager { get; private set; } = new DmodManager();
        public DmodCrawler DmodCrawler { get; private set; } = new DmodCrawler();

        private string ConfigFile { get; } 


        public AppLogic() {
            this.ConfigFile = Path.Combine(LocationHelper.AppBaseDirectory, "config", "config.json");
            System.Diagnostics.Trace.WriteLine("SOMETHING");
            this.Config.LoadFromFile(this.ConfigFile);

            // try to set loaded theme...
            string themeName = this.Config.General.ThemeName;
            if (Enum.TryParse(themeName, out ApplicationTheme themeValue)) {
                StyleManager.Instance.UseTheme(themeValue);
            }

            #if PLATF_LINUX
            
            string defaultLinuxFreedinkExe = "/usr/games/freedink";
            string defaultLinuxDinkGameData = "/usr/share/games/dink";
            string? defaultLinuxHome = Environment.GetEnvironmentVariable("HOME");
            string defaultLinuxDmods = Path.Combine(defaultLinuxHome ?? "", "dmods");

            if (File.Exists(defaultLinuxFreedinkExe) && 
                this.Config.General.GameExePaths.Contains(defaultLinuxFreedinkExe) == false) {
                this.Config.General.AddGameExePath(defaultLinuxFreedinkExe);
            }

            if (Directory.Exists(defaultLinuxDinkGameData) &&
                this.Config.General.AdditionalDmodLocations.Contains(defaultLinuxDinkGameData) == false) {
                this.Config.General.AddAdditionalDmodPath(defaultLinuxDinkGameData);
            }

            if (defaultLinuxHome != null &&
                Directory.Exists(defaultLinuxDmods) &&
                this.Config.General.AdditionalDmodLocations.Contains(defaultLinuxDmods) == false) {
                this.Config.General.AddAdditionalDmodPath(defaultLinuxDmods);
            }
            #endif
            
            this.Config.General.Updated += this.GeneralOnUpdated;
            this.Config.Launch.Updated += this.LaunchOnUpdated;

            this.DmodManager.Initialize(this.Config.General);
        }
        private void LaunchOnUpdated(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationChanged"]);
            this.Config.SaveToFile(this.ConfigFile);
        }

        private void GeneralOnUpdated(object? sender, ConfigUpdateEventArgs e) {
            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationChanged"]);
            this.Config.SaveToFile(this.ConfigFile);
            
            
            // reinitialize dmod manager if need be..
            foreach (string name in e.UpdatedProperties) {
                if (name == nameof(ConfigGeneral.AdditionalDmodLocations) ||
                    name == nameof(ConfigGeneral.DefaultDmodLocation) ||
                    name == nameof(ConfigGeneral.GameExePaths)) {
                    this.DmodManager.Initialize(this.Config.General);
                    break;
                }
            }
        }
    }
}
