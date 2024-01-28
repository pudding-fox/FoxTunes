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
            if (this.Output.Float)
            {
                flags |= BassFlags.Float;
            }
            if (this.Output.PlayFromMemory)
            {
                Logger.Write(this, LogLevel.Warn, "This provider cannot play from memory.");
            }
#if NET40
            return TaskEx.FromResult(BassDts.CreateStream(playlistItem.FileName, 0, 0, flags));
#else
            return Task.FromResult(BassDts.CreateStream(playlistItem.FileName, 0, 0, flags));
#endif
        }
    }
}
