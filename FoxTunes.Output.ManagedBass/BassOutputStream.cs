using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class BassOutputStream : OutputStream
    {
        const int UPDATE_INTERVAL = 100;

        static BassOutputStream()
        {
            _ActiveStreams = new List<BassOutputStream>();
            ActiveStreams = new ReadOnlyCollection<BassOutputStream>(_ActiveStreams);
        }

        private static IList<BassOutputStream> _ActiveStreams { get; set; }

        public static IReadOnlyCollection<BassOutputStream> ActiveStreams { get; private set; }

        public BassOutputStream(BassOutput output, PlaylistItem playlistItem, int channelHandle)
            : base(playlistItem)
        {
            this.Output = output;
            this.ChannelHandle = channelHandle;
            _ActiveStreams.Add(this);
        }

        public BassOutput Output { get; private set; }

        public int ChannelHandle { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public BassNotificationSource NotificationSource { get; private set; }

        public override long Position
        {
            get
            {
                return this.GetPosition();
            }
            set
            {
                this.SetPosition(value);
            }
        }

        protected virtual long GetPosition()
        {
            var position = Bass.ChannelGetPosition(this.ChannelHandle);
            if (this.Output != null && this.Output.Pipeline != null)
            {
                var buffer = this.Output.Pipeline.BufferLength;
                if (buffer > 0)
                {
                    position -= Bass.ChannelSeconds2Bytes(this.ChannelHandle, buffer);
                }
            }
            return position;
        }

        protected virtual void SetPosition(long position)
        {
            if (this.Output != null && this.Output.Pipeline != null)
            {
                var buffer = this.Output.Pipeline.BufferLength;
                if (buffer > 0)
                {
                    this.Output.Pipeline.ClearBuffer();
                }
            }
            if (position >= this.Length)
            {
                position = this.Length - 1;
            }
            BassUtils.OK(Bass.ChannelSetPosition(this.ChannelHandle, position));
            if (position > this.NotificationSource.EndingPosition)
            {
                Logger.Write(this, LogLevel.Debug, "Channel {0} was manually seeked past the \"Ending\" sync, raising it manually.", this.ChannelHandle);
                this.NotificationSource.Ending();
            }
        }

        public override long Length
        {
            get
            {
                return Bass.ChannelGetLength(this.ChannelHandle);
            }
        }

        public override int Rate
        {
            get
            {
                return BassUtils.GetChannelPcmRate(this.ChannelHandle);
            }
        }

        public override int Channels
        {
            get
            {
                return BassUtils.GetChannelCount(this.ChannelHandle);
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

        public override bool IsPlaying
        {
            get
            {
                if (!this.Output.IsStarted || this.Output.Pipeline == null)
                {
                    return false;
                }
                return this.Output.Pipeline.Output.IsPlaying;
            }
        }

        public override bool IsPaused
        {
            get
            {
                if (!this.Output.IsStarted || this.Output.Pipeline == null)
                {
                    return false;
                }
                return this.Output.Pipeline.Output.IsPaused;
            }
        }

        public override bool IsStopped
        {
            get
            {
                if (!this.Output.IsStarted || this.Output.Pipeline == null)
                {
                    return false;
                }
                return this.Output.Pipeline.Output.IsStopped;
            }
        }

        public override void Play()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            this.Output.GetOrCreatePipeline(this).Play();
            this.EmitState();
            this.OnPlayed(true);
        }

        public override void Pause()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            this.Output.GetOrCreatePipeline(this).Pause();
            this.EmitState();
            this.OnPaused();
        }

        public override void Resume()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            this.Output.GetOrCreatePipeline(this).Resume();
            this.EmitState();
            this.OnResumed();
        }

        public override void Stop()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            this.Output.GetOrCreatePipeline(this).Stop();
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
            try
            {
                this.Output.FreeStream(this.ChannelHandle);
            }
            finally
            {
                _ActiveStreams.Remove(this);
            }
            this.ChannelHandle = 0;
        }
    }

    public class BassOutputStreamException : OutputStreamException
    {
        private BassOutputStreamException(string message)
            : base(message)
        {

        }

        public static readonly BassOutputStreamException StaleStream = new BassOutputStreamException("Output was shut down since the stream was created, it should have been disposed.");
    }
}
