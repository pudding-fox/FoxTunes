using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class PlaylistCache : StandardComponent, IPlaylistCache, IDisposable
    {
        public Lazy<IList<PlaylistItem>> Items { get; private set; }

        public Lazy<IDictionary<int, PlaylistItem>> ItemsById { get; private set; }

        public Lazy<IDictionary<int, IList<PlaylistItem>>> ItemsByLibraryId { get; private set; }

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

        public bool TryGetItemById(int id, out PlaylistItem playlistItem)
        {
            if (this.Items == null || !this.Items.IsValueCreated)
            {
                playlistItem = null;
                return false;
            }
            if (this.ItemsById == null)
            {
                this.ItemsById = new Lazy<IDictionary<int, PlaylistItem>>(() => this.Items.Value.ToDictionary(item => item.Id));
            }
            return this.ItemsById.Value.TryGetValue(id, out playlistItem);
        }

        public bool TryGetItemsByLibraryId(int id, out IEnumerable<PlaylistItem> playlistItems)
        {
            if (this.Items == null || !this.Items.IsValueCreated)
            {
                playlistItems = null;
                return false;
            }
            var value = default(IList<PlaylistItem>);
            if (this.ItemsByLibraryId == null)
            {
                this.ItemsByLibraryId = new Lazy<IDictionary<int, IList<PlaylistItem>>>(() =>
                {
                    var result = new Dictionary<int, IList<PlaylistItem>>();
                    foreach (var playlistItem in this.Items.Value)
                    {
                        if (!playlistItem.LibraryItem_Id.HasValue)
                        {
                            continue;
                        }
                        result.GetOrAdd(
                            playlistItem.LibraryItem_Id.Value,
                            key => new List<PlaylistItem>()
                        ).Add(playlistItem);
                    }
                    return result;
                });
            }
            if (this.ItemsByLibraryId.Value.TryGetValue(id, out value))
            {
                playlistItems = value;
                return true;
            }
            playlistItems = null;
            return false;
        }

        public IEnumerable<PlaylistItem> GetItems(Func<IEnumerable<PlaylistItem>> factory)
        {
            if (this.Items == null)
            {
                this.Items = new Lazy<IList<PlaylistItem>>(() => new List<PlaylistItem>(factory()));
            }
            return this.Items.Value;
        }

        public void Reset()
        {
            this.Items = null;
            this.ItemsById = null;
            this.ItemsByLibraryId = null;
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
