using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Memory;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassMemoryStreamProvider : BassStreamProvider
    {
        public BassMemoryBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<BassMemoryBehaviour>();
            base.InitializeComponent(core);
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            if (!this.Behaviour.Enabled)
            {
                return false;
            }
            return base.CanCreateStream(playlistItem);
        }

        public override IBassStream CreateInteractiveStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var fileName = this.GetFileName(playlistItem, advice);
            var channelHandle = BassMemory.CreateStream(fileName, 0, 0, flags);
            if (channelHandle == 0)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create memory stream: {0}", Enum.GetName(typeof(Errors), Bass.LastError));
            }
            return this.CreateInteractiveStream(channelHandle, advice, flags);
        }
    }
}
