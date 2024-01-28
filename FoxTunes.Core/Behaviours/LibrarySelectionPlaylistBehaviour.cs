using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibrarySelectionPlaylistBehaviour : PlaylistBehaviourBase
    {
        public override Func<Playlist, bool> Predicate
        {
            get
            {
                return playlist => playlist.Type == PlaylistType.Selection && playlist.Enabled;
            }
        }

        public ILibraryManager LibraryManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryManager = core.Managers.Library;
            this.LibraryManager.SelectedItemChanged += this.OnSelectedItemChanged;
            base.InitializeComponent(core);
        }

        protected virtual void OnSelectedItemChanged(object sender, EventArgs e)
        {
            this.Dispatch(() =>
            {
                foreach (var playlist in this.Playlists)
                {
                    var task = this.Refresh(playlist);
                }
            });
        }

        protected override async Task Refresh(Playlist playlist)
        {
            var libraryHierarchyNode = this.LibraryManager.SelectedItem;
            if (libraryHierarchyNode != null && !LibraryHierarchyNode.Empty.Equals(libraryHierarchyNode))
            {
                playlist.Name = libraryHierarchyNode.Value;
                await this.Update(playlist).ConfigureAwait(false);
                await this.PlaylistManager.Add(playlist, libraryHierarchyNode, true).ConfigureAwait(false);
            }
        }

        protected override void OnDisposing()
        {
            if (this.LibraryManager != null)
            {
                this.LibraryManager.SelectedItemChanged -= this.OnSelectedItemChanged;
            }
            base.OnDisposing();
        }
    }
}
