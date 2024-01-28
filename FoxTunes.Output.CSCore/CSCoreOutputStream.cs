using CSCore;
using CSCore.SoundOut;
using CSCore.Streams;
using FoxTunes.Interfaces;
using System.Threading.Tasks;
using System;

namespace FoxTunes
{
    public class CSCoreOutputStream : OutputStream
    {
        const int UPDATE_INTERVAL = 100;

        public CSCoreOutputStream(PlaylistItem playlistItem, IWaveSource waveSource, ISoundOut soundOut)
            : base(playlistItem)
        {
            this.WaveSource = waveSource;
            this.SoundOut = soundOut;
            this.SampleSource = this.WaveSource.ToSampleSource();
            this.NotificationSource = new NotificationSource(this.SampleSource);
            this.NotificationSource.Interval = UPDATE_INTERVAL;
            this.NotificationSource.BlockRead += this.NotificationSource_BlockRead;
            this.SoundOut.Stopped += this.SoundOut_Stopped;
        }

        public IWaveSource WaveSource { get; private set; }

        public ISampleSource SampleSource { get; private set; }

        public NotificationSource NotificationSource { get; private set; }

        public ISoundOut SoundOut { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public WaveFormat WaveFormat
        {
            get
            {
                return this.NotificationSource.WaveFormat;
            }
        }

        public override long Position
        {
            get
            {
                try
                {
                    return this.WaveSource.Position;
                }
                catch (NullReferenceException)
                {
                    //Disposed.
                    return 0;
                }
            }
            set
            {
                this.WaveSource.Position = value;
            }
        }

        public override long Length
        {
            get
            {
                try
                {
                    return this.WaveSource.Length;
                }
                catch (NullReferenceException)
                {
                    //Disposed.
                    return 0;
                }
            }
        }

        public override int BytesPerSample
        {
            get
            {
                return this.WaveFormat.BytesPerSample;
            }
        }

        public override int ExtraSize
        {
            get
            {
                return this.WaveFormat.ExtraSize;
            }
        }

        public override int BitsPerSample
        {
            get
            {
                return this.WaveFormat.BitsPerSample;
            }
        }

        public override int BlockAlign
        {
            get
            {
                return this.WaveFormat.BlockAlign;
            }
        }

        public override int BytesPerSecond
        {
            get
            {
                return this.WaveFormat.BytesPerSecond;
            }
        }

        public override int SampleRate
        {
            get
            {
                return this.WaveFormat.SampleRate;
            }
        }

        public override string Format
        {
            get
            {
                return Enum.GetName(typeof(AudioEncoding), this.WaveFormat.WaveFormatTag);
            }
        }

        public override int BytesPerBlock
        {
            get
            {
                return this.WaveFormat.BytesPerBlock;
            }
        }

        public override int Channels
        {
            get
            {
                return this.WaveFormat.Channels;
            }
        }

        public override bool IsPlaying
        {
            get
            {
                return this.SoundOut.PlaybackState == PlaybackState.Playing;
            }
        }

        public override bool IsPaused
        {
            get
            {
                return this.SoundOut.PlaybackState == PlaybackState.Paused;
            }
        }

        public override bool IsStopped
        {
            get
            {
                return this.SoundOut.PlaybackState == PlaybackState.Stopped;
            }
        }

        public bool StopRequested { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            base.InitializeComponent(core);
        }

        protected virtual void NotificationSource_BlockRead(object sender, BlockReadEventArgs<float> e)
        {
            this.ForegroundTaskRunner.RunAsync(() => this.OnPositionChanged());
        }

        protected virtual void SoundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            var manual = this.StopRequested;
            this.StopRequested = false;
            this.EmitState();
            this.OnStopped(manual);
        }

        public override void Play()
        {
            Logger.Write(this, LogLevel.Debug, "Initializing sound out.");
            this.SoundOut.Initialize(this.NotificationSource.ToWaveSource());
            Logger.Write(this, LogLevel.Debug, "Playing sound out.");
            this.SoundOut.Play();
            this.EmitState();
            this.OnPlayed(true);
        }

        public override void Pause()
        {
            Logger.Write(this, LogLevel.Debug, "Pausing sound out.");
            this.SoundOut.Pause();
            this.EmitState();
            this.OnPaused();
        }

        public override void Resume()
        {
            Logger.Write(this, LogLevel.Debug, "Resuming sound out.");
            this.SoundOut.Resume();
            this.EmitState();
            this.OnResumed();
        }

        public override Task Stop()
        {
            Logger.Write(this, LogLevel.Debug, "Stopping sound out.");
            this.StopRequested = true;
            this.SoundOut.Stop();
            this.EmitState();
            return Task.CompletedTask;
        }

        private void EmitState()
        {
            this.OnIsPlayingChanged();
            this.OnIsPausedChanged();
            this.OnIsStoppedChanged();
        }

        public override void Dispose()
        {
            this.SoundOut.Dispose();
            this.WaveSource.Dispose();
        }
    }
}
