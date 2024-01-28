using FoxTunes.Interfaces;
using System;
using System.Timers;

namespace FoxTunes
{
    public abstract class VisualizationBase : RendererBase
    {
        public readonly object SyncRoot = new object();

        public bool Enabled { get; private set; }

        public int UpdateInterval
        {
            get
            {
                if (this.Timer == null)
                {
                    return 0;
                }
                return Convert.ToInt32(this.Timer.Interval);
            }
            protected set
            {
                if (this.Timer == null)
                {
                    return;
                }
                this.Timer.Interval = value;
            }
        }

        protected global::System.Timers.Timer Timer { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Timer = new global::System.Timers.Timer();
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
            PlaybackStateNotifier.IsStartedChanged += this.OnIsStartedChanged;
            PlaybackStateNotifier.IsPlayingChanged += this.OnIsPlayingChanged;
            base.InitializeComponent(core);
            this.Update();
        }

        protected virtual void OnIsStartedChanged(object sender, EventArgs e)
        {
            this.Update();
        }

        protected virtual void OnIsPlayingChanged(object sender, EventArgs e)
        {
            this.Update();
        }

        protected virtual void Update()
        {
            if (!PlaybackStateNotifier.IsStarted)
            {
                var task = this.Clear();
            }
            if (PlaybackStateNotifier.IsPlaying && !this.Enabled)
            {
                Logger.Write(this, LogLevel.Debug, "Playback was started, starting renderer.");
                this.Start();
            }
            else if (!PlaybackStateNotifier.IsPlaying && this.Enabled)
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

        protected virtual void Restart()
        {
            lock (this.SyncRoot)
            {
                if (!this.Enabled)
                {
                    return;
                }
                this.Start();
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
        }

        protected abstract void OnElapsed(object sender, ElapsedEventArgs e);

        protected override void OnDisposing()
        {
            PlaybackStateNotifier.IsStartedChanged -= this.OnIsStartedChanged;
            PlaybackStateNotifier.IsPlayingChanged -= this.OnIsPlayingChanged;
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
