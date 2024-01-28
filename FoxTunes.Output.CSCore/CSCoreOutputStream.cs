using CSCore;
using CSCore.Codecs;
using CSCore.Ffmpeg;
using CSCore.SoundOut;
using CSCore.Streams;

namespace FoxTunes
{
    public class CSCoreOutputStream : OutputStream
    {
        const int UPDATE_INTERVAL = 100;

        public CSCoreOutputStream(string fileName)
        {
            this.FileName = fileName;
            this.InitializeComponent();
        }

        public string FileName { get; private set; }

        public IWaveSource WaveSource { get; private set; }

        public ISampleSource SampleSource { get; private set; }

        public NotificationSource NotificationSource { get; private set; }

        public ISoundOut SoundOut { get; private set; }

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
                return this.WaveSource.Position;
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
                return this.WaveSource.Length;
            }
        }

        public override int BlockAlign
        {
            get
            {
                return this.WaveFormat.BlockAlign;
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

        private void InitializeComponent()
        {
            this.WaveSource = new FfmpegDecoder(this.FileName); //CodecFactory.Instance.GetCodec(this.FileName);
            this.SampleSource = this.WaveSource.ToSampleSource();
            this.NotificationSource = new NotificationSource(this.SampleSource);
            this.NotificationSource.Interval = UPDATE_INTERVAL;
            this.NotificationSource.BlockRead += this.NotificationSource_BlockRead;
            this.SoundOut = new DirectSoundOut();
            this.SoundOut.Stopped += this.SoundOut_Stopped;
        }

        protected virtual void NotificationSource_BlockRead(object sender, BlockReadEventArgs<float> e)
        {
            this.OnPositionChanged();
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
            this.SoundOut.Initialize(this.NotificationSource.ToWaveSource());
            this.SoundOut.Play();
            this.EmitState();
            this.OnPlayed(true);
        }

        public override void Pause()
        {
            this.SoundOut.Pause();
            this.EmitState();
            this.OnPaused();
        }

        public override void Resume()
        {
            this.SoundOut.Resume();
            this.EmitState();
            this.OnResumed();
        }

        public override void Stop()
        {
            this.StopRequested = true;
            this.SoundOut.Stop();
            this.EmitState();
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
