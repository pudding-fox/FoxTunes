using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistsActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string ADD_PLAYLIST = "QQQQ";

        public const string REMOVE_PLAYLIST = "RRRR";

        public const string MANAGE_PLAYLISTS = "SSSS";

        public const string CREATE_PLAYLIST = "AAAD";

        public PlaylistsActionsBehaviour()
        {
            Instance = this;
        }

        public bool Enabled
        {
            get
            {
                return LayoutManager.Instance.IsComponentActive(typeof(Playlists));
            }
        }

        protected virtual void OnEnabledChanged()
        {
            if (!this.Enabled)
            {
                var playlists = this.PlaylistBrowser.GetPlaylists();
                this.PlaylistManager.SelectedPlaylist = playlists.FirstOrDefault();
            }
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Enabled");
        }

        public event EventHandler EnabledChanged;

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            LayoutManager.Instance.ActiveComponentsChanged += this.OnEnabledChanged;
            this.Core = core;
            this.LibraryManager = core.Managers.Library;
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            base.InitializeComponent(core);
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs e)
        {
            this.OnEnabledChanged();
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLISTS, ADD_PLAYLIST, "Add Playlist");
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLISTS, REMOVE_PLAYLIST, "Remove Playlist");
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLISTS, MANAGE_PLAYLISTS, "Playlist Manager", attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
                    if (this.LibraryManager.SelectedItem != null)
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, CREATE_PLAYLIST, "Create Playlist");
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case ADD_PLAYLIST:
                    return this.AddPlaylist();
                case REMOVE_PLAYLIST:
                    return this.RemovePlaylist();
                case MANAGE_PLAYLISTS:
                    return this.ManagePlaylists();
                case CREATE_PLAYLIST:
                    return this.AddPlaylist(this.LibraryManager.SelectedItem);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task AddPlaylist()
        {
            var playlist = this.CreatePlaylist();
            await this.PlaylistManager.Add(playlist).ConfigureAwait(false);
            this.PlaylistManager.SelectedPlaylist = playlist;
        }

        public async Task AddPlaylist(IEnumerable<string> paths)
        {
            var playlist = this.CreatePlaylist();
            await this.PlaylistManager.Add(playlist, paths).ConfigureAwait(false);
            this.PlaylistManager.SelectedPlaylist = playlist;
        }

        public async Task AddPlaylist(LibraryHierarchyNode libraryHierarchyNode)
        {
            var playlist = new Playlist()
            {
                Name = libraryHierarchyNode.Value,
                Enabled = true
            };
            await this.PlaylistManager.Add(playlist, libraryHierarchyNode).ConfigureAwait(false);
            this.PlaylistManager.SelectedPlaylist = playlist;
        }

        public async Task AddPlaylist(IEnumerable<PlaylistItem> playlistItems)
        {
            var playlist = this.CreatePlaylist();
            await this.PlaylistManager.Add(playlist, playlistItems).ConfigureAwait(false);
            this.PlaylistManager.SelectedPlaylist = playlist;
        }

        public async Task RemovePlaylist()
        {
            var playlist = this.PlaylistManager.SelectedPlaylist;
            var playlists = this.PlaylistBrowser.GetPlaylists();
            if (playlists.Length > 1)
            {
                var index = playlists.IndexOf(this.PlaylistManager.SelectedPlaylist);
                if (index > 0)
                {
                    index--;
                }
                else
                {
                    index++;
                }
                this.PlaylistManager.SelectedPlaylist = playlists[index];
                await this.PlaylistManager.Remove(playlist).ConfigureAwait(false);
            }
            else
            {
                await this.PlaylistManager.Remove(playlist).ConfigureAwait(false);
                await this.AddPlaylist().ConfigureAwait(false);
            }
        }

        public Task ManagePlaylists()
        {
            return Windows.Invoke(() =>
            {
                if (!Windows.IsPlaylistManagerWindowCreated)
                {
                    Windows.PlaylistManagerWindow.DataContext = this.Core;
                    Windows.PlaylistManagerWindow.Show();
                }
            });
        }

        protected virtual Playlist CreatePlaylist()
        {
            var playlists = this.PlaylistBrowser.GetPlaylists();
            return new Playlist()
            {
                Name = Playlist.GetName(playlists),
                Enabled = true
            };
        }

        public static PlaylistsActionsBehaviour Instance { get; private set; }
    }
}
