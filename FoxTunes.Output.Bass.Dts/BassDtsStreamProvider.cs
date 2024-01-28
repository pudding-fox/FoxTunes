using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dts;
using System;
using System.Collections.Generic;
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

        public override Task<IBassStream> CreateStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice)
        {
            var flags = BassFlags.Decode;
            if (this.Output != null && this.Output.Float)
            {
                flags |= BassFlags.Float;
            }
            return this.CreateStream(playlistItem, flags, advice);
        }

#if NET40
        public override Task<IBassStream> CreateStream(PlaylistItem playlistItem, BassFlags flags, IEnumerable<IBassStreamAdvice> advice)
#else
        public override async Task<IBassStream> CreateStream(PlaylistItem playlistItem, BassFlags flags, IEnumerable<IBassStreamAdvice> advice)
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
    }
}
