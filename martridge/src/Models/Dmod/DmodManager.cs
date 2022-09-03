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
                    Dictionary<string, DmodFileDefinition> tempAddedDirectories = new Dictionary<string, DmodFileDefinition>();
                    List<DmodFileDefinition> tempSymbolicLinks = new List<DmodFileDefinition>();

                    List<DirectoryInfo> directories = cfg.GetRealDmodDirectories();

                    foreach (DirectoryInfo dir in directories) {
                        ScanDirectoryForDmods(dir, tempAddedDirectories, tempSymbolicLinks);
                    }

                    foreach (DmodFileDefinition dfd in tempSymbolicLinks) {
                        if (dfd.DmodRoot.LinkTarget != null && tempAddedDirectories.ContainsKey(dfd.DmodRoot.LinkTarget) == false) {
                            tempAddedDirectories.Add(dfd.DmodRoot.LinkTarget, dfd);
                        }
                    }

                    List<DmodFileDefinition> dmodList = new List<DmodFileDefinition>();

                    foreach (var kvp in tempAddedDirectories) {
                        dmodList.Add(kvp.Value);
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

        private static void ScanDirectoryForDmods(DirectoryInfo dirInfo, Dictionary<string, DmodFileDefinition> normalDmodDirs, List<DmodFileDefinition> symbolicLinkDmods) {
            dirInfo.Refresh();
            if (dirInfo.Exists == false) {
                return;
            }

            DirectoryInfo[] subdirs = dirInfo.GetDirectories();

            foreach (DirectoryInfo dir in subdirs) {
                DmodFileDefinition dfd = new DmodFileDefinition(dir.FullName);
                if (dfd.IsCorrectlyDefined) {
                    // found a dmod! add it to the list...
                    if (dfd.DmodRoot.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
                        // add symbolic links to separate list for later processing
                        symbolicLinkDmods.Add(dfd);
                    } else {
                        // add non duplicate normal directories to dictionary
                        if (normalDmodDirs.ContainsKey(dfd.DmodRoot.FullName) == false) {
                            normalDmodDirs.Add(dfd.DmodRoot.FullName, dfd);
                        }
                    }
                }
            }

        }
    }
}
