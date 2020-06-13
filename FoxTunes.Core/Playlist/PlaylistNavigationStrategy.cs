using FoxDb;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class PlaylistNavigationStrategy : BaseComponent
    {
        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        protected virtual Playlist GetPlaylist(PlaylistItem playlistItem)
        {
            if (playlistItem == null)
            {
                return this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist;
            }
            return this.PlaylistBrowser.GetPlaylist(playlistItem) ?? this.PlaylistManager.SelectedPlaylist;
        }

        public abstract PlaylistItem GetNext(PlaylistItem playlistItem);

        public abstract PlaylistItem GetPrevious(PlaylistItem playlistItem);

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.PlaylistManager = core.Managers.Playlist;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }
    }

    public class StandardPlaylistNavigationStrategy : PlaylistNavigationStrategy
    {
        public override PlaylistItem GetNext(PlaylistItem playlistItem)
        {
            var playlist = this.GetPlaylist(playlistItem);
            if (playlist == null)
            {
                return null;
            }
            if (playlistItem == null)
            {
                return this.PlaylistBrowser.GetFirstItem(playlist);
            }
            playlistItem = this.PlaylistBrowser.GetItemBySequence(playlist, playlistItem.Sequence + 1);
            if (playlistItem == null)
            {
                playlistItem = this.PlaylistBrowser.GetFirstItem(playlist);
            }
            return playlistItem;
        }

        public override PlaylistItem GetPrevious(PlaylistItem playlistItem)
        {
            var playlist = this.GetPlaylist(playlistItem);
            if (playlist == null)
            {
                return null;
            }
            if (playlistItem == null)
            {
                return this.PlaylistBrowser.GetLastItem(playlist);
            }
            playlistItem = this.PlaylistBrowser.GetItemBySequence(playlist, playlistItem.Sequence - 1);
            if (playlistItem == null)
            {
                return this.PlaylistBrowser.GetLastItem(playlist);
            }
            return playlistItem;
        }
    }

    public class ShufflePlaylistNavigationStrategy : PlaylistNavigationStrategy
    {
        public ShufflePlaylistNavigationStrategy()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Sequences = new List<int>();
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public IList<int> Sequences { get; private set; }

        public Playlist Playlist { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    if (this.Playlist != null)
                    {
                        var playlists = signal.State as IEnumerable<Playlist>;
                        if (playlists == null || !playlists.Any() || playlists.Contains(this.Playlist))
                        {
                            return this.Refresh();
                        }
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Refresh()
        {
            if (this.Playlist != null)
            {
                this.Refresh(this.Playlist);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void Refresh(Playlist playlist)
        {
            this.Playlist = playlist;
            this.Sequences.Clear();
            this.Sequences.AddRange(
                this.PlaylistBrowser.GetItems(playlist).Select(
                    playlistItem => playlistItem.Sequence
                )
            );
            this.Sequences.Shuffle();
        }

        public override PlaylistItem GetNext(PlaylistItem playlistItem)
        {
            var playlist = this.GetPlaylist(playlistItem);
            if (playlist == null)
            {
                return null;
            }
            if (this.Playlist == null || this.Playlist != playlist)
            {
                this.Refresh(playlist);
            }
            if (this.Sequences.Count == 0)
            {
                return default(PlaylistItem);
            }
            var position = default(int);
            if (playlistItem != null)
            {
                position = this.Sequences.IndexOf(playlistItem.Sequence);
                if (position >= this.Sequences.Count - 1)
                {
                    position = 0;
                }
                else
                {
                    position++;
                }
            }
            else
            {
                position = 0;
            }
            var sequence = this.Sequences[position];
            return this.PlaylistBrowser.GetItemBySequence(playlist, sequence);
        }

        public override PlaylistItem GetPrevious(PlaylistItem playlistItem)
        {
            var playlist = this.GetPlaylist(playlistItem);
            if (playlist == null)
            {
                return null;
            }
            if (this.Playlist == null || this.Playlist != playlist)
            {
                this.Refresh(playlist);
            }
            if (this.Sequences.Count == 0)
            {
                return default(PlaylistItem);
            }
            var position = default(int);
            if (playlistItem != null)
            {
                position = this.Sequences.IndexOf(playlistItem.Sequence);
                if (position == 0)
                {
                    position = this.Sequences.Count - 1;
                }
                else
                {
                    position--;
                }
            }
            else
            {
                position = this.Sequences.Count - 1;
            }
            var sequence = this.Sequences[position];
            return this.PlaylistBrowser.GetItemBySequence(playlist, sequence);
        }
    }
}
