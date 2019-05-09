using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassOutputStream : OutputStream
    {
        const int UPDATE_INTERVAL = 100;

        static BassOutputStream()
        {
            ActiveStreams = new ConcurrentDictionary<PlaylistItem, BassOutputStream>();
        }

        public static ConcurrentDictionary<PlaylistItem, BassOutputStream> ActiveStreams { get; private set; }

        public BassOutputStream(IBassOutput output, IBassStreamPipelineManager manager, IBassStreamProvider provider, PlaylistItem playlistItem, int channelHandle)
            : base(playlistItem)
        {
            this.Output = output;
            this.Manager = manager;
            this.Provider = provider;
            this.ChannelHandle = channelHandle;
            if (!ActiveStreams.TryAdd(playlistItem, this))
            {
                //TODO: Warn.
            }
        }

        public IBassOutput Output { get; private set; }

        public IBassStreamPipelineManager Manager { get; private set; }

        public IBassStreamProvider Provider { get; private set; }

        public int ChannelHandle { get; private set; }

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
            this.Manager.WithPipeline(pipeline =>
            {
                if (pipeline != null)
                {
                    var buffer = pipeline.BufferLength;
                    if (buffer > 0)
                    {
                        position -= Bass.ChannelSeconds2Bytes(this.ChannelHandle, buffer);
                    }
                }
            });
            return position;
        }

        protected virtual void SetPosition(long position)
        {
            this.Manager.WithPipeline(pipeline =>
            {
                if (pipeline != null)
                {
                    var buffer = pipeline.BufferLength;
                    if (buffer > 0)
                    {
                        pipeline.ClearBuffer();
                    }
                }
            });
            if (position >= this.Length)
            {
                position = this.Length - 1;
            }
            BassUtils.OK(Bass.ChannelSetPosition(this.ChannelHandle, position));
            if (position > this.NotificationSource.EndingPosition)
            {
                Logger.Write(this, LogLevel.Debug, "Channel {0} was manually seeked past the \"Ending\" sync, raising it manually.", this.ChannelHandle);
                this.NotificationSource.OnEnding();
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
            this.NotificationSource = new BassNotificationSource(this);
            this.NotificationSource.Ending += this.OnEnding;
            this.NotificationSource.Ended += this.OnEnded;
            this.NotificationSource.InitializeComponent(core);
            base.InitializeComponent(core);
        }

        protected virtual async void OnEnding(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.OnEnding();
            }
        }

        protected virtual async void OnEnded(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.OnEnded();
            }
        }

        public override bool IsPlaying
        {
            get
            {
                if (!this.Output.IsStarted)
                {
                    throw BassOutputStreamException.StaleStream;
                }
                var result = default(bool);
                this.Manager.WithPipeline(pipeline =>
                {
                    if (pipeline != null)
                    {
                        result = pipeline.Output.IsPlaying;
                    }
                });
                return result;
            }
        }

        public override bool IsPaused
        {
            get
            {
                if (!this.Output.IsStarted)
                {
                    throw BassOutputStreamException.StaleStream;
                }
                var result = default(bool);
                this.Manager.WithPipeline(pipeline =>
                {
                    if (pipeline != null)
                    {
                        result = pipeline.Output.IsPaused;
                    }
                });
                return result;
            }
        }

        public override bool IsStopped
        {
            get
            {
                if (!this.Output.IsStarted)
                {
                    throw BassOutputStreamException.StaleStream;
                }
                var result = default(bool);
                this.Manager.WithPipeline(pipeline =>
                {
                    if (pipeline != null)
                    {
                        result = pipeline.Output.IsStopped;
                    }
                });
                return result;
            }
        }

        public override Task Play()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            return this.Manager.WithPipelineExclusive(this, pipeline =>
            {
                if (pipeline != null)
                {
                    pipeline.Play();
                }
            });
        }

        public override Task Pause()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            return this.Manager.WithPipelineExclusive(this, pipeline =>
            {
                if (pipeline != null)
                {
                    pipeline.Pause();
                }
            });
        }

        public override Task Resume()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            return this.Manager.WithPipelineExclusive(this, pipeline =>
            {
                if (pipeline != null)
                {
                    pipeline.Resume();
                }
            });
        }

        public override Task Stop()
        {
            if (!this.Output.IsStarted)
            {
                throw BassOutputStreamException.StaleStream;
            }
            return this.Manager.WithPipelineExclusive(this, pipeline =>
            {
                if (pipeline != null)
                {
                    pipeline.Stop();
                }
            });
        }

        public override TimeSpan GetDuration(long position)
        {
            return TimeSpan.FromSeconds(Bass.ChannelBytes2Seconds(this.ChannelHandle, position));
        }

        protected override void OnDisposing()
        {
            if (this.NotificationSource != null)
            {
                this.NotificationSource.Ending -= this.OnEnding;
                this.NotificationSource.Ended -= this.OnEnded;
            }
            try
            {
                this.Provider.FreeStream(this.PlaylistItem, this.ChannelHandle);
            }
            finally
            {
                if (!ActiveStreams.TryRemove(this.PlaylistItem))
                {
                    //TODO: Warn.
                }
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
