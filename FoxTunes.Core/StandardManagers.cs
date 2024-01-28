using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class StandardManagers : IStandardManagers
    {
        private StandardManagers()
        {

        }

        public IPlaybackManager Playback
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IPlaybackManager>();
            }
        }

        public IPlaylistManager Playlist
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IPlaylistManager>();
            }
        }

        public static readonly IStandardManagers Instance = new StandardManagers();
    }
}
