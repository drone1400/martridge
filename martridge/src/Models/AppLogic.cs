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
            this.Config.General.UpdatedActiveGameExe += this.GeneralOnUpdatedActiveGameExe;
            this.Config.General.UpdatedActiveEditorExe += this.GeneralOnUpdatedActiveEditorExe;
            this.Config.Launch.Updated += this.LaunchOnUpdated;

            this.DmodManager.Initialize(this.Config.General);
        }
        private void LaunchOnUpdated(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationChanged"]);
            this.Config.SaveToFile(this.ConfigFile);
        }

        private void GeneralOnUpdatedActiveGameExe(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationChangedActiveGameExe"]);
            this.Config.SaveToFile(this.ConfigFile);
        }
        
        private void GeneralOnUpdatedActiveEditorExe(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationChangedActiveEditorExe"]);
            this.Config.SaveToFile(this.ConfigFile);
        }

        private void GeneralOnUpdated(object? sender, EventArgs e) {
            MyTrace.Global.WriteMessage(MyTraceCategory.General, Localizer.Instance[@"General/ConfigurationChanged"]);
            this.Config.SaveToFile(this.ConfigFile);

            // reinitialize dmod manager..
            this.DmodManager.Initialize(this.Config.General);
        }
    }
}
