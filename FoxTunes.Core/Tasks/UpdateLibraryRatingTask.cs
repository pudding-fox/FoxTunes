using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class UpdateLibraryRatingTask : LibraryTaskBase
    {
        public UpdateLibraryRatingTask(LibraryHierarchyNode libraryHierarchyNode, byte rating)
        {
            this.LibraryHierarchyNode = libraryHierarchyNode;
            this.Rating = rating;
        }

        public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

        public byte Rating { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IPlaylistCache PlaylistCache { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.PlaylistCache = core.Components.PlaylistCache;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(
                this.LibraryHierarchyNode,
                true
            ).ToArray();
            var refreshPlaylist = default(bool);
            foreach (var libraryItem in libraryItems)
            {
                lock (libraryItem.MetaDatas)
                {
                    var metaDataItem = libraryItem.MetaDatas.FirstOrDefault(
                        _metaDataItem => string.Equals(_metaDataItem.Name, CommonMetaData.Rating, StringComparison.OrdinalIgnoreCase)
                    );
                    if (metaDataItem == null)
                    {
                        metaDataItem = new MetaDataItem(CommonMetaData.Rating, MetaDataItemType.Tag);
                        libraryItem.MetaDatas.Add(metaDataItem);
                    }
                    metaDataItem.Value = Convert.ToString(this.Rating);
                }
                if (!refreshPlaylist)
                {
                    refreshPlaylist = this.PlaylistCache.Contains(playlistItem => playlistItem.LibraryItem_Id == libraryItem.Id);
                }
            }
            await this.MetaDataManager.Save(libraryItems, true, CommonMetaData.Rating).ConfigureAwait(false);
            if (refreshPlaylist)
            {
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
            }
        }
    }
}
