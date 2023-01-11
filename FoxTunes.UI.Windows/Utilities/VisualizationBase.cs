using FoxTunes.Interfaces;
using System;
using System.Timers;
using System.Windows.Controls;

namespace FoxTunes
{
    public abstract class VisualizationBase : RendererBase
    {
        public readonly object SyncRoot = new object();

        public bool Enabled { get; private set; }

        private int _UpdateInterval { get; set; }

        public int UpdateInterval
        {
            get
            {
                return this._UpdateInterval;
            }
            protected set
            {
                this._UpdateInterval = value;
                this.OnUpdateIntervalChanged();
            }
        }

        protected virtual void OnUpdateIntervalChanged()
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Interval = this.UpdateInterval;
                }
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

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Configuration.GetElement<IntegerConfigurationElement>(
                   VisualizationBehaviourConfiguration.SECTION,
                   VisualizationBehaviourConfiguration.INTERVAL_ELEMENT
                ).ConnectValue(value => this.UpdateInterval = value);
            }
            base.OnConfigurationChanged();
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
