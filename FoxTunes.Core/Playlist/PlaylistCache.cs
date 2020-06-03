using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    [Component("AF71D00F-5D47-4740-BC14-1B3E4513A1A3", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class PlaylistCache : StandardComponent, IPlaylistCache, IDisposable
    {
        public Playlist[] Playlists { get; private set; }

        public ConcurrentDictionary<Playlist, IndexedArray<PlaylistItem>> Items { get; private set; }

        public ConcurrentDictionary<Playlist, IndexedArray<PlaylistItem>.Index<int>> ItemsById { get; private set; }

        public ConcurrentDictionary<Playlist, IndexedArray<PlaylistItem>.Index<int>> ItemsBySequence { get; private set; }

        public ConcurrentDictionary<Playlist, IndexedArray<PlaylistItem>.Index<int>> ItemsByLibraryId { get; private set; }

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
                case CommonSignals.PlaylistUpdated:
                    var playlists = signal.State as IEnumerable<Playlist>;
                    if (playlists != null)
                    {
                        foreach (var playlist in playlists)
                        {
                            Logger.Write(this, LogLevel.Debug, "Playlist \"{0}\" was updated, resetting cache.", playlist.Name);
                            this.Reset(playlist);
                        }
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Debug, "Playlists were updated, resetting cache.");
                        this.Reset();
                    }
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
                key => new IndexedArray<PlaylistItem>(factory().ToArray())
            ).InnerArray;
        }

        public bool TryGetItemById(int id, out PlaylistItem result)
        {
            foreach (var playlist in this.Playlists)
            {
                var playlistItems = default(IndexedArray<PlaylistItem>);
                if (!this.Items.TryGetValue(playlist, out playlistItems))
                {
                    continue;
                }
                if (this.ItemsById.GetOrAdd(
                    playlist,
                    key => playlistItems.By(playlistItem => playlistItem.Id, IndexedCollection.IndexType.Single)
                ).TryFind(id, out result))
                {
                    return true;
                }
            }
            result = default(PlaylistItem);
            return false;
        }

        public bool TryGetItemBySequence(Playlist playlist, int sequence, out PlaylistItem result)
        {
            var playlistItems = default(IndexedArray<PlaylistItem>);
            if (this.Items.TryGetValue(playlist, out playlistItems))
            {
                if (this.ItemsBySequence.GetOrAdd(
                    playlist,
                    key => playlistItems.By(playlistItem => playlistItem.Sequence, IndexedCollection.IndexType.Single)
                ).TryFind(sequence, out result))
                {
                    return true;
                }
            }
            result = default(PlaylistItem);
            return false;
        }

        public bool TryGetItemsByLibraryId(int id, out PlaylistItem[] result)
        {
            var list = new List<PlaylistItem>();
            foreach (var playlist in this.Playlists)
            {
                var playlistItems = default(IndexedArray<PlaylistItem>);
                if (!this.Items.TryGetValue(playlist, out playlistItems))
                {
                    continue;
                }
                list.AddRange(this.ItemsById.GetOrAdd(
                    playlist,
                    key => playlistItems.By(playlistItem => playlistItem.LibraryItem_Id.GetValueOrDefault(), IndexedCollection.IndexType.Multiple)
                ).FindAll(id));
            }
            if (!list.Any())
            {
                result = default(PlaylistItem[]);
                return false;
            }
            result = list.ToArray();
            return true;
        }

        public void Reset()
        {
            this.Items = new ConcurrentDictionary<Playlist, IndexedArray<PlaylistItem>>();
            this.ItemsById = new ConcurrentDictionary<Playlist, IndexedArray<PlaylistItem>.Index<int>>();
            this.ItemsBySequence = new ConcurrentDictionary<Playlist, IndexedArray<PlaylistItem>.Index<int>>();
            this.ItemsByLibraryId = new ConcurrentDictionary<Playlist, IndexedArray<PlaylistItem>.Index<int>>();
        }

        public void Reset(Playlist playlist)
        {
            this.Playlists = null;
            if (this.Items != null)
            {
                this.Items.TryRemove(playlist);
            }
            if (this.ItemsById != null)
            {
                this.ItemsById.TryRemove(playlist);
            }
            if (this.ItemsBySequence != null)
            {
                this.ItemsBySequence.TryRemove(playlist);
            }
            if (this.ItemsByLibraryId != null)
            {
                this.ItemsByLibraryId.TryRemove(playlist);
            }
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
