using FoxTunes;
using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassDtsStreamProvider : BassStreamProvider
    {
        public static readonly string[] EXTENSIONS = new[]
        {
            "dts"
        };

        public BassDtsStreamProviderBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<BassDtsStreamProviderBehaviour>();
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
            var channelHandle = BassDts.CreateStream(fileName, 0, 0, flags);
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
            channelHandle = BassDts.CreateStream(fileName, 0, 0, flags);
            return this.CreateInteractiveStream(channelHandle, advice);
        }
    }
}
