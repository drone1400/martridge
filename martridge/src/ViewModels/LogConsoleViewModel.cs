using Martridge.Trace;
using ReactiveUI;
using System.ComponentModel;
using Martridge.Models.Localization;

namespace Martridge.ViewModels {
    public class LogConsoleViewModel : ViewModelBase {

#if DEBUG
        public string Title => "Martridge (Debug Build) - " + Localizer.Instance["LogWindow/Title"];
#else
        public string Title => "Martridge - " + Localizer.Instance["LogWindow/Title"];
#endif
        
        public string Text { get => this._traceListener.Text; }
        private MyTraceListenerGui _traceListener = new MyTraceListenerGui("LogConsoleViewTraceListener");

        public LogConsoleViewModel() {
            MyTrace.Global.Listeners.Add(this._traceListener);
            this._traceListener.PropertyChanged += this._traceListener_PropertyChanged;
            MyTrace.Global.WriteMessage("<Initialized Log Console...>");
        }

        public void CloseTraceListener() {
            this._traceListener.Close();
        }

        private void _traceListener_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            this.RaisePropertyChanged(nameof(this.Text));
        }
    }
}
