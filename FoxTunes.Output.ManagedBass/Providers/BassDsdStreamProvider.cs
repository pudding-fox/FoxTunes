using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dsd;
using System;
using System.Linq;

namespace FoxTunes
{
    public class BassDsdStreamProvider : BassStreamProvider
    {
        public BassDsdStreamProvider(IBassOutput output)
            : base(output)
        {

        }

        public override byte Priority
        {
            get
            {
                return PRIORITY_HIGH;
            }
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            return this.Output.DsdDirect && new[]
            {
                "dsd",
                "dsf"
            }.Contains(playlistItem.FileName.GetExtension(), StringComparer.OrdinalIgnoreCase);
        }

        public override int CreateStream(PlaylistItem playlistItem)
        {
            var flags = (this.Output.Flags & ~BassFlags.Float) | BassFlags.DSDRaw;
            var channelHandle = BassDsd.CreateStream(playlistItem.FileName, 0, 0, flags);
            if (channelHandle == 0)
            {
                BassUtils.Throw();
            }
            return channelHandle;
        }
    }
}
