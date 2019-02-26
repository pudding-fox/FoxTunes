using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string APPEND_PLAYLIST = "AAAB";

        public const string REPLACE_PLAYLIST = "AAAC";

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryManager = core.Managers.Library;
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, APPEND_PLAYLIST, "Add To Playlist");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, REPLACE_PLAYLIST, "Replace Playlist");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case APPEND_PLAYLIST:
                    return this.AddToPlaylist(false);
                case REPLACE_PLAYLIST:
                    return this.AddToPlaylist(true);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        private async Task AddToPlaylist(bool clear)
        {
            await this.PlaylistManager.Add(this.LibraryManager.SelectedItem, clear);
        }
    }
}
