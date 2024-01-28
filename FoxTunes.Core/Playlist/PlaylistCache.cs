using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class PlaylistCache : StandardComponent, IPlaylistCache, IDisposable
    {
        public Playlist[] Playlists { get; private set; }

        public ConcurrentDictionary<Playlist, PlaylistItem[]> Items { get; private set; }

        public ConcurrentDictionary<Playlist, IDictionary<int, PlaylistItem>> ItemsById { get; private set; }

        public ConcurrentDictionary<Playlist, IDictionary<int, PlaylistItem[]>> ItemsByLibraryId { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public PlaylistCache()
        {
            this.Reset();
        }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistsUpdated:
                    Logger.Write(this, LogLevel.Debug, "Playlists were updated, resetting cache.");
                    this.Reset();
                    break;
                case CommonSignals.PlaylistUpdated:
                    Logger.Write(this, LogLevel.Debug, "Playlist was updated, resetting cache.");
                    this.Reset();
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Playlist[] GetPlaylists(Func<IEnumerable<Playlist>> factory)
        {
            if (this.Playlists == null)
            {
                this.Playlists = factory().ToArray();
            }
            return this.Playlists;
        }

        public PlaylistItem[] GetItems(Playlist playlist, Func<IEnumerable<PlaylistItem>> factory)
        {
            return this.Items.GetOrAdd(
                playlist,
                key => factory().ToArray()
            );
        }

        public bool TryGetItemById(int id, out PlaylistItem result)
        {
            if (this.Playlists != null)
            {
                foreach (var playlist in this.Playlists)
                {
                    var sequence = default(IDictionary<int, PlaylistItem>);
                    if (!this.ItemsById.TryGetValue(playlist, out sequence))
                    {
                        var playlistItems = default(PlaylistItem[]);
                        if (!this.Items.TryGetValue(playlist, out playlistItems))
                        {
                            continue;
                        }
                        sequence = this.ItemsById.GetOrAdd(
                            playlist,
                            key => playlistItems.ToDictionary(playlistItem => playlistItem.Id)
                        );
                    }
                    if (sequence.TryGetValue(id, out result))
                    {
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        public bool TryGetItemsByLibraryId(int id, out PlaylistItem[] result)
        {
            if (this.Playlists != null)
            {
                foreach (var playlist in this.Playlists)
                {
                    var sequence = default(IDictionary<int, PlaylistItem[]>);
                    if (!this.ItemsByLibraryId.TryGetValue(playlist, out sequence))
                    {
                        var playlistItems = default(PlaylistItem[]);
                        if (!this.Items.TryGetValue(playlist, out playlistItems))
                        {
                            continue;
                        }
                        sequence = this.ItemsByLibraryId.GetOrAdd(
                            playlist,
                            key => this.GetItemsByLibraryId(playlistItems)
                        );
                    }
                    if (sequence.TryGetValue(id, out result))
                    {
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        protected virtual IDictionary<int, PlaylistItem[]> GetItemsByLibraryId(PlaylistItem[] playlistItems)
        {
            var query =
                from playlistItem in playlistItems
                where playlistItem.LibraryItem_Id.HasValue
                group playlistItem by playlistItem.LibraryItem_Id.Value into grouping
                select grouping;
            return query.ToDictionary(group => group.Key, group => group.ToArray());
        }

        public void Reset()
        {
            this.Playlists = null;
            this.Items = new ConcurrentDictionary<Playlist, PlaylistItem[]>();
            this.ItemsById = new ConcurrentDictionary<Playlist, IDictionary<int, PlaylistItem>>();
            this.ItemsByLibraryId = new ConcurrentDictionary<Playlist, IDictionary<int, PlaylistItem[]>>();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
        }

        ~PlaylistCache()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
