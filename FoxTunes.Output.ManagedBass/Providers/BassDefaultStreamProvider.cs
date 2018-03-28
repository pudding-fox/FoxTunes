using FoxTunes.Interfaces;
using ManagedBass;

namespace FoxTunes
{
    public class BassDefaultStreamProvider : BassStreamProvider
    {
        public BassDefaultStreamProvider(IBassOutput output)
            : base(output)
        {

        }

        public override byte Priority
        {
            get
            {
                return PRIORITY_LOW;
            }
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            return true;
        }

        public override int CreateStream(PlaylistItem playlistItem)
        {
            var channelHandle = Bass.CreateStream(playlistItem.FileName, 0, 0, this.Output.Flags);
            if (channelHandle == 0)
            {
                BassUtils.Throw();
            }
            return channelHandle;
        }
    }
}
