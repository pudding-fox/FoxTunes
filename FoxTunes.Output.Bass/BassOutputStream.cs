using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
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

        public IBassStreamProvider Provider
        {
            get
            {
                return this.Stream.Provider;
            }
        }

        public IEnumerable<IBassStreamAdvice> Advice
        {
            get
            {
                return this.Stream.Advice;
            }
        }

        public int ChannelHandle
        {
            get
            {
                return this.Stream.ChannelHandle;
            }
        }

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

        public override bool IsReady
        {
            get
            {
                var result = default(bool);
                this.Manager.WithPipeline(pipeline =>
                {
                    if (pipeline != null)
                    {
                        result = pipeline.Input.Contains(this);
                    }
                });
                return result;
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

        public override event EventHandler Ending
        {
            add
            {
                this.Stream.Ending += value;
            }
            remove
            {
                this.Stream.Ending -= value;
            }
        }

        public override event EventHandler Ended
        {
            add
            {
                this.Stream.Ended += value;
            }
            remove
            {
                this.Stream.Ended -= value;
            }
        }

        public override bool CanReset
        {
            get
            {
                return this.Stream.CanReset;
            }
        }

        public override void Reset()
        {
            this.Stream.Reset();
        }

        protected override void OnDisposing()
        {
            try
            {
                if (this.Stream != null)
                {
                    if (this.Stream.Syncs != null)
                    {
                        foreach (var sync in this.Stream.Syncs)
                        {
                            Bass.ChannelRemoveSync(this.ChannelHandle, sync);
                        }
                    }
                    this.Stream.Provider.FreeStream(this.PlaylistItem, this.ChannelHandle);
                }
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
