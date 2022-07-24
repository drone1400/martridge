using Martridge.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Martridge.Trace {
    public class MyTraceListenerLogger : MyTraceListener, INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void FirePropertyChanged([CallerMemberName] string? name = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private FileStream _logStream;
        private StreamWriter _streamWriter;
        private FileInfo _logFileInfo;
        private Thread _autoLogFlusher;
        private object _objectLock = new object();

        public MyTraceListenerLogger(string name, bool autoFlush = true) : base(name){
            string fileName = name + "_" + DateTime.Now.ToString(MyTrace.FileTimestamp) + ".log";

            string path = Path.Combine(LocationHelper.LogsDirectory, fileName);
            this._logFileInfo = new FileInfo(path);
            this._logStream = new FileStream(this._logFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
            this._streamWriter = new StreamWriter(this._logStream);
            this._autoLogFlusher = new Thread(this.LogFlushLoop);
            if (autoFlush) {
                this._autoLogFlusher.Start();
            }
        }

        private void LogFlushLoop() {
            while (true) {
                if (this.IsClosed) return;
                this._streamWriter.Flush();
                this._logStream.Flush();

                Thread.Sleep(5000);
            }
        }

        public override void Close() {
            base.Close();

            this._streamWriter.Close();
            this._logStream.Close();
        }

        public override void Flush() {
            if (this.IsClosed) { return; }

            this._streamWriter.Flush();
            this._logStream.Flush();
        }

        public override void WriteMessage(DateTime timestamp, string category, string message, MyTraceLevel level) {
            if (this.ShowLevels) {
                this._streamWriter.WriteLine(MyTrace.FormatLine(timestamp, category, message, level));
            } else {
                this._streamWriter.WriteLine(MyTrace.FormatLine(timestamp, category, message));
            }
        }

        public override void WriteMessage(DateTime timestamp, string category, List<string> messages, MyTraceLevel level) {
            foreach (string message in messages) {
                if (this.ShowLevels) {
                    this._streamWriter.WriteLine(MyTrace.FormatLine(timestamp, category, message, level));
                } else {
                    this._streamWriter.WriteLine(MyTrace.FormatLine(timestamp, category, message));
                }
            }
        }

    }
}
