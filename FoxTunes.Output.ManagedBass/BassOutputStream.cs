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
                return Bass.ChannelGetPosition(this.ChannelHandle);
            }
            set
            {
                Bass.ChannelSetPosition(this.ChannelHandle, value);
            }
        }

        public override long Length
        {
            get
            {
                return Bass.ChannelGetLength(this.ChannelHandle);
            }
        }

        public override int SampleRate
        {
            get
            {
                return Bass.ChannelGetInfo(this.ChannelHandle).Frequency;
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
                return this.Output.MasterChannel.IsPlaying;
            }
        }

        public override bool IsPaused
        {
            get
            {
                return this.Output.MasterChannel.IsPaused;
            }
        }

        public override bool IsStopped
        {
            get
            {
                return this.Output.MasterChannel.IsStopped;
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
            this.Output.MasterChannel.SetPrimaryChannel(this.ChannelHandle);
            this.Output.MasterChannel.Play();
            this.EmitState();
            this.OnPlayed(true);
        }

        public override void Pause()
        {
            this.Output.MasterChannel.Pause();
            this.EmitState();
            this.OnPaused();
        }

        public override void Resume()
        {
            this.Output.MasterChannel.Resume();
            this.EmitState();
            this.OnResumed();
        }

        public override void Stop()
        {
            this.Output.MasterChannel.Stop();
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
            Bass.StreamFree(this.ChannelHandle); //Not checking result code as it contains an error if the application is shutting down.
            this.ChannelHandle = 0;
        }
    }
}
