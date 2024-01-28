using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class PlaylistManager : StandardManager, IPlaylistManager
    {
        public const string CLEAR_PLAYLIST = "ZZZZ";

        public PlaylistManager()
        {
            this.SelectedItems = new PlaylistItem[] { };
        }

        private volatile bool IsNavigating = false;

        public ICore Core { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private bool _CanNavigate { get; set; }

        public bool CanNavigate
        {
            get
            {
                return this._CanNavigate;
            }
            set
            {
                this._CanNavigate = value;
                this.OnCanNavigateChanged();
            }
        }

        protected virtual void OnCanNavigateChanged()
        {
            if (this.CanNavigateChanged != null)
            {
                this.CanNavigateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CanNavigate");
        }

        public event EventHandler CanNavigateChanged;

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.DatabaseFactory = core.Factories.Database;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            //TODO: Bad .Wait().
            this.Refresh().Wait();
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task<bool> HasItems()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.ExecuteScalarAsync<bool>(database.QueryFactory.Build().With(query1 =>
                    {
                        query1.Output.AddCase(
                            query1.Output.CreateCaseCondition(
                                query1.Output.CreateFunction(
                                    QueryFunction.Exists,
                                    query1.Output.CreateSubQuery(
                                        database.QueryFactory.Build().With(query2 =>
                                        {
                                            query2.Output.AddOperator(QueryOperator.Star);
                                            query2.Source.AddTable(database.Tables.PlaylistItem);
                                        })
                                    )
                                ),
                                query1.Output.CreateConstant(1)
                            ),
                            query1.Output.CreateCaseCondition(
                                query1.Output.CreateConstant(0)
                            )
                        );
                    }), transaction).ConfigureAwait(false);
                }
            }
        }

        public async Task Refresh()
        {
            Logger.Write(this, LogLevel.Debug, "Refresh was requested, determining whether navigation is possible.");
            this.CanNavigate = this.DatabaseFactory != null && await this.HasItems().ConfigureAwait(false);
            if (this.CanNavigate)
            {
                Logger.Write(this, LogLevel.Debug, "Navigation is possible.");
                if (this.CurrentItem != null)
                {
                    Logger.Write(this, LogLevel.Debug, "Refreshing current item.");
                    using (var database = this.DatabaseFactory.Create())
                    {
                        using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                        {
                            var set = database.Set<PlaylistItem>(transaction);
                            var playlistItem = await set.FindAsync(this.CurrentItem.Id).ConfigureAwait(false);
                            if (playlistItem != null && string.Equals(this.CurrentItem.FileName, playlistItem.FileName, StringComparison.OrdinalIgnoreCase))
                            {
                                this.CurrentItem = playlistItem;
                            }
                            else
                            {
                                this.CurrentItem = null;
                                Logger.Write(this, LogLevel.Warn, "Failed to refresh current item.");
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Navigation is not possible, playlist is empty.");
            }
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            Logger.Write(this, LogLevel.Debug, "Playback manager output stream changed, updating current playlist item.");
            if (this.PlaybackManager.CurrentStream == null)
            {
                this.CurrentItem = null;
                Logger.Write(this, LogLevel.Debug, "Playback manager output stream is empty. Cleared current playlist item.");
            }
            else if (this.PlaybackManager.CurrentStream.PlaylistItem != this.CurrentItem)
            {
                this.CurrentItem = this.PlaybackManager.CurrentStream.PlaylistItem;
                Logger.Write(this, LogLevel.Debug, "Updated current playlist item: {0} => {1}", this.CurrentItem.Id, this.CurrentItem.FileName);
            }
        }

        public async Task Add(IEnumerable<string> paths, bool clear)
        {
            Logger.Write(this, LogLevel.Debug, "Adding paths to playlist.");
            var index = await this.PlaylistBrowser.GetInsertIndex().ConfigureAwait(false);
            await this.Insert(index, paths, clear).ConfigureAwait(false);
        }

        public async Task Insert(int index, IEnumerable<string> paths, bool clear)
        {
            Logger.Write(this, LogLevel.Debug, "Inserting paths into playlist at index: {0}", index);
            using (var task = new AddPathsToPlaylistTask(index, paths, clear))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task Add(LibraryHierarchyNode libraryHierarchyNode, bool clear)
        {
            Logger.Write(this, LogLevel.Debug, "Adding library node to playlist.");
            var index = await this.PlaylistBrowser.GetInsertIndex().ConfigureAwait(false);
            await this.Insert(index, libraryHierarchyNode, clear).ConfigureAwait(false);
        }

        public async Task Insert(int index, LibraryHierarchyNode libraryHierarchyNode, bool clear)
        {
            Logger.Write(this, LogLevel.Debug, "Inserting library node into playlist at index: {0}", index);
            using (var task = new AddLibraryHierarchyNodeToPlaylistTask(index, libraryHierarchyNode, clear))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task Add(IEnumerable<LibraryHierarchyNode> libraryHierarchyNodes, bool clear)
        {
            Logger.Write(this, LogLevel.Debug, "Adding library nodes to playlist.");
            var index = await this.PlaylistBrowser.GetInsertIndex().ConfigureAwait(false);
            await this.Insert(index, libraryHierarchyNodes, clear).ConfigureAwait(false);
        }

        public async Task Insert(int index, IEnumerable<LibraryHierarchyNode> libraryHierarchyNodes, bool clear)
        {
            Logger.Write(this, LogLevel.Debug, "Inserting library nodes into playlist at index: {0}", index);
            using (var task = new AddLibraryHierarchyNodesToPlaylistTask(index, libraryHierarchyNodes, clear))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task Move(IEnumerable<PlaylistItem> playlistItems)
        {
            Logger.Write(this, LogLevel.Debug, "Re-ordering playlist items.");
            var index = await this.PlaylistBrowser.GetInsertIndex().ConfigureAwait(false);
            await this.Move(index, playlistItems).ConfigureAwait(false);
        }

        public async Task Move(int index, IEnumerable<PlaylistItem> playlistItems)
        {
            Logger.Write(this, LogLevel.Debug, "Re-ordering playlist items at index: {0}", index);
            using (var task = new MovePlaylistItemsTask(index, playlistItems))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task Remove(IEnumerable<PlaylistItem> playlistItems)
        {
            using (var task = new RemoveItemsFromPlaylistTask(playlistItems))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task Crop(IEnumerable<PlaylistItem> playlistItems)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var queryable = database.AsQueryable<PlaylistItem>(transaction);
                    var query = queryable.Except(playlistItems);
                    //TODO: Bad .ToArray()
                    using (var task = new RemoveItemsFromPlaylistTask(query.ToArray()))
                    {
                        task.InitializeComponent(this.Core);
                        await this.OnBackgroundTask(task).ConfigureAwait(false);
                        await task.Run().ConfigureAwait(false);
                    }
                }
            }
        }

        public async Task Next()
        {
            if (!this.CanNavigate)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Navigating to next playlist item.");
            if (this.IsNavigating)
            {
                Logger.Write(this, LogLevel.Debug, "Already navigating, ignoring request.");
                return;
            }
            try
            {
                this.IsNavigating = true;
                var playlistItem = await this.PlaylistBrowser.GetNext(true).ConfigureAwait(false);
                if (playlistItem == null)
                {
                    return;
                }
                Logger.Write(this, LogLevel.Debug, "Playing playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                await this.Play(playlistItem).ConfigureAwait(false);
            }
            finally
            {
                this.IsNavigating = false;
            }
        }

        public async Task Previous()
        {
            if (!this.CanNavigate)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Navigating to previous playlist item.");
            if (this.IsNavigating)
            {
                Logger.Write(this, LogLevel.Debug, "Already navigating, ignoring request.");
                return;
            }
            try
            {
                this.IsNavigating = true;
                var playlistItem = await this.PlaylistBrowser.GetPrevious(true).ConfigureAwait(false);
                Logger.Write(this, LogLevel.Debug, "Playing playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                await this.Play(playlistItem).ConfigureAwait(false);
            }
            finally
            {
                this.IsNavigating = false;
            }
        }

        public async Task Play(PlaylistItem playlistItem)
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream != null && outputStream.PlaylistItem == playlistItem)
            {
                outputStream.Position = 0;
                return;
            }
            var exception = default(Exception);
            try
            {
                await this.PlaybackManager.Load(playlistItem, true).ConfigureAwait(false);
                return;
            }
            catch (Exception e)
            {
                exception = e;
            }
            await this.OnError(exception).ConfigureAwait(false);
        }

        public async Task Play(string fileName)
        {
            var playlistItem = await this.PlaylistBrowser.Get(fileName).ConfigureAwait(false);
            if (playlistItem == null)
            {
                return;
            }
            await this.Play(playlistItem).ConfigureAwait(false);
        }

        public async Task Play(int sequence)
        {
            var playlistItem = await this.PlaylistBrowser.Get(sequence).ConfigureAwait(false);
            if (playlistItem == null)
            {
                return;
            }
            await this.Play(playlistItem).ConfigureAwait(false);
        }

        public async Task Clear()
        {
            using (var task = new ClearPlaylistTask())
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        private PlaylistItem _CurrentItem { get; set; }

        public PlaylistItem CurrentItem
        {
            get
            {
                return this._CurrentItem;
            }
            protected set
            {
                this._CurrentItem = value;
                this.OnCurrentItemChanged();
            }
        }

        protected virtual void OnCurrentItemChanged()
        {
            if (this.CurrentItemChanged != null)
            {
                this.CurrentItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CurrentItem");
        }

        public event EventHandler CurrentItemChanged;

        private PlaylistItem[] _SelectedItems { get; set; }

        public PlaylistItem[] SelectedItems
        {
            get
            {
                return this._SelectedItems;
            }
            set
            {
                this._SelectedItems = value;
                this.OnSelectedItemsChanged();
            }
        }

        protected virtual void OnSelectedItemsChanged()
        {
            if (this.SelectedItemsChanged != null)
            {
                this.SelectedItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItems");
        }

        public event EventHandler SelectedItemsChanged;

        public async Task SetRating(IEnumerable<PlaylistItem> playlistItems, byte rating)
        {
            using (var task = new UpdatePlaylistRatingTask(playlistItems, rating))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task IncrementPlayCount(PlaylistItem playlistItem)
        {
            using (var task = new UpdatePlaylistPlayCountTask(playlistItem))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual Task OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new BackgroundTaskEventArgs(backgroundTask);
            this.BackgroundTask(this, e);
            return e.Complete();
        }

        public event BackgroundTaskEventHandler BackgroundTask;

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, CLEAR_PLAYLIST, "Clear", attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case CLEAR_PLAYLIST:
                    return this.Clear();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public void InitializeDatabase(IDatabaseComponent database, DatabaseInitializeType type)
        {
            if (!type.HasFlag(DatabaseInitializeType.Playlist))
            {
                return;
            }
            var scriptingRuntime = ComponentRegistry.Instance.GetComponent<IScriptingRuntime>();
            if (scriptingRuntime == null)
            {
                return;
            }
            using (var transaction = database.BeginTransaction())
            {
                var set = database.Set<PlaylistColumn>(transaction);
                set.Clear();
                set.Add(new PlaylistColumn() { Name = "Artist / album", Type = PlaylistColumnType.Script, Sequence = 1, Script = scriptingRuntime.CoreScripts.Artist_Album, Width = PlaylistColumn.WIDTH_LARGE, Enabled = true });
                set.Add(new PlaylistColumn() { Name = "Track no", Type = PlaylistColumnType.Script, Sequence = 2, Script = scriptingRuntime.CoreScripts.Track, Enabled = true });
                set.Add(new PlaylistColumn() { Name = "Title / track artist", Type = PlaylistColumnType.Script, Sequence = 3, Script = scriptingRuntime.CoreScripts.Title_Performer, Width = PlaylistColumn.WIDTH_LARGE, Enabled = true });
                set.Add(new PlaylistColumn() { Name = "Duration", Type = PlaylistColumnType.Script, Sequence = 4, Script = scriptingRuntime.CoreScripts.Duration, Enabled = true });
                set.Add(new PlaylistColumn() { Name = "Codec", Type = PlaylistColumnType.Script, Sequence = 5, Script = scriptingRuntime.CoreScripts.Codec, Enabled = true });
                set.Add(new PlaylistColumn() { Name = "BPM", Type = PlaylistColumnType.Script, Sequence = 6, Script = scriptingRuntime.CoreScripts.BPM, Enabled = false });
                set.Add(new PlaylistColumn() { Name = "Album gain", Type = PlaylistColumnType.Script, Sequence = 7, Script = scriptingRuntime.CoreScripts.ReplayGainAlbumGain, Enabled = false });
                set.Add(new PlaylistColumn() { Name = "Album peak", Type = PlaylistColumnType.Script, Sequence = 8, Script = scriptingRuntime.CoreScripts.ReplayGainAlbumPeak, Enabled = false });
                set.Add(new PlaylistColumn() { Name = "Track gain", Type = PlaylistColumnType.Script, Sequence = 9, Script = scriptingRuntime.CoreScripts.ReplayGainTrackGain, Enabled = false });
                set.Add(new PlaylistColumn() { Name = "Track peak", Type = PlaylistColumnType.Script, Sequence = 10, Script = scriptingRuntime.CoreScripts.ReplayGainTrackPeak, Enabled = false });
                set.Add(new PlaylistColumn() { Name = "Play count", Type = PlaylistColumnType.Script, Sequence = 11, Script = scriptingRuntime.CoreScripts.PlayCount, Enabled = false });
                set.Add(new PlaylistColumn() { Name = "Last played", Type = PlaylistColumnType.Script, Sequence = 12, Script = scriptingRuntime.CoreScripts.LastPlayed, Enabled = false });
                transaction.Commit();
            }
        }
    }
}
