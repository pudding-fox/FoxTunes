using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class RatingManager : StandardManager, IBackgroundTaskSource
    {
        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        public async Task SetRating(LibraryHierarchyNode libraryHierarchyNode, byte rating)
        {
            using (var task = new UpdateLibraryRatingTask(libraryHierarchyNode, rating))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task SetRating(IEnumerable<LibraryItem> libraryItems, byte rating)
        {
            using (var task = new UpdateLibraryRatingTask(libraryItems, rating))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task SetRating(IEnumerable<PlaylistItem> playlistItems, byte rating)
        {
            using (var task = new UpdatePlaylistRatingTask(playlistItems, rating))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
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
