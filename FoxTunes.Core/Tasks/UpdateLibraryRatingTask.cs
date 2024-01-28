using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class UpdateLibraryRatingTask : BackgroundTask
    {
        public const string ID = "0DA1A44E-0FEC-492B-9F57-92F402196792";

        public UpdateLibraryRatingTask(LibraryHierarchyNode libraryHierarchyNode, byte rating) : base(ID)
        {
            this.LibraryHierarchyNode = libraryHierarchyNode;
            this.Rating = rating;
        }

        public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

        public byte Rating { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(
                this.LibraryHierarchyNode,
                true
            ).ToArray();
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
            }
            await this.MetaDataManager.Save(libraryItems, true, false, CommonMetaData.Rating).ConfigureAwait(false);
        }
    }
}
