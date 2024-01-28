using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class BassStreamProvider : BaseComponent, IBassStreamProvider
    {
        public const byte PRIORITY_HIGHEST = 0;

        public const byte PRIORITY_HIGH = 100;

        public const byte PRIORITY_LOW = 255;

        public BassStreamProvider(IBassOutput output)
        {
            this.Output = output;
        }

        public IBassOutput Output { get; private set; }

        public abstract byte Priority { get; }

        public abstract bool CanCreateStream(PlaylistItem playlistItem);

        public abstract int CreateStream(PlaylistItem playlistItem);
    }
}
