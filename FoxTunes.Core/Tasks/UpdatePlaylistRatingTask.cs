using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class UpdatePlaylistRatingTask : PlaylistTaskBase
    {
        public UpdatePlaylistRatingTask(IEnumerable<PlaylistItem> playlistItems, byte rating)
        {
            this.PlaylistItems = playlistItems;
            this.Rating = rating;
        }

        public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

        public byte Rating { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
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
                    }
                    metaDataItem.Value = Convert.ToString(this.Rating);
                }
            }
            //Send a soft playlist update so the list refreshes, this makes the task seem more responsive.
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, CommonSignalFlags.SOFT)).ConfigureAwait(false);
            await this.MetaDataManager.Save(this.PlaylistItems, true, CommonMetaData.Rating).ConfigureAwait(false);
        }
    }
}
