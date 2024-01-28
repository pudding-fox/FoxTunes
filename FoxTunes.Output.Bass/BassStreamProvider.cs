using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassStreamProvider : StandardComponent, IBassStreamProvider
    {
        public const byte PRIORITY_HIGH = 0;

        public const byte PRIORITY_NORMAL = 100;

        public const byte PRIORITY_LOW = 255;

        public static readonly SyncProcedure EndProcedure = new SyncProcedure((Handle, Channel, Data, User) => Bass.ChannelStop(Handle));

        public BassStreamProvider()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public IBassOutput Output { get; private set; }

        public IBassStreamFactory StreamFactory { get; private set; }

        public IBassStreamPipelineManager PipelineManager { get; private set; }

        public virtual byte Priority
        {
            get
            {
                return PRIORITY_NORMAL;
            }
        }

        public virtual BassStreamProviderFlags Flags
        {
            get
            {
                return BassStreamProviderFlags.None;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.StreamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            this.PipelineManager = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineManager>();
            if (this.StreamFactory != null)
            {
                this.StreamFactory.Register(this);
            }
            base.InitializeComponent(core);
        }

        public virtual bool CanCreateStream(PlaylistItem playlistItem)
        {
            return true;
        }

        public virtual Task<IBassStream> CreateStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice)
        {
            var flags = BassFlags.Decode;
            if (this.Output != null && this.Output.Float)
            {
                flags |= BassFlags.Float;
            }
            return this.CreateStream(playlistItem, flags, advice);
        }

#if NET40
        public virtual Task<IBassStream> CreateStream(PlaylistItem playlistItem, BassFlags flags, IEnumerable<IBassStreamAdvice> advice)
#else
        public virtual async Task<IBassStream> CreateStream(PlaylistItem playlistItem, BassFlags flags, IEnumerable<IBassStreamAdvice> advice)
#endif
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                var channelHandle = default(int);
                var fileName = this.GetFileName(playlistItem, advice);
                if (this.Output != null && this.Output.PlayFromMemory)
                {
                    channelHandle = BassInMemoryHandler.CreateStream(fileName, 0, 0, flags);
                    if (channelHandle == 0)
                    {
                        Logger.Write(this, LogLevel.Warn, "Failed to load file into memory: {0}", fileName);
                        channelHandle = Bass.CreateStream(fileName, 0, 0, flags);
                    }
                }
                else
                {
                    channelHandle = Bass.CreateStream(fileName, 0, 0, flags);
                }
#if NET40
                return TaskEx.FromResult(this.CreateStream(channelHandle, advice));
#else
                return this.CreateStream(channelHandle, advice);
#endif
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected virtual IBassStream CreateStream(int channelHandle, IEnumerable<IBassStreamAdvice> advice)
        {
            var stream = default(IBassStream);
            foreach (var advisory in advice)
            {
                if (advisory.Wrap(this, channelHandle, out stream))
                {
                    break;
                }
            }
            if (stream == null)
            {
                stream = new BassStream(this, channelHandle, Bass.ChannelGetLength(channelHandle, PositionFlags.Bytes));
            }
            stream.RegisterSyncHandlers();
            return stream;
        }

        protected virtual string GetFileName(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice)
        {
            foreach (var advisory in advice)
            {
                if (!string.IsNullOrEmpty(advisory.FileName))
                {
                    return advisory.FileName;
                }
            }
            return playlistItem.FileName;
        }

        public virtual void FreeStream(PlaylistItem playlistItem, int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Freeing stream: {0}", channelHandle);
            Bass.StreamFree(channelHandle); //Not checking result code as it contains an error if the application is shutting down.
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            //Nothing to do.
        }

        ~BassStreamProvider()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public class BassStreamProviderKey : IEquatable<BassStreamProviderKey>
        {
            public BassStreamProviderKey(string fileName, int channelHandle)
            {
                this.FileName = fileName;
                this.ChannelHandle = channelHandle;
            }

            public string FileName { get; private set; }

            public int ChannelHandle { get; private set; }

            public virtual bool Equals(BassStreamProviderKey other)
            {
                if (other == null)
                {
                    return false;
                }
                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }
                if (!string.Equals(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (this.ChannelHandle != other.ChannelHandle)
                {
                    return false;
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as BassStreamProviderKey);
            }

            public override int GetHashCode()
            {
                var hashCode = default(int);
                unchecked
                {
                    if (!string.IsNullOrEmpty(this.FileName))
                    {
                        hashCode += this.FileName.GetHashCode();
                    }
                    hashCode += this.ChannelHandle.GetHashCode();
                }
                return hashCode;
            }

            public static bool operator ==(BassStreamProviderKey a, BassStreamProviderKey b)
            {
                if ((object)a == null && (object)b == null)
                {
                    return true;
                }
                if ((object)a == null || (object)b == null)
                {
                    return false;
                }
                if (object.ReferenceEquals((object)a, (object)b))
                {
                    return true;
                }
                return a.Equals(b);
            }

            public static bool operator !=(BassStreamProviderKey a, BassStreamProviderKey b)
            {
                return !(a == b);
            }
        }
    }
}
