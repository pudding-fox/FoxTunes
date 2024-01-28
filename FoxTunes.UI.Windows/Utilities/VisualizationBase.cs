using FoxTunes.Interfaces;
using System;
using System.Timers;

namespace FoxTunes
{
    public abstract class VisualizationBase : RendererBase
    {
        public readonly object SyncRoot = new object();

        public global::System.Timers.Timer Timer;

        public VisualizationBase()
        {
            this.Timer = new global::System.Timers.Timer();
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
        }

        public bool Enabled { get; private set; }

        public int UpdateInterval
        {
            get
            {
                return Convert.ToInt32(this.Timer.Interval);
            }
            protected set
            {
                this.Timer.Interval = value;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Output.CanGetDataChanged += this.OnCanGetDataChanged;
            this.OnCanGetDataChanged(this, EventArgs.Empty);
        }

        protected virtual void OnCanGetDataChanged(object sender, EventArgs e)
        {
            PlaybackStateNotifier.Notify -= this.OnNotify;
            if (this.Output.CanGetData)
            {
                PlaybackStateNotifier.Notify += this.OnNotify;
            }
            this.Update();
        }

        protected virtual void OnNotify(object sender, EventArgs e)
        {
            this.Update();
        }

        protected virtual void Update()
        {
            var enabled = default(bool);
            lock (this.SyncRoot)
            {
                enabled = this.Enabled;
            }
            if (PlaybackStateNotifier.IsPlaying && !enabled)
            {
                Logger.Write(this, LogLevel.Debug, "Playback was started, starting renderer.");
                this.Start();
            }
            else if (!PlaybackStateNotifier.IsPlaying && enabled)
            {
                Logger.Write(this, LogLevel.Debug, "Playback was stopped, stopping renderer.");
                this.Stop();
            }
        }

        public void Start()
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Start();
                    this.Enabled = true;
                }
            }
        }

        public void Stop()
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Enabled = false;
                }
            }
            if (!PlaybackStateNotifier.IsPlaying && !PlaybackStateNotifier.IsPaused)
            {
                var task = this.Clear();
            }
        }

        protected abstract void OnElapsed(object sender, ElapsedEventArgs e);

        protected override void OnDisposing()
        {
            if (this.Output != null)
            {
                this.Output.IsStartedChanged -= this.OnCanGetDataChanged;
            }
            PlaybackStateNotifier.Notify -= this.OnNotify;
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                }
            }
            base.OnDisposing();
        }
    }
}
