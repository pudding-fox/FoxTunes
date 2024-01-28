using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Timers;

namespace FoxTunes
{
    public class Debouncer : IDisposable
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly object SyncRoot = new object();

        private Debouncer()
        {
            this.Actions = new HashSet<Action>();
        }

        public Debouncer(int timeout) : this()
        {
            this.Timer = new global::System.Timers.Timer(timeout);
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
        }

        public Debouncer(TimeSpan timeout) : this(Convert.ToInt32(timeout.TotalMilliseconds))
        {

        }

        public void Exec(Action action)
        {
            lock (SyncRoot)
            {
                this.Actions.Add(action);
                this.Timer.Stop();
                this.Timer.Start();
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (SyncRoot)
            {
                foreach (var action in this.Actions)
                {
                    action();
                }
                this.Actions.Clear();
            }
        }

        public HashSet<Action> Actions { get; private set; }

        public global::System.Timers.Timer Timer { get; private set; }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            lock (SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                }
            }
        }

        ~Debouncer()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
