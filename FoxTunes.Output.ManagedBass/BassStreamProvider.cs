using FoxTunes.Interfaces;
using ManagedBass;

namespace FoxTunes
{
    public class BassStreamProvider : BaseComponent, IBassStreamProvider
    {
        public const byte PRIORITY_HIGH = 0;

        public const byte PRIORITY_NORMAL = 100;

        public const byte PRIORITY_LOW = 255;

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

        public virtual int CreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            var flags = BassFlags.Decode;
            if (output.Float)
            {
                flags |= BassFlags.Float;
            }
            return Bass.CreateStream(playlistItem.FileName, 0, 0, flags);
        }
    }
}
