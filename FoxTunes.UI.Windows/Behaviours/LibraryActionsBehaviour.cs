using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LibraryActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string APPEND_PLAYLIST = "AAAB";

        public const string REPLACE_PLAYLIST = "AAAC";

        public const string RESCAN = "ZZAA";

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IMetaDataBrowser MetaDataBrowser { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryManager = core.Managers.Library;
            this.MetaDataBrowser = core.Components.MetaDataBrowser;
            this.UserInterface = core.Components.UserInterface;
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.LibraryManager.SelectedItem != null)
                {
                    if (this.PlaylistManager.SelectedPlaylist != null)
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, APPEND_PLAYLIST, Strings.LibraryActionsBehaviour_AppendPlaylist);
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, REPLACE_PLAYLIST, Strings.LibraryActionsBehaviour_ReplacePlaylist);
                    }
                }
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, RESCAN, Strings.LibraryActionsBehaviour_Rescan, path: Strings.LibraryActionsBehaviour_Library);
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
                case RESCAN:
                    return this.Rescan();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        private Task AddToPlaylist(bool clear)
        {
            if (this.LibraryManager.SelectedItem == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.PlaylistManager.Add(
                this.PlaylistManager.SelectedPlaylist,
                this.LibraryManager.SelectedItem,
                clear
            );
        }

        protected virtual Task Rescan()
        {
            return this.LibraryManager.Rescan(false);
        }
    }
}
