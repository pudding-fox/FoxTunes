using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class RatingManager : StandardManager, IBackgroundTaskSource
    {
        public IMetaDataManager MetaDataManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.HierarchyManager = core.Managers.Hierarchy;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            base.InitializeComponent(core);
        }

        public Task SetRating(LibraryHierarchyNode libraryHierarchyNode, byte rating)
        {
            //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(
                libraryHierarchyNode,
                true
            ).ToArray();
            return this.SetRating(libraryItems, rating);
        }

        public async Task SetRating(IEnumerable<IFileData> fileDatas, byte rating)
        {
            foreach (var fileData in fileDatas)
            {
                lock (fileData.MetaDatas)
                {
                    fileData.AddOrUpdate(CommonStatistics.Rating, MetaDataItemType.Tag, Convert.ToString(rating));
                }
            }
            await this.MetaDataManager.Save(fileDatas, true, false, CommonStatistics.Rating).ConfigureAwait(false);
            await this.HierarchyManager.Refresh(fileDatas, CommonStatistics.Rating).ConfigureAwait(false);
        }

        protected virtual Task OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new BackgroundTaskEventArgs(backgroundTask);
            this.BackgroundTask(this, e);
            return e.Complete();
        }

        public event BackgroundTaskEventHandler BackgroundTask;
    }
}
