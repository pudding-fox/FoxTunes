using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.ZipStream;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassArchiveStreamProvider : BassStreamProvider
    {
        public BassArchiveStreamProviderBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<BassArchiveStreamProviderBehaviour>();
            base.InitializeComponent(core);
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            //The behaviour is not loaded for utilities so we can't check whether it's enabled.
            //if (this.Behaviour == null || !this.Behaviour.Enabled)
            //{
            //    return false;
            //}
            var fileName = default(string);
            var entryName = default(string);
            return ArchiveUtils.ParseUrl(playlistItem.FileName, out fileName, out entryName);
        }

        public override IBassStream CreateBasicStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var fileName = default(string);
            var entryName = default(string);
            if (!ArchiveUtils.ParseUrl(playlistItem.FileName, out fileName, out entryName))
            {
                //This shouldn't happen as CanCreateStream would have returned false.
                return BassStream.Empty;
            }
            var index = default(int);
            if (!ArchiveUtils.GetEntryIndex(fileName, entryName, out index))
            {
                //The associated entry was not found.
                return BassStream.Empty;
            }
            var channelHandle = BassZipStream.CreateStream(fileName, index, Flags: flags);
            return this.CreateBasicStream(channelHandle, advice, flags);
        }

        public override IBassStream CreateInteractiveStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var fileName = default(string);
            var entryName = default(string);
            if (!ArchiveUtils.ParseUrl(playlistItem.FileName, out fileName, out entryName))
            {
                //This shouldn't happen as CanCreateStream would have returned false.
                return BassStream.Empty;
            }
            var index = default(int);
            if (!ArchiveUtils.GetEntryIndex(fileName, entryName, out index))
            {
                //The associated entry was not found.
                return BassStream.Empty;
            }
            var channelHandle = BassZipStream.CreateStream(fileName, index, Flags: flags);
            return this.CreateInteractiveStream(channelHandle, advice, flags);
        }
    }
}
