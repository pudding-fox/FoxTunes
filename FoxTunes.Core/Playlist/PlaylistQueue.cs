using FoxTunes.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistQueue : StandardComponent
    {
        public PlaylistQueue()
        {
            this.Queue = new ConcurrentDictionary<Playlist, List<PlaylistItem>>();
        }

        public ConcurrentDictionary<Playlist, List<PlaylistItem>> Queue { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
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

        public Task Enqueue(Playlist playlist, PlaylistItem playlistItem, PlaylistQueueFlags flags)
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
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task<PlaylistItem> Dequeue(Playlist playlist)
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
#if NET40
            return TaskEx.FromResult(playlistItem);
#else
            return Task.FromResult(playlistItem);
#endif
        }

        public Task<int> GetQueuePosition(Playlist playlist, PlaylistItem playlistItem)
        {
            var position = -1;
            var queue = default(List<PlaylistItem>);
            if (this.Queue.TryGetValue(playlist, out queue))
            {
                position = queue.IndexOf(playlistItem);
            }
#if NET40
            return TaskEx.FromResult(position);
#else
            return Task.FromResult(position);
#endif
        }
    }
}
