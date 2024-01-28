using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dts;
using System;
using System.Linq;

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

        public override bool CanCreateStream(IBassOutput output, PlaylistItem playlistItem)
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

        public override int CreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            var flags = BassFlags.Decode;
            if (output.Float)
            {
                flags |= BassFlags.Float;
            }
            return BassDts.CreateStream(playlistItem.FileName, 0, 0, flags);
        }
    }
}
