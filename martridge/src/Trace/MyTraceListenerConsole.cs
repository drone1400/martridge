using Martridge.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Martridge.Trace {
    public class MyTraceListenerConsole : MyTraceListener, INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void FirePropertyChanged([CallerMemberName] string? name = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MyTraceListenerConsole(string name) : base(name){ }

        public override void WriteMessage(DateTime timestamp, string category, string message, MyTraceLevel level) {
            if (this.ShowLevels) {
                Console.WriteLine(MyTrace.FormatLine(timestamp, category, message, level));
            } else {
                Console.WriteLine(MyTrace.FormatLine(timestamp, category, message));
            }
        }

        public override void WriteMessage(DateTime timestamp, string category, List<string> messages, MyTraceLevel level) {
            foreach (string message in messages) {
                if (this.ShowLevels) {
                    Console.WriteLine(MyTrace.FormatLine(timestamp, category, message, level));
                } else {
                    Console.WriteLine(MyTrace.FormatLine(timestamp, category, message));
                }
            }
        }
        public override void Flush() {
            // nothing to do
        }
    }
}
