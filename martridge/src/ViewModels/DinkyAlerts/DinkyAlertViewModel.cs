using System;
using Martridge.Models.Localization;
using Martridge.ViewModels.DinkyGraphics;
using ReactiveUI;
using System.Collections.Generic;
using Martridge.Trace;

namespace Martridge.ViewModels.DinkyAlerts;


public class AlertResultDoneEventArgs : EventArgs {
    public AlertResults Result { get; }
    public AlertResultDoneEventArgs(AlertResults result) {
        this.Result = result;
    }
}

public class DinkyAlertViewModel : ViewModelBase {

    public event EventHandler<AlertResultDoneEventArgs>? ResultIsDone;

    private void FireResultIsDone() {
        try {
            this.ResultIsDone?.Invoke(this, new AlertResultDoneEventArgs(this._result));
        } catch (Exception ex) {
            MyTrace.Global.WriteException(MyTraceCategory.General, ex);
        }
    }

    public string Title => this._title;
    public string Message => this._message;
    public bool ShowRememberResultCheckbox => this._resultButtons.HasFlag(AlertResults.RememberResult);
    public bool ShowButtonOk => this._resultButtons.HasFlag(AlertResults.Ok);
    public bool ShowButtonNo => this._resultButtons.HasFlag(AlertResults.No);
    public bool ShowButtonYes => this._resultButtons.HasFlag(AlertResults.Yes);
    public bool ShowButtonCancel => this._resultButtons.HasFlag(AlertResults.Cancel);
    public string ButtonTextYes => this._buttonTextYes;
    public string ButtonTextNo => this._buttonTextNo;
    public string ButtonTextOk => this._buttonTextOk;
    public string ButtonTextCancel => this._buttonTextCancel;


    public AlertResults Result {
        get => this._result;
        private set => this.RaiseAndSetIfChanged(ref this._result, value);
    }
    public bool IsResultRemembered {
        get => this._isResultRemembered;
        set => this.RaiseAndSetIfChanged(ref this._isResultRemembered, value);
    }


    public AnimatedDinkGraphicViewModel? AnimatedImage {
        get => this._type switch {
            AlertType.Info => DinkyAlert.AnimatedDuckWizardRight,
            AlertType.Warning => DinkyAlert.AnimatedDinkSword,
            AlertType.Error => DinkyAlert.AnimatedPillbug,
            AlertType.Secret => DinkyAlert.AnimatedSpinningDuck,
            _ => null,
        };
    }

    private readonly string _title;
    private readonly string _message;
    private readonly AlertResults _resultButtons;
    private readonly AlertType _type;
    private readonly string _buttonTextYes;
    private readonly string _buttonTextNo;
    private readonly string _buttonTextOk;
    private readonly string _buttonTextCancel;
    private AlertResults _result;
    private bool _isResultRemembered = false;


    public DinkyAlertViewModel() {
        this._title = "placeholder";
        this._message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        this._resultButtons = AlertResults.No | AlertResults.Yes;
        this._type = AlertType.Info;
        
        this._buttonTextOk = Localizer.Instance[@"DinkyAlertWindow/ButtonOk"];
        this._buttonTextYes = Localizer.Instance[@"DinkyAlertWindow/ButtonYes"];
        this._buttonTextNo = Localizer.Instance[@"DinkyAlertWindow/ButtonNo"];
        this._buttonTextCancel = Localizer.Instance[@"DinkyAlertWindow/ButtonCancel"];
    }

    public DinkyAlertViewModel(string title, string message, AlertResults resultButtons, AlertType type, Dictionary<AlertResults, string>? customButtonText = null) {
        this._title = title;
        this._message = message;
        this._resultButtons = resultButtons;
        this._type = type;

        this._buttonTextOk = Localizer.Instance[@"DinkyAlertWindow/ButtonOk"];
        this._buttonTextYes = Localizer.Instance[@"DinkyAlertWindow/ButtonYes"];
        this._buttonTextNo = Localizer.Instance[@"DinkyAlertWindow/ButtonNo"];
        this._buttonTextCancel = Localizer.Instance[@"DinkyAlertWindow/ButtonCancel"];

        if (customButtonText != null) {
            foreach (var kvp in customButtonText) {
                switch (kvp.Key) {
                    case AlertResults.Ok:
                        this._buttonTextOk = kvp.Value;
                        break;
                    case AlertResults.Yes:
                        this._buttonTextYes = kvp.Value;
                        break;
                    case AlertResults.No:
                        this._buttonTextNo = kvp.Value;
                        break;
                    case AlertResults.Cancel:
                        this._buttonTextCancel = kvp.Value;
                        break;
                }
            }
        }
    }

    public void CmdExitOk (object? parameter) {
        if (this.CanCmdExitOk(parameter)) {
            this.Result = AlertResults.Ok;
            this.FireResultIsDone();
        }
    }
    public bool CanCmdExitOk (object? parameter) => this._resultButtons.HasFlag(AlertResults.Ok);

    public void CmdExitCancel (object? parameter) {
        if (this.CanCmdExitCancel(parameter)) {
            this.Result = AlertResults.Cancel;
            this.FireResultIsDone();
        }
    }
    public bool CanCmdExitCancel (object? parameter) => this._resultButtons.HasFlag(AlertResults.Cancel);

    public void CmdExitYes (object? parameter) {
        if (this.CanCmdExitYes(parameter)) {
            this.Result = AlertResults.Yes;
            this.FireResultIsDone();
        }
    }
    public bool CanCmdExitYes (object? parameter) => this._resultButtons.HasFlag(AlertResults.Yes);

    public void CmdExitNo (object? parameter) {
        if (this.CanCmdExitNo(parameter)) {
            this.Result = AlertResults.No;
            this.FireResultIsDone();
        }
    }
    public bool CanCmdExitNo (object? parameter) => this._resultButtons.HasFlag(AlertResults.No);
}
