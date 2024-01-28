using System;
using FoxTunes.Interfaces;
using ManagedBass;

namespace FoxTunes
{
    public class BassOutputStream : OutputStream
    {
        const int UPDATE_INTERVAL = 100;

        public BassOutputStream(BassOutput output, PlaylistItem playlistItem, int channelHandle)
            : base(playlistItem)
        {
            this.Output = output;
            this.ChannelHandle = channelHandle;
        }

        public BassOutput Output { get; private set; }

        public int ChannelHandle { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public BassNotificationSource NotificationSource { get; private set; }

        public override long Position
        {
            get
            {
                //return Bass.ChannelGetPosition(this.ChannelHandle);
                return this.Output.OutputChannel.Position;
            }
            set
            {
                Bass.ChannelSetPosition(this.ChannelHandle, value);
                this.Output.OutputChannel.ClearBuffer();
                if (value > this.NotificationSource.EndingPosition)
                {
                    Logger.Write(this, LogLevel.Debug, "Channel {0} was manually seeked past the \"Ending\" sync, raising it manually.", this.ChannelHandle);
                    this.NotificationSource.Ending();
                }
            }
        }

        public override long Length
        {
            get
            {
                return Bass.ChannelGetLength(this.ChannelHandle);
            }
        }

        public override int PCMRate
        {
            get
            {
                var rate = default(float);
                if (!Bass.ChannelGetAttribute(this.ChannelHandle, ChannelAttribute.Frequency, out rate))
                {
                    return 0;
                }
                return Convert.ToInt32(rate);
            }
        }

        public override int DSDRate
        {
            get
            {
                if (!Bass.ChannelHasFlag(this.ChannelHandle, BassFlags.DSDRaw))
                {
                    return 0;
                }
                var rate = default(float);
                if (!Bass.ChannelGetAttribute(this.ChannelHandle, ChannelAttribute.DSDRate, out rate))
                {
                    return 0;
                }
                return Convert.ToInt32(rate);
            }
        }

        public override int Channels
        {
            get
            {
                return Bass.ChannelGetInfo(this.ChannelHandle).Channels;
            }
        }

        public override bool IsPlaying
        {
            get
            {
                if (!this.Output.IsStarted)
                {
                    return false;
                }
                return this.Output.OutputChannel.IsPlaying;
            }
        }

        public override bool IsPaused
        {
            get
            {
                if (!this.Output.IsStarted)
                {
                    return false;
                }
                return this.Output.OutputChannel.IsPaused;
            }
        }

        public override bool IsStopped
        {
            get
            {
                if (!this.Output.IsStarted)
                {
                    return true;
                }
                return this.Output.OutputChannel.IsStopped;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            this.NotificationSource = new BassNotificationSource(this);
            this.NotificationSource.Interval = UPDATE_INTERVAL;
            this.NotificationSource.Updated += this.NotificationSource_Updated;
            this.NotificationSource.Stopping += this.NotificationSource_Stopping;
            this.NotificationSource.Stopped += this.NotificationSource_Stopped;
            this.NotificationSource.InitializeComponent(core);
            base.InitializeComponent(core);
        }

        protected virtual void NotificationSource_Updated(object sender, BassNotificationSourceEventArgs e)
        {
            this.ForegroundTaskRunner.RunAsync(() => this.OnPositionChanged());
        }

        protected virtual void NotificationSource_Stopping(object sender, BassNotificationSourceEventArgs e)
        {
            this.ForegroundTaskRunner.RunAsync(() =>
            {
                this.EmitState();
                this.OnStopping();
            });
        }

        protected virtual void NotificationSource_Stopped(object sender, BassNotificationSourceEventArgs e)
        {
            this.ForegroundTaskRunner.RunAsync(() =>
            {
                this.EmitState();
                this.OnStopped(false);
            });
        }

        public override void Play()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            this.Output.OutputChannel.Play(this, true);
            this.EmitState();
            this.OnPlayed(true);
        }

        public override void Pause()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            this.Output.OutputChannel.Pause();
            this.EmitState();
            this.OnPaused();
        }

        public override void Resume()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            this.Output.OutputChannel.Resume();
            this.EmitState();
            this.OnResumed();
        }

        public override void Stop()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            this.Output.OutputChannel.Stop();
            this.EmitState();
            this.OnStopped(true);
        }

        protected virtual void ChannelSyncEnd(int handle, int channel, int data, IntPtr user)
        {
            this.EmitState();
            this.OnStopped(false);
        }

        protected override void OnDisposing()
        {
            if (this.Output.IsStarted && this.Output.OutputChannel.QueueContains(this))
            {
                this.Output.OutputChannel.Remove(this);
            }
            this.Output.FreeStream(this.ChannelHandle);
            this.ChannelHandle = 0;
        }
    }

    public class BassOutputStreamException : OutputStreamException
    {
        private BassOutputStreamException(string message) : base(message)
        {

        }

        public static readonly BassOutputStreamException StaleStream = new BassOutputStreamException("Output was shut down since the stream was created, it should have been disposed.");
    }
}
