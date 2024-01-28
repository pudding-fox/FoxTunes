using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPlaylistTask : PlaylistTaskBase
    {
        public AddPlaylistTask(Playlist playlist, LibraryHierarchyNode libraryHierarchyNode = null) : base(playlist)
        {
            this.LibraryHierarchyNode = libraryHierarchyNode;
        }

        public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            await this.AddPlaylist().ConfigureAwait(false);
            if (this.LibraryHierarchyNode != null)
            {
                await this.AddPlaylistItems(this.LibraryHierarchyNode, this.LibraryHierarchyBrowser.Filter).ConfigureAwait(false);
                await this.SetPlaylistItemsStatus(PlaylistItemStatus.None).ConfigureAwait(false);
            }
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, new[] { this.Playlist })).ConfigureAwait(false);
        }
    }
}
