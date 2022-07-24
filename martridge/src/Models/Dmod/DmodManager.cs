using Martridge.Models.Configuration;
using Martridge.Trace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Martridge.Models.Dmod {
    public class DmodManager {

        public event EventHandler? DmodListInitialized;

        public List<DmodFileDefinition> DmodList { get => this._dmodList; }
        private List<DmodFileDefinition> _dmodList = new List<DmodFileDefinition>();

        public void Initialize(ConfigGeneral cfg) {
            Task task = new Task(() => { 
                try {
                    List<DmodFileDefinition> dmodList = new List<DmodFileDefinition>();

                    List<DirectoryInfo> directories = cfg.GetRealDmodDirectories();

                    foreach (DirectoryInfo dir in directories) {
                        ScanDirectoryForDmods(dir, dmodList);
                    }

                    this._dmodList = dmodList;
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DmodBrowser, ex);
                } finally {
                    this.DmodListInitialized?.Invoke(this, EventArgs.Empty);
                }
            });
            task.Start();
        }

        private static void ScanDirectoryForDmods(DirectoryInfo dirInfo, List<DmodFileDefinition> dmodList) {
            dirInfo.Refresh();
            if (dirInfo.Exists == false) {
                return;
            }

            DirectoryInfo[] subdirs = dirInfo.GetDirectories();

            foreach (DirectoryInfo dir in subdirs) {
                DmodFileDefinition dfd = new DmodFileDefinition(dir.FullName);
                if (dfd.IsCorrectlyDefined) {
                    // found a dmod! add it to the list...
                    dmodList.Add(dfd);
                }
            }

        }
    }
}
