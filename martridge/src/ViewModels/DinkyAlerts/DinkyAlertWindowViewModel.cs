using Avalonia.Controls;
using Martridge.ViewModels.DinkyGraphics;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;

namespace Martridge.ViewModels.DinkyAlerts {
    public class DinkyAlertWindowViewModel :ViewModelBase {

        private string _title = "???";
        public string Title {
            get => this._title;
            set => this.RaiseAndSetIfChanged(ref this._title, value);
        }
        
        private string _message = "Hello World!";
        public string Message {
            get => this._message;
            set => this.RaiseAndSetIfChanged(ref this._message, value);
        }

        private AlertResults _result = AlertResults.None;
        public AlertResults Result {
            get => this._result;
            set => this.RaiseAndSetIfChanged(ref this._result, value);
        }

        private AlertResults _resultButtons = AlertResults.None;

        public bool ShowButtonOk {
            get => this._resultButtons.HasFlag(AlertResults.Ok);
        }
        
        public bool ShowButtonNo {
            get => this._resultButtons.HasFlag(AlertResults.No);
        }
        
        public bool ShowButtonYes {
            get => this._resultButtons.HasFlag(AlertResults.Yes);
        }

        public bool ShowButtonCancel {
            get => this._resultButtons.HasFlag(AlertResults.Cancel);
        }
        
        public bool ShowButtonAbort {
            get => this._resultButtons.HasFlag(AlertResults.Abort);
        }

        private AlertType _type = AlertType.Info;
        public AnimatedDinkGraphicViewModel? AnimatedImage {
            get => this._type switch {
                AlertType.Info => GetGraphicInfo(),
                AlertType.Warning => GetGraphicWarning(),
                AlertType.Error => GetGraphicError(),
                AlertType.Secret => GetGraphicSecret(),
                _ => null,
            };
        }

        private AnimatedDinkGraphicViewModel GetGraphicInfo() {
            return DinkyAlert.AnimatedDuckWizardRight;
        }
        
        private AnimatedDinkGraphicViewModel GetGraphicWarning() {
            return DinkyAlert.AnimatedDinkSword;
        }
        
        private AnimatedDinkGraphicViewModel GetGraphicError() {
            return DinkyAlert.AnimatedPillbug;
        }
        
        private AnimatedDinkGraphicViewModel GetGraphicSecret() {
            return DinkyAlert.AnimatedPillbug;
        }

        
        public DinkyAlertWindowViewModel(string title, string message, AlertResults resultButtons, AlertType type) {
            this._title = title;
            this._message = message;
            this._resultButtons = resultButtons;
            this._type = type;
        }

        public void CmdExitOk (object? parameter) {
            if (parameter is Window win) {
                this._result = AlertResults.Ok;
                win.Close();
            }
        }
        public bool CanCmdExitOk (object? parameter) {
            if (parameter is Window) return true;
            return false;
        }
        
        public void CmdExitCancel (object? parameter) {
            if (parameter is Window win) {
                this._result = AlertResults.Cancel;
                win.Close();
            }
        }
        public bool CanCmdExitCancel (object? parameter) {
            if (parameter is Window) return true;
            return false;
        }
        
        public void CmdExitYes (object? parameter) {
            if (parameter is Window win) {
                this._result = AlertResults.Yes;
                win.Close();
            }
        }
        public bool CanCmdExitYes (object? parameter) {
            if (parameter is Window) return true;
            return false;
        }
        
        public void CmdExitNo (object? parameter) {
            if (parameter is Window win) {
                this._result = AlertResults.No;
                win.Close();
            }
        }
        public bool CanCmdExitNo (object? parameter) {
            if (parameter is Window) return true;
            return false;
        }
        
        public void CmdExitAbort (object? parameter) {
            if (parameter is Window win) {
                this._result = AlertResults.Abort;
                win.Close();
            }
        }
        public bool CanCmdExitAbort (object? parameter) {
            if (parameter is Window) return true;
            return false;
        }
    }
}
