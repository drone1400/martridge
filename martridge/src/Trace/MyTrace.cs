using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Martridge.Trace {

    public class MyTrace {
        public static MyTrace Global = new MyTrace("Global");

        public static readonly string TimestampFormat = "HH:mm:ss.ffffff: ";
        public static readonly string FileTimestamp = "yyyy-MM-dd_HH-mm-ss";

        public List<MyTraceListener> Listeners { get; } = new List<MyTraceListener>();

        public string Name { get; private set; }

        public bool MirrorToGlobalTrace { get; set; } = false;

        public MyTrace(string name) {
            this.Name = name;

        }

        public void Close() {
            foreach (MyTraceListener listener in this.Listeners) {
                listener.Close();
            }

            this.Listeners.Clear();
        }

        public void Flush() {
            foreach (MyTraceListener listener in this.Listeners) {
                listener.Flush();
            }
        }

        public void WriteMessage(string message, MyTraceLevel level = MyTraceLevel.Information, [CallerFilePath] string? callerFilePath = null) {
            DateTime now = DateTime.Now;
            string category = callerFilePath == null
                ? "generic"
                : Path.GetFileNameWithoutExtension(callerFilePath);
            foreach (MyTraceListener listener in this.Listeners) {
                if (!listener.IsClosed && listener.Levels.HasFlag(level)) {
                    listener.WriteMessage(now, category, message, level);
                }
            }

            if (ReferenceEquals(this, MyTrace.Global) == false && this.MirrorToGlobalTrace) {
                MyTrace.Global.WriteMessage(category, message, level);
            }
        }

        public void WriteMessage(string category, string message, MyTraceLevel level = MyTraceLevel.Information) {
            DateTime now = DateTime.Now;
            foreach (MyTraceListener listener in this.Listeners) {
                if (!listener.IsClosed && listener.Levels.HasFlag(level)) {
                    listener.WriteMessage(now, category, message, level);
                }
            }
            
            if (ReferenceEquals(this, MyTrace.Global) == false && this.MirrorToGlobalTrace) {
                MyTrace.Global.WriteMessage(category, message, level);
            }
        }

        public void WriteMessage(List<string> messages, MyTraceLevel level = MyTraceLevel.Information, [CallerFilePath] string? callerFilePath = null) {
            DateTime now = DateTime.Now;
            string category = callerFilePath == null
                ? "generic"
                : Path.GetFileNameWithoutExtension(callerFilePath);
            foreach (MyTraceListener listener in this.Listeners) {
                if (!listener.IsClosed && listener.Levels.HasFlag(level)) {
                    listener.WriteMessage(now, category, messages, level);
                }
            }
            
            if (ReferenceEquals(this, MyTrace.Global) == false && this.MirrorToGlobalTrace) {
                MyTrace.Global.WriteMessage(category, messages, level);
            }
        }

        public void WriteMessage(string category, List<string> messages, MyTraceLevel level = MyTraceLevel.Information) {
            DateTime now = DateTime.Now;
            foreach (MyTraceListener listener in this.Listeners) {
                if (!listener.IsClosed && listener.Levels.HasFlag(level)) {
                    listener.WriteMessage(now, category, messages, level);
                }
            }
            
            if (ReferenceEquals(this, MyTrace.Global) == false && this.MirrorToGlobalTrace) {
                MyTrace.Global.WriteMessage(category, messages, level);
            }
        }

        public void WriteException(Exception ex, MyTraceLevel level = MyTraceLevel.Error, [CallerFilePath] string? callerFilePath = null) {
            string category = callerFilePath == null
                ? "generic"
                : Path.GetFileNameWithoutExtension(callerFilePath);
            this.WriteMessage(category,GetExceptionStringAsList(ex), level);
        }

        public void WriteException(string category, Exception ex, MyTraceLevel level = MyTraceLevel.Error) {
            this.WriteMessage(category, GetExceptionStringAsList(ex), level);
        }

        public static string GetExceptionMessages(Exception? ex) {
            StringBuilder str = new StringBuilder();

            while (ex != null) {
                str.AppendLine(ex.ToString());
                ex = ex.InnerException;
            }

            return str.ToString();
        }

        public static List<string> GetExceptionStringAsList(Exception? ex) {
            List<string> messages = new List<string>();

            while (ex != null) {
                messages.Add(ex.ToString());
                ex = ex.InnerException;
            }

            return messages;
        }

        public static string FormatLine(DateTime timestamp, string category, string message) {
            return $"{timestamp.ToString(TimestampFormat)} [{category}] {message}";
        }

        public static string FormatLine(DateTime timestamp, string category, string message, MyTraceLevel level) {
            return $"{timestamp.ToString(TimestampFormat)} [{category}] [{level}] {message}";
        }
    }
}
