using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibrarySelectionPlaylistBehaviour : StandardBehaviour
    {
        public LibrarySelectionPlaylistBehaviour()
        {
            this.Playlists = new List<Playlist>();
        }

        public IList<Playlist> Playlists { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryManager = core.Managers.Library;
            this.LibraryManager.SelectedItemChanged += this.OnSelectedItemChanged;
            this.PlaylistManager = core.Managers.Playlist;
            this.DatabaseFactory = core.Factories.Database;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual void OnSelectedItemChanged(object sender, EventArgs e)
        {
            this.Dispatch(() =>
            {
                foreach (var playlist in this.Playlists)
                {
                    var task = this.Refresh(playlist);
                }
            });
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    this.Dispatch(this.Refresh);
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void Refresh()
        {
            var playlists = this.PlaylistBrowser.GetPlaylists().Where(
                playlist => playlist.Type == PlaylistType.Selection && playlist.Enabled
            ).ToArray();
            for (var a = this.Playlists.Count - 1; a >= 0; a--)
            {
                if (!playlists.Contains(this.Playlists[a]))
                {
                    this.Playlists.RemoveAt(a);
                }
            }
            foreach (var playlist in playlists)
            {
                if (!this.Playlists.Contains(playlist))
                {
                    this.Playlists.Add(playlist);
                    var task = this.Refresh(playlist);
                }
            }
        }

        protected virtual async Task Refresh(Playlist playlist)
        {
            var libraryHierarchyNode = this.LibraryManager.SelectedItem;
            if (libraryHierarchyNode != null && !LibraryHierarchyNode.Empty.Equals(libraryHierarchyNode))
            {
                playlist.Name = libraryHierarchyNode.Value;
                await this.Update(playlist).ConfigureAwait(false);
                await this.PlaylistManager.Add(playlist, libraryHierarchyNode, true).ConfigureAwait(false);
            }
        }

        protected virtual async Task Update(Playlist playlist)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var task = new SingletonReentrantTask(CancellationToken.None, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var set = database.Set<Playlist>(transaction);
                        await set.AddOrUpdateAsync(playlist).ConfigureAwait(false);
                        transaction.Commit();
                    }
                }))
                {
                    await task.Run().ConfigureAwait(false);
                }
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
            if (this.LibraryManager != null)
            {
                this.LibraryManager.SelectedItemChanged -= this.OnSelectedItemChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
        }

        ~LibrarySelectionPlaylistBehaviour()
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
