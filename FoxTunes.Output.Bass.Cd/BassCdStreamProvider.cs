using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Cd;
using System;

namespace FoxTunes
{
    public class BassCdStreamProvider : BassStreamProvider
    {
        public const string SCHEME = "cda";

        public override byte Priority
        {
            get
            {
                return PRIORITY_HIGH;
            }
        }

        public override bool CanCreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            var drive = default(int);
            var track = default(int);
            return ParseUrl(playlistItem.FileName, out drive, out track);
        }

        public override int CreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            var drive = default(int);
            var track = default(int);
            if (!ParseUrl(playlistItem.FileName, out drive, out track))
            {
                return 0;
            }
            var flags = BassFlags.Decode;
            if (output.Float)
            {
                flags |= BassFlags.Float;
            }
            return BassCd.CreateStream(drive, track, flags);
        }

        public static string CreateUrl(int drive, int track)
        {
            return string.Format("{0}://{1}/{2}", SCHEME, drive, track);
        }

        public static bool ParseUrl(string url, out int drive, out int track)
        {
            return ParseUrl(new Uri(url), out drive, out track);
        }

        public static bool ParseUrl(Uri url, out int drive, out int track)
        {
            drive = default(int);
            track = default(int);
            if (!string.Equals(url.Scheme, SCHEME, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return
                int.TryParse(url.GetComponents(UriComponents.Host, UriFormat.Unescaped), out drive) &&
                int.TryParse(url.GetComponents(UriComponents.Path, UriFormat.Unescaped), out track);
        }
    }
}
