using System.IO;

namespace FoxTunes
{
    public class SacdPlaylistItemFactory : SacdItemFactory<PlaylistItem>
    {
        public SacdPlaylistItemFactory(bool reportProgress) : base(reportProgress)
        {

        }
    }
}
