using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassDtsStreamProvider : BassStreamProvider
    {
        public override byte Priority
        {
            get
            {
                return PRIORITY_HIGH;
            }
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            if (!new[]
            {
                "dts"
            }.Contains(playlistItem.FileName.GetExtension(), StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public override Task<int> CreateStream(PlaylistItem playlistItem)
        {
            var flags = BassFlags.Decode;
            if (this.Output != null && this.Output.Float)
            {
                flags |= BassFlags.Float;
            }
            return this.CreateStream(playlistItem, flags);
        }

#if NET40
        public override Task<int> CreateStream(PlaylistItem playlistItem, BassFlags flags)
#else
        public override async Task<int> CreateStream(PlaylistItem playlistItem, BassFlags flags)
#endif
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                if (this.Output != null && this.Output.PlayFromMemory)
                {
                    Logger.Write(this, LogLevel.Warn, "This provider cannot play from memory.");
                }
                var channelHandle = BassDts.CreateStream(playlistItem.FileName, 0, 0, flags);
#if NET40
                return TaskEx.FromResult(channelHandle);
#else
                return channelHandle;
#endif
            }
            finally
            {
                this.Semaphore.Release();
            }
        }
    }
}
