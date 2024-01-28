using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassOutputStream : OutputStream
    {
        public BassOutputStream(IBassOutput output, IBassStreamPipelineManager manager, IBassStream stream, PlaylistItem playlistItem)
            : base(playlistItem)
        {
            this.Output = output;
            this.Manager = manager;
            this.Stream = stream;
            if (!BassOutputStreams.Add(this))
            {
                //TODO: Warn.
            }
        }

        public IBassOutput Output { get; private set; }

        public IBassStreamPipelineManager Manager { get; private set; }

        public IBassStream Stream { get; private set; }

        public int ChannelHandle
        {
            get
            {
                return this.Stream.ChannelHandle;
            }
        }

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
            var position = this.Stream.Position;
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
            this.Stream.Position = position;
            this.NotificationSource.Check();
        }

        public long Offset
        {
            get
            {
                return this.Stream.Offset;
            }
        }

        public override long Length
        {
            get
            {
                return this.Stream.Length;
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
                await this.OnEnding().ConfigureAwait(false);
            }
        }

        protected virtual async void OnEnded(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.OnEnded().ConfigureAwait(false);
            }
        }

        public override bool IsPlaying
        {
            get
            {
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
                this.Stream.Provider.FreeStream(this.PlaylistItem, this.ChannelHandle);
            }
            finally
            {
                if (!BassOutputStreams.Remove(this))
                {
                    //TODO: Warn.
                }
            }
        }
    }
}
