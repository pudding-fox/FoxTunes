using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class UpdatePlaylistRatingTask : BackgroundTask
    {
        public const string ID = "7713A9E4-99DF-43A3-A66A-642B6F5C2761";

        public UpdatePlaylistRatingTask(IEnumerable<PlaylistItem> playlistItems, byte rating) : base(ID)
        {
            this.PlaylistItems = playlistItems;
            this.Rating = rating;
        }

        public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

        public byte Rating { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            var refreshPlaylist = default(bool);
            foreach (var playlistItem in this.PlaylistItems)
            {
                lock (playlistItem.MetaDatas)
                {
                    var metaDataItem = playlistItem.MetaDatas.FirstOrDefault(
                        _metaDataItem => string.Equals(_metaDataItem.Name, CommonMetaData.Rating, StringComparison.OrdinalIgnoreCase)
                    );
                    if (metaDataItem == null)
                    {
                        metaDataItem = new MetaDataItem(CommonMetaData.Rating, MetaDataItemType.Tag);
                        playlistItem.MetaDatas.Add(metaDataItem);
                        refreshPlaylist = true;
                    }
                    metaDataItem.Value = Convert.ToString(this.Rating);
                }
            }
            if (refreshPlaylist)
            {
                //We only need to refresh if a meta data entry was created rather than updated as the view model monitors the values.
                //Send a soft playlist update so the list refreshes, this makes the task seem more responsive.
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, CommonSignalFlags.SOFT)).ConfigureAwait(false);
            }
            await this.MetaDataManager.Save(this.PlaylistItems, true, false, CommonMetaData.Rating).ConfigureAwait(false);
        }
    }
}
