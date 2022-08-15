using Avalonia.Threading;
using Martridge.Models.Localization;
using Martridge.Trace;
using Martridge.ViewModels.DinkyAlerts;
using ReactiveUI;
using System;

namespace Martridge.ViewModels.Installer {
    public abstract class InstallerViewModelBase : ViewModelBase {

        // ------------------------------------------------------------------------------------------
        //      Progress reporting 
        //

        public string InstallerProgressTitle {
            get => this._installerProgressTitle;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressTitle, value);
        }
        private string _installerProgressTitle = "";

        public bool InstallerProgressLevel0IsVisibile {
            get => this._installerProgressLevel0IsVisibile;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel0IsVisibile, value);
        }
        private bool _installerProgressLevel0IsVisibile = true;

        public string InstallerProgressLevel0MainTitle {
            get => this._installerProgressLevel0MainTitle;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel0MainTitle, value);
        }
        private string _installerProgressLevel0MainTitle = "";
        public string InstallerProgressLevel0SubTitle {
            get => this._installerProgressLevel0SubTitle;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel0SubTitle, value);
        }
        private string _installerProgressLevel0SubTitle = "";

        public double InstallerProgressLevel0Progress {
            get => this._installerProgressLevel0Progress;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel0Progress, value);
        }
        private double _installerProgressLevel0Progress = 0.0;

        public bool InstallerProgressLevel1IsVisibile {
            get => this._installerProgressLevel1IsVisibile;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel1IsVisibile, value);
        }
        private bool _installerProgressLevel1IsVisibile = false;

        public string InstallerProgressLevel1MainTitle {
            get => this._installerProgressLevel1MainTitle;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel1MainTitle, value);
        }
        private string _installerProgressLevel1MainTitle = "";
        public string InstallerProgressLevel1SubTitle {
            get => this._installerProgressLevel1SubTitle;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel1SubTitle, value);
        }
        private string _installerProgressLevel1SubTitle = "";

        public double InstallerProgressLevel1Progress {
            get => this._installerProgressLevel1Progress;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel1Progress, value);
        }
        private double _installerProgressLevel1Progress = 0.0;

        public bool InstallerProgressLevel1Indeterminate {
            get => this._installerProgressLevel1Indeterminate;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLevel1Indeterminate, value);
        }
        private bool _installerProgressLevel1Indeterminate = false;


        public string InstallerProgressLog {
            get => this.InstallerTraceListener?.Text ?? "";
        }
        protected MyTraceListenerGui? InstallerTraceListener = null;

        public int InstallerProgressLogCaretIndex {
            get => this._installerProgressLogCaretIndex;
            protected set => this.RaiseAndSetIfChanged(ref this._installerProgressLogCaretIndex, value);
        }
        private int _installerProgressLogCaretIndex = 0;

        //
        //
        //

        protected void ShowInstallerCancelledMessageBox() {
            Dispatcher.UIThread.InvokeAsync(() => {
                try {
                    string title = Localizer.Instance[@"DinkInstallerView/MessageBox_Cancel_Title"];
                    string body = Localizer.Instance[@"DinkInstallerView/MessageBox_Cancel_Body"];
                    DinkyAlert.ShowDialog(title, body, AlertResults.Ok, AlertType.Info, this.ParentWindow);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }

        protected void ShowInstallerErrorMessageBox(Exception? exception) {
            Dispatcher.UIThread.InvokeAsync(() => {
                try {
                    string title = Localizer.Instance[@"DinkInstallerView/MessageBox_Error_Title"];
                    string body = Localizer.Instance[@"DinkInstallerView/MessageBox_Error_Body"] + Environment.NewLine + MyTrace.GetExceptionMessages(exception);
                    DinkyAlert.ShowDialog(title, body, AlertResults.Ok, AlertType.Error, this.ParentWindow);
                } catch (Exception ex) {
                    MyTrace.Global.WriteException(MyTraceCategory.DinkInstaller, ex);
                }
            });
        }
    }
}
