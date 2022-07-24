using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Martridge.Trace {
    public class MyTraceListenerGui : MyTraceListener, INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void FirePropertyChanged([CallerMemberName] string? name = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private StringBuilder _stringBuilder = new StringBuilder();
        public string Text { get => this._stringBuilder.ToString(); }

        private Thread _notifyThread;
        private object _notifyThreadLock = new object();
        private bool _notifyThreadStop = false;
        private bool _notifyThreadHasChanges = false;

        public MyTraceListenerGui(string name) : base(name) {
            this._notifyThread = new Thread(this.NotifyThreadLoop);
            this._notifyThread.Start();
        }

        private void NotifyThreadLoop() {
            while (true) {

                bool fireEvent = false;
                lock (this._notifyThreadLock) {
                    if (this._notifyThreadStop) {
                        return;
                    }

                    if (this._notifyThreadHasChanges) {
                        this._notifyThreadHasChanges = false;
                        fireEvent = true;
                    }
                }

                if (fireEvent) {
                    this.FirePropertyChanged(nameof(this.Text));
                }

                Thread.Sleep(500);
            }
        }

        public override void Close() {
            base.Close();

            lock (this._notifyThreadLock) {
                this._notifyThreadStop = true;
            }

            this.FirePropertyChanged(nameof(this.Text));
        }
        public override void Flush() {
            lock (this._notifyThreadLock) {
                this._notifyThreadHasChanges = true;
            }
        }
        public override void WriteMessage(DateTime timestamp, string category, string message, MyTraceLevel level) {
            if (this.ShowLevels) {
                this._stringBuilder.AppendLine(MyTrace.FormatLine(timestamp, category, message, level));
            } else {
                this._stringBuilder.AppendLine(MyTrace.FormatLine(timestamp, category, message));
            }

            lock (this._notifyThreadLock) {
                this._notifyThreadHasChanges = true;
            }
        }

        public override void WriteMessage(DateTime timestamp, string category, List<string> messages, MyTraceLevel level) {
            foreach (string message in messages) {
                if (this.ShowLevels) {
                    this._stringBuilder.AppendLine(MyTrace.FormatLine(timestamp, category, message, level));
                } else {
                    this._stringBuilder.AppendLine(MyTrace.FormatLine(timestamp, category, message));
                }
            }
            lock (this._notifyThreadLock) {
                this._notifyThreadHasChanges = true;
            }
        }


    }
}
