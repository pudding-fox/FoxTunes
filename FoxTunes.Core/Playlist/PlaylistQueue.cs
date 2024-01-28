using FoxTunes.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistQueue : StandardComponent, IPlaylistQueue
    {
        public const string QUEUE_LAST = "BBBB";

        public const string QUEUE_NEXT = "BBBC";

        public const string QUEUE_RESET = "BBBD";

        public PlaylistQueue()
        {
            this.Queue = new ConcurrentDictionary<Playlist, List<PlaylistItem>>();
        }

        public ConcurrentDictionary<Playlist, List<PlaylistItem>> Queue { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    var playlists = signal.State as IEnumerable<Playlist>;
                    if (playlists != null && playlists.Any())
                    {
                        foreach (var playlist in playlists)
                        {
                            this.Refresh(playlist);
                        }
                    }
                    else
                    {
                        this.Refresh();
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, QUEUE_LAST, "Add", path: "Queue", attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, QUEUE_NEXT, "Add (Next)", path: "Queue");
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, QUEUE_RESET, "Reset", path: "Queue");
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case QUEUE_LAST:
                    this.Enqueue(PlaylistQueueFlags.None);
                    break;
                case QUEUE_NEXT:
                    this.Enqueue(PlaylistQueueFlags.Next);
                    break;
                case QUEUE_RESET:
                    this.Enqueue(PlaylistQueueFlags.Reset);
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public void Refresh()
        {
            foreach (var pair in this.Queue)
            {
                this.Refresh(pair.Key);
            }
        }

        public void Refresh(Playlist playlist)
        {
            var queue = default(List<PlaylistItem>);
            if (!this.Queue.TryGetValue(playlist, out queue))
            {
                return;
            }
            for (var a = queue.Count - 1; a >= 0; a--)
            {
                var playlistItem = queue[a];
                //TODO: Equality is not implemented.
                if (this.PlaylistBrowser.GetPlaylist(playlistItem) != playlist)
                {
                    queue.RemoveAt(a);
                }
            }
            if (queue.Count == 0)
            {
                this.Queue.TryRemove(playlist);
            }
        }

        public void Enqueue(PlaylistQueueFlags flags)
        {
            var playlistItems = this.PlaylistManager.SelectedItems as IEnumerable<PlaylistItem>;
            if (flags == PlaylistQueueFlags.Next)
            {
                playlistItems = playlistItems.Reverse();
            }
            foreach (var playlistItem in playlistItems)
            {
                var playlist = this.PlaylistBrowser.GetPlaylist(playlistItem);
                if (playlist == null)
                {
                    continue;
                }
                this.Enqueue(playlist, playlistItem, flags);
            }
        }

        public void Enqueue(Playlist playlist, PlaylistItem playlistItem, PlaylistQueueFlags flags)
        {
            var queue = this.Queue.GetOrAdd(playlist, key => new List<PlaylistItem>());
            queue.Remove(playlistItem);
            switch (flags)
            {
                case PlaylistQueueFlags.None:
                    queue.Add(playlistItem);
                    break;
                case PlaylistQueueFlags.Next:
                    queue.Insert(0, playlistItem);
                    break;
            }
        }

        public PlaylistItem Dequeue(Playlist playlist)
        {
            var queue = default(List<PlaylistItem>);
            var playlistItem = default(PlaylistItem);
            if (this.Queue.TryGetValue(playlist, out queue))
            {
                playlistItem = queue.FirstOrDefault();
                if (playlistItem != null)
                {
                    queue.Remove(playlistItem);
                    if (queue.Count == 0)
                    {
                        this.Queue.TryRemove(playlist);
                    }
                }
            }
            return playlistItem;
        }

        public int GetQueuePosition(PlaylistItem playlistItem)
        {
            var position = -1;
            var playlist = this.PlaylistBrowser.GetPlaylist(playlistItem);
            if (playlist != null)
            {
                var queue = default(List<PlaylistItem>);
                if (this.Queue.TryGetValue(playlist, out queue))
                {
                    position = queue.IndexOf(playlistItem);
                }
            }
            return position;
        }
    }
}
