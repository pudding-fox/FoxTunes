using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassStreamProvider : BaseComponent, IBassStreamProvider
    {
        public const byte PRIORITY_HIGH = 0;

        public const byte PRIORITY_NORMAL = 100;

        public const byte PRIORITY_LOW = 255;

        public BassStreamProvider()
        {
            this.Streams = new ConcurrentDictionary<int, byte[]>();
        }

        public ConcurrentDictionary<int, byte[]> Streams { get; private set; }

        public virtual byte Priority
        {
            get
            {
                return PRIORITY_NORMAL;
            }
        }

        public virtual bool CanCreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            return true;
        }

        public virtual async Task<int> CreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            var flags = BassFlags.Decode;
            if (output.Float)
            {
                flags |= BassFlags.Float;
            }
            if (output.PlayFromMemory)
            {
                Logger.Write(this, LogLevel.Debug, "Loading file \"{0}\" into memory.", playlistItem.FileName);
                using (var stream = File.OpenRead(playlistItem.FileName))
                {
                    var buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    Logger.Write(this, LogLevel.Debug, "Buffered {0} bytes from file \"{0}\".", buffer.Length, playlistItem.FileName);
                    var channelHandle = Bass.CreateStream(buffer, 0, buffer.Length, flags);
                    if (!this.Streams.TryAdd(channelHandle, buffer))
                    {
                        Logger.Write(this, LogLevel.Warn, "Failed to pin handle of buffer for file \"{0}\". Playback may fail.", playlistItem.FileName);
                    }
                    return channelHandle;
                }
            }
            return Bass.CreateStream(playlistItem.FileName, 0, 0, flags);
        }

        public virtual void FreeStream(int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Freeing stream: {0}", channelHandle);
            if (this.Streams.TryRemove(channelHandle))
            {
                Logger.Write(this, LogLevel.Debug, "Released handle of buffer for channel {0}.", channelHandle);
            }
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
            this.Streams.Clear();
        }

        ~BassStreamProvider()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
