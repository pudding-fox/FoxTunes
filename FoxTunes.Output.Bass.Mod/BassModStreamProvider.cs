using FoxTunes;
using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class BassModStreamProvider : BassStreamProvider
    {
        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(BassModStreamProvider).Assembly.Location);
            }
        }

        public static readonly string[] EXTENSIONS = new[]
        {
            "it",
            "mo3",
            "mod",
            "mptm",
            "mtm",
            "s3m",
            "umx",
            "xm"
        };

        public BassModStreamProvider()
        {
            BassPluginLoader.AddPath(Path.Combine(Location, "Addon"));
            BassPluginLoader.AddExtensions(EXTENSIONS);
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            if (!EXTENSIONS.Contains(playlistItem.FileName.GetExtension(), StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public override IBassStream CreateBasicStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var fileName = this.GetFileName(playlistItem, advice);
            var channelHandle = Bass.MusicLoad(fileName, 0, 0, flags | BassFlags.Prescan);
            if (channelHandle == 0)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create MOD stream: {0}", Enum.GetName(typeof(Errors), Bass.LastError));
            }
            return this.CreateBasicStream(channelHandle, advice, flags);
        }

        public override IBassStream CreateInteractiveStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var fileName = this.GetFileName(playlistItem, advice);
            var channelHandle = Bass.MusicLoad(fileName, 0, 0, flags | BassFlags.Prescan);
            if (channelHandle == 0)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create MOD stream: {0}", Enum.GetName(typeof(Errors), Bass.LastError));
            }
            return this.CreateInteractiveStream(channelHandle, advice, flags);
        }

        public override void FreeStream(int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Freeing stream: {0}", channelHandle);
            Bass.MusicFree(channelHandle); //Not checking result code as it contains an error if the application is shutting down.
        }
    }
}
