using Avalonia.Controls;
using Martridge.Models.Localization;
using Martridge.ViewModels.DinkyGraphics;
using ReactiveUI;
using System.Collections.Generic;

namespace Martridge.ViewModels.DinkyAlerts {
    public class DinkyAlertWindowViewModel :ViewModelBase {

        public string Title {
            get => this._title;
            set => this.RaiseAndSetIfChanged(ref this._title, value);
        }
        private string _title;
        
        public string Message {
            get => this._message;
            set => this.RaiseAndSetIfChanged(ref this._message, value);
        }
        private string _message;

        public string? SpecialMessage {
            get => this._specialMessage;
            set {
                this.RaiseAndSetIfChanged(ref this._specialMessage, value);
                this.RaisePropertyChanged(nameof(this.ShowSpecialMessage));
            }
        }
        private string? _specialMessage = null;

        private bool ShowSpecialMessage {
            get => this._specialMessage != null;
        }

        
        public AlertResults Result {
            get => this._result;
            set => this.RaiseAndSetIfChanged(ref this._result, value);
        }
        private AlertResults _result;

        private AlertResults _resultButtons;
        
        public bool ShowRememberResultCheckbox {
            get => this._resultButtons.HasFlag(AlertResults.RememberResult);
        }

        public bool IsResultRemembered {
            get => this._isResultRemembered;
            set => this.RaiseAndSetIfChanged(ref this._isResultRemembered, value);
        }

        private bool _isResultRemembered = false;

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

        public string ButtonTextYes {
            get => this._buttonTextYes;
            private set => this.RaiseAndSetIfChanged(ref this._buttonTextYes, value);
        }
        private string _buttonTextYes;
        
        public string ButtonTextNo {
            get => this._buttonTextNo;
            private set => this.RaiseAndSetIfChanged(ref this._buttonTextNo, value);
        }
        private string _buttonTextNo;
        
        public string ButtonTextOk {
            get => this._buttonTextOk;
            private set => this.RaiseAndSetIfChanged(ref this._buttonTextOk, value);
        }
        private string _buttonTextOk;
        
        public string ButtonTextCancel {
            get => this._buttonTextCancel;
            private set => this.RaiseAndSetIfChanged(ref this._buttonTextCancel, value);
        }
        private string _buttonTextCancel;

        private AlertType _type;
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
        
        public DinkyAlertWindowViewModel(string title, string message, AlertResults resultButtons, AlertType type, Dictionary<AlertResults,string>? customButtonText = null, string? specialMessage = null) {
            this._title = title;
            this._message = message;
            this._resultButtons = resultButtons;
            this._type = type;

            this._specialMessage = specialMessage;

            this._buttonTextOk = Localizer.Instance[@"DinkyAlertWindow/ButtonOk"];
            this._buttonTextYes = Localizer.Instance[@"DinkyAlertWindow/ButtonYes"];
            this._buttonTextNo = Localizer.Instance[@"DinkyAlertWindow/ButtonNo"];
            this._buttonTextCancel = Localizer.Instance[@"DinkyAlertWindow/ButtonCancel"];

            SetCustomButtonText(customButtonText);
        }
        
        

        private void SetCustomButtonText(Dictionary<AlertResults, string>? customButtonText = null) {
            if (customButtonText != null) {
                foreach (var kvp in customButtonText) {
                    switch (kvp.Key) {
                        case AlertResults.Ok: this.ButtonTextOk = kvp.Value;
                            break;
                        case AlertResults.Yes: this.ButtonTextYes = kvp.Value;
                            break;
                        case AlertResults.No: this.ButtonTextNo = kvp.Value;
                            break;
                        case AlertResults.Cancel: this.ButtonTextCancel = kvp.Value;
                            break;
                    }
                }
            }
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
    }
}
