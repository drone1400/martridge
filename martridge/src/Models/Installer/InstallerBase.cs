using Martridge.Trace;
using System;
using System.Threading;

namespace Martridge.Models.Installer {
    public abstract class InstallerBase {
        //
        // State related Properties
        //

        private object _lockStateAccess = new object();

        public bool IsBusy {
            get {
                lock (this._lockStateAccess) {
                    return this._isBusy;
                }
            }
            protected set {
                lock (this._lockStateAccess) {
                    this._isBusy = value;
                }
            }
        }
        private bool _isBusy = false;

        public bool IsDone {
            get {
                lock (this._lockStateAccess) {
                    return this._isDone;
                }
            }
            protected set {
                lock (this._lockStateAccess) {
                    this._isDone = value;
                }
            }
        }
        private bool _isDone = false;

        public DateTime StartTime {
            get {
                lock (this._lockStateAccess) {
                    return this._startTime;
                }
            }
            protected set {
                lock (this._lockStateAccess) {
                    this._startTime = value;
                }
            }
        }
        private DateTime _startTime = DateTime.MinValue;

        public DateTime EndTime {
            get {
                lock (this._lockStateAccess) {
                    return this._endTime;
                }
            }
            protected set {
                lock (this._lockStateAccess) {
                    this._endTime = value;
                }
            }
        }
        private DateTime _endTime = DateTime.MaxValue;

        //
        // Internal stuff
        //

        public CancellationTokenSource CancelTokenSource { get; } = new CancellationTokenSource();

        protected CancellationToken CancelToken => CancelTokenSource.Token;

        public MyTrace CustomTrace { get; }
        
        protected double ProgPhaseCurrent = 0;
        protected double ProgPhaseTotal = 0;

        //
        // Constructor
        //

        public InstallerBase() {
            this.CustomTrace = new MyTrace(this.GetType().ToString());

            // add text logger listener to trace...
            MyTraceListenerLogger traceLogger = new MyTraceListenerLogger(this.GetType().ToString());
            this.CustomTrace.Listeners.Add(traceLogger);
        }
        
        
        //
        // Progress event
        //

        public event EventHandler<InstallerProgressEventArgs>? ProgressReport;
        protected void ReportProgress(InstallerReportLevel level, string heading, string detail, double percent) {
            this.ProgressReport?.Invoke(this, new InstallerProgressEventArgs(level, heading, detail, percent));
        }

    }
}
