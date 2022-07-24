using Martridge.Trace;
using ReactiveUI;
using System.ComponentModel;

namespace Martridge.ViewModels {
    public class LogConsoleViewModel : ViewModelBase {
        public string Text { get => this._traceListener.Text; }
        private MyTraceListenerGui _traceListener = new MyTraceListenerGui("LogConsoleViewTraceListener");

        public LogConsoleViewModel() {
            MyTrace.Global.Listeners.Add(this._traceListener);
            this._traceListener.PropertyChanged += this._traceListener_PropertyChanged;
            MyTrace.Global.WriteMessage(MyTraceCategory.General, "<Initialized Log Console...>");
        }

        public void CloseTraceListener() {
            this._traceListener.Close();
        }

        private void _traceListener_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            this.RaisePropertyChanged(nameof(this.Text));
        }
    }
}
