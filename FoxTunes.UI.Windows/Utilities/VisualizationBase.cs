using FoxTunes.Interfaces;
using System;
using System.Timers;
using System.Windows;

namespace FoxTunes
{
    public abstract class VisualizationBase : RendererBase
    {
        public readonly object SyncRoot = new object();

        public static readonly DependencyProperty ConfigurationProperty = ConfigurableUIComponentBase.ConfigurationProperty.AddOwner(
            typeof(VisualizationBase),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnConfigurationChanged)
        );

        private static void OnConfigurationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var visualizationBase = sender as VisualizationBase;
            if (visualizationBase == null)
            {
                return;
            }
            visualizationBase.OnConfigurationChanged();
        }

        public static IConfiguration GetConfiguration(VisualizationBase source)
        {
            return (IConfiguration)source.GetValue(ConfigurationProperty);
        }

        public static void SetConfiguration(VisualizationBase source, IConfiguration value)
        {
            source.SetValue(ConfigurationProperty, value);
        }

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

        private IConfiguration _Configuration { get; set; }

        protected IConfiguration GetConfiguration()
        {
            return this._Configuration;
        }

        public IConfiguration Configuration
        {
            get
            {
                return GetConfiguration(this);
            }
            set
            {
                SetConfiguration(this, value);
            }
        }

        protected virtual void OnConfigurationChanged()
        {
            this._Configuration = this.Configuration;
            if (this.Configuration != null)
            {
                this.Configuration.GetElement<IntegerConfigurationElement>(
                   VisualizationBehaviourConfiguration.SECTION,
                   VisualizationBehaviourConfiguration.INTERVAL_ELEMENT
                ).ConnectValue(value => this.UpdateInterval = value);
            }
            if (this.ConfigurationChanged != null)
            {
                this.ConfigurationChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ConfigurationChanged;

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
