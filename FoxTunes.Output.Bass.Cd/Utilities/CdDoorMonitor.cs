using FoxTunes.Interfaces;
using ManagedBass.Cd;
using System;
using System.Timers;

namespace FoxTunes
{
    public class CdDoorMonitor : StandardComponent, IDisposable
    {
        const int UPDATE_INTERVAL = 1000;

        public IPlaybackManager PlaybackManager { get; private set; }

        public Timer Timer { get; private set; }

        public bool Enabled
        {
            get
            {
                return this.Timer != null && this.Timer.Enabled;
            }
        }

        private CdDoorState _State { get; set; }

        public CdDoorState State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;
                this.OnStateChanged();
            }
        }

        protected virtual void OnStateChanged()
        {
            Logger.Write(this, LogLevel.Debug, "CD door state changed to {0}.", Enum.GetName(typeof(CdDoorState), this.State));
            if (this.StateChanged == null)
            {
                return;
            }
            this.StateChanged(this, EventArgs.Empty);
        }

        public event EventHandler StateChanged;

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            var drive = default(int);
            if (this.GetDrive(out drive))
            {
                this.Enable();
            }
            else
            {
                this.Disable();
            }
        }

        public void Enable()
        {
            if (this.Enabled)
            {
                return;
            }
            this.Timer = new Timer();
            this.Timer.Interval = UPDATE_INTERVAL;
            this.Timer.Elapsed += this.OnElapsed;
            this.Timer.Start();
            Logger.Write(this, LogLevel.Debug, "Started monitoring CD door state.");
            this.UpdateState(false);
        }

        public void Disable()
        {
            if (!this.Enabled)
            {
                return;
            }
            this.Timer.Stop();
            this.Timer.Elapsed -= this.OnElapsed;
            this.Timer.Dispose();
            this.Timer = null;
            Logger.Write(this, LogLevel.Debug, "Stopped monitoring CD door state.");
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            this.UpdateState(true);
        }

        protected virtual bool GetDrive(out int drive)
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
                drive = default(int);
                return false;
            }
            var id = default(string);
            var track = default(int);
            return BassCdStreamProvider.ParseUrl(outputStream.FileName, out drive, out id, out track);
        }

        protected virtual void UpdateState(bool notify)
        {
            var drive = default(int);
            if (!this.GetDrive(out drive))
            {
                return;
            }
            if (BassCd.DoorIsOpen(drive) || !BassCd.IsReady(drive))
            {
                this.UpdateState(notify, CdDoorState.Open);
            }
            else
            {
                this.UpdateState(notify, CdDoorState.Closed);
            }
        }

        protected virtual void UpdateState(bool notify, CdDoorState state)
        {
            if (this.State == state)
            {
                return;
            }
            if (notify)
            {
                this.State = state;
            }
            else
            {
                this._State = state;
            }
        }

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
            this.Disable();
        }

        ~CdDoorMonitor()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
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

    public enum CdDoorState : byte
    {
        None,
        Open,
        Closed
    }
}
