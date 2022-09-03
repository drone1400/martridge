using Martridge.Models.Configuration.Save;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace Martridge.Models.Configuration {
    public class ConfigAlertResults {
        public event EventHandler? Updated;
        
        public AlertResults WinDinkEditLaunchAsAdmin { get; private set; } = AlertResults.None;
        public AlertResults WinDinkEditCreateSymbolicLink { get; private set; } = AlertResults.None;


        public ConfigAlertResults() {

        }
        
        private void FireUpdatedEvent() {
            this.Updated?.Invoke(this, EventArgs.Empty);
        }

        private static bool ListsAreDifferent(List<string> list1, List<string> list2) {
            if (list1.Count != list2.Count) {
                return true;
            }
            
            for (int i = 0; i < list1.Count; i++) {
                if (list1[i] != list2[i]) {
                    return true;
                }
            }

            return false;
        }

        public AlertResults GetResult(ConfigAlertResultMap id) {
            return id switch {
                ConfigAlertResultMap.WinDinkEditLaunchAsAdmin => this.WinDinkEditLaunchAsAdmin,
                ConfigAlertResultMap.WinDinkEditCreateSymbolicLink => this.WinDinkEditCreateSymbolicLink,
                _ => AlertResults.None,
            };
        }

        public void SaveResult(ConfigAlertResultMap id, AlertResults result) {
            bool hasChanges = false;

            switch (id) {
                case ConfigAlertResultMap.WinDinkEditLaunchAsAdmin:
                    if (this.WinDinkEditLaunchAsAdmin != result) {
                        this.WinDinkEditLaunchAsAdmin = result;
                        hasChanges = true;
                    }
                    break;
                case ConfigAlertResultMap.WinDinkEditCreateSymbolicLink:
                    if (this.WinDinkEditCreateSymbolicLink != result) {
                        this.WinDinkEditCreateSymbolicLink = result;
                        hasChanges = true;
                    }
                    break;
            }

            if (hasChanges) {
                this.FireUpdatedEvent();
            }
        }

        public void UpdateFromData(ConfigDataAlertResults data) {
            bool hasChanges = false;

            if (data.RememberWinDinkEditLaunchAsAdmin != null && 
                Enum.TryParse(data.RememberWinDinkEditLaunchAsAdmin, true, out AlertResults alertResult) && 
                this.WinDinkEditLaunchAsAdmin != alertResult ) {
                this.WinDinkEditLaunchAsAdmin = alertResult;
                hasChanges = true;
            }
            
            if (hasChanges) {
                this.FireUpdatedEvent();
            }
        }

        public ConfigDataAlertResults GetData() {
           
            ConfigDataAlertResults data = new ConfigDataAlertResults()  {
                RememberWinDinkEditLaunchAsAdmin = this.WinDinkEditLaunchAsAdmin.ToString(),
            };

            return data;
        }
    }
}
