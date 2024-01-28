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

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(
                this.LibraryHierarchyNode,
                true
            ).ToArray();
            foreach (var libraryItem in libraryItems)
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
            return this.MetaDataManager.Save(libraryItems, CommonMetaData.Rating);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated)).ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated)).ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
        }
    }
}
