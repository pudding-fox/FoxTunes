using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    public class BassOutputStream : OutputStream
    {
        static BassOutputStream()
        {
            Instances = new List<WeakReference<BassOutputStream>>();
        }

        private static IList<WeakReference<BassOutputStream>> Instances { get; set; }

        public static IEnumerable<BassOutputStream> Active
        {
            get
            {
                lock (Instances)
                {
                    return Instances
                        .Where(instance => instance != null && instance.IsAlive)
                        .Select(instance => instance.Target)
                        .ToArray();
                }
            }
        }

        protected static void OnActiveChanged(BassOutputStream sender)
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(sender, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;

        public BassOutputStream(IBassOutput output, IBassStreamPipelineManager manager, IBassStream stream, PlaylistItem playlistItem)
            : base(playlistItem)
        {
            this.Output = output;
            this.Manager = manager;
            this.Stream = stream;
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
            return Math.Max(position, 0);
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
                return Math.Max(this.Stream.Length, 0);
            }
        }

        public override int Rate
        {
            get
            {
                if (BassUtils.GetChannelDsdRaw(this.ChannelHandle))
                {
                    return BassUtils.GetChannelDsdRate(this.ChannelHandle);
                }
                else
                {
                    return BassUtils.GetChannelPcmRate(this.ChannelHandle);
                }
            }
        }

        public override int Channels
        {
            get
            {
                return BassUtils.GetChannelCount(this.ChannelHandle);
            }
        }

        public BassFlags Flags
        {
            get
            {
                return BassUtils.GetChannelFlags(this.ChannelHandle);
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

        public override bool IsEnded
        {
            get
            {
                return this.Stream.IsEnded;
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

        public override OutputStreamFormat Format
        {
            get
            {
                if (this.Stream.Flags.HasFlag(BassFlags.DSDRaw))
                {
                    return OutputStreamFormat.DSDRaw;
                }
                else if (this.Stream.Flags.HasFlag(BassFlags.Float))
                {
                    return OutputStreamFormat.Float;
                }
                else
                {
                    return OutputStreamFormat.Short;
                }
            }
        }

        public override T[] GetBuffer<T>(TimeSpan duration)
        {
            var length = Convert.ToInt32(
                Bass.ChannelSeconds2Bytes(this.ChannelHandle, duration.TotalSeconds)
            );
            if (typeof(T) == typeof(short))
            {
                length /= sizeof(short);
            }
            else if (typeof(T) == typeof(float))
            {
                length /= sizeof(float);
            }
            else
            {
                throw new NotImplementedException();
            }
            return new T[length];
        }

        public override int GetData(short[] buffer)
        {
            return Bass.ChannelGetData(this.ChannelHandle, buffer, buffer.Length * sizeof(short));
        }

        public override int GetData(float[] buffer)
        {
            return Bass.ChannelGetData(this.ChannelHandle, buffer, buffer.Length * sizeof(float));
        }

        public override float[] GetBuffer(int fftSize)
        {
            return this.Output.GetBuffer(fftSize);
        }

        public override int GetData(float[] buffer, int fftSize)
        {
            var length = (uint)buffer.Length;
            switch (fftSize)
            {
                case BassFFT.FFT256:
                    length |= BassFFT.FFT256_MASK;
                    break;
                case BassFFT.FFT512:
                    length |= BassFFT.FFT512_MASK;
                    break;
                case BassFFT.FFT1024:
                    length |= BassFFT.FFT1024_MASK;
                    break;
                case BassFFT.FFT2048:
                    length |= BassFFT.FFT2048_MASK;
                    break;
                case BassFFT.FFT4096:
                    length |= BassFFT.FFT4096_MASK;
                    break;
                case BassFFT.FFT8192:
                    length |= BassFFT.FFT8192_MASK;
                    break;
                case BassFFT.FFT16384:
                    length |= BassFFT.FFT16384_MASK;
                    break;
                case BassFFT.FFT32768:
                    length |= BassFFT.FFT32768_MASK;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return Bass.ChannelGetData(this.ChannelHandle, buffer, unchecked((int)length));
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

        public override void InitializeComponent(ICore core)
        {
            lock (Instances)
            {
                Instances.Add(new WeakReference<BassOutputStream>(this));
            }
            OnActiveChanged(this);
            base.InitializeComponent(core);
        }

        protected override void OnDisposing()
        {
            try
            {
                if (this.Stream != null)
                {
                    this.Stream.Dispose();
                }
            }
            finally
            {
                lock (Instances)
                {
                    for (var a = Instances.Count - 1; a >= 0; a--)
                    {
                        var instance = Instances[a];
                        if (instance == null || !instance.IsAlive)
                        {
                            Instances.RemoveAt(a);
                        }
                        else if (object.ReferenceEquals(this, instance.Target))
                        {
                            Instances.RemoveAt(a);
                        }
                    }
                }
                OnActiveChanged(this);
            }
        }
    }
}
