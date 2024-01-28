using FoxTunes;
using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassModStreamProvider : BassStreamProvider
    {
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

        public BassModStreamProviderBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<BassModStreamProviderBehaviour>();
            base.InitializeComponent(core);
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
            return this.CreateBasicStream(channelHandle, advice);
        }

        public override IBassStream CreateInteractiveStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var fileName = this.GetFileName(playlistItem, advice);
            var channelHandle = default(int);
            if (this.Output != null && this.Output.PlayFromMemory)
            {
                Logger.Write(this, LogLevel.Warn, "This provider cannot play from memory.");
            }
            channelHandle = Bass.MusicLoad(fileName, 0, 0, flags | BassFlags.Prescan);
            return this.CreateInteractiveStream(channelHandle, advice);
        }

        public override void FreeStream(PlaylistItem playlistItem, int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Freeing stream: {0}", channelHandle);
            Bass.MusicFree(channelHandle); //Not checking result code as it contains an error if the application is shutting down.
        }
    }
}
