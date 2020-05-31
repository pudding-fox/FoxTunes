using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistsActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string ADD_PLAYLIST = "QQQQ";

        public const string REMOVE_PLAYLIST = "RRRR";

        public PlaylistsActionsBehaviour()
        {
            Instance = this;
        }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLISTS, ADD_PLAYLIST, "Add Playlist");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLISTS, REMOVE_PLAYLIST, "Remove Playlist");
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
                //Invoke so that the SelectedItem binding is updated before the ItemsSource (Playlists).
                await Windows.Invoke(() => this.PlaylistManager.SelectedPlaylist = playlists[index]).ConfigureAwait(false);
                await this.PlaylistManager.Remove(playlist).ConfigureAwait(false);
            }
            else
            {
                await this.PlaylistManager.Remove(playlist).ConfigureAwait(false);
                await this.AddPlaylist().ConfigureAwait(false);
            }
        }

        protected virtual Playlist CreatePlaylist()
        {
            var name = "New Playlist";
            for (var a = 1; a < 100; a++)
            {
                var success = true;
                foreach (var playlist in this.PlaylistBrowser.GetPlaylists())
                {
                    if (string.Equals(playlist.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        name = string.Format("New Playlist ({0})", a);
                        success = false;
                        break;
                    }
                }
                if (success)
                {
                    break;
                }
            }
            return new Playlist()
            {
                Name = name,
                Enabled = true
            };
        }

        public static PlaylistsActionsBehaviour Instance { get; private set; }
    }
}
