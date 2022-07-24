using System;
using System.Collections.Generic;

namespace Martridge.Trace {
    public abstract class MyTraceListener {

        public string Name { get; private set; }

        public MyTraceLevel Levels { get; set; } = MyTraceLevel.Critical | MyTraceLevel.Error | MyTraceLevel.Warning | MyTraceLevel.Information | MyTraceLevel.Verbose;

        public bool ShowLevels { get; set; } = true;

        public bool IsClosed {
            get { lock (this._closedLock) { return this._isClosed; } }
            protected set { lock (this._closedLock) { this._isClosed = value; } }
        }
        private bool _isClosed = false;
        private object _closedLock = new object();


        public MyTraceListener(string  name) {
            this.Name = name;
        }

        public abstract void WriteMessage(DateTime timestamp, string category, string message, MyTraceLevel level);

        public abstract void WriteMessage(DateTime timestamp, string category, List<string> messages, MyTraceLevel level);

        public virtual void Close() {
            this.IsClosed = true;
        }
        public abstract void Flush();

    }
}
