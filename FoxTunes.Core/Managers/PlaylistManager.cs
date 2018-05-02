using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes.Managers
{
    public class PlaylistManager : StandardManager, IPlaylistManager
    {
        public const string CLEAR_PLAYLIST = "F452E482-2DF8-42F3-8D7D-4B3C7F40A708";

        private volatile bool IsNavigating = false;

        public ICore Core { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Components.Database;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.PlaybackManager_CurrentStreamChanged;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    return this.Refresh();
            }
            return Task.CompletedTask;
        }

        public Task Refresh()
        {
            Logger.Write(this, LogLevel.Debug, "Refresh was requested, determining whether navigation is possible.");
            this.CanNavigate = this.Database.ExecuteScalar<bool>(this.Database.QueryFactory.Build().With(query1 =>
            {
                query1.Output.AddFunction(QueryFunction.Exists, query1.Output.CreateSubQuery(this.Database.QueryFactory.Build().With(query2 =>
                {
                    query2.Output.AddOperator(QueryOperator.Star);
                    query2.Source.AddTable(this.Database.Tables.PlaylistItem);
                })));
            }));
            if (this.CanNavigate)
            {
                Logger.Write(this, LogLevel.Debug, "Navigation is possible.");
                if (this.CurrentItem != null)
                {
                    Logger.Write(this, LogLevel.Debug, "Refreshing current item.");
                    if ((this.CurrentItem = this.Database.Sets.PlaylistItem.Find(this.CurrentItem.Id)) == null)
                    {
                        Logger.Write(this, LogLevel.Warn, "Failed to refresh current item.");
                    }
                }
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Navigation is not possible, playlist is empty.");
            }
            return Task.CompletedTask;
        }

        protected virtual void PlaybackManager_CurrentStreamChanged(object sender, EventArgs e)
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

        public Task Add(IEnumerable<string> paths)
        {
            Logger.Write(this, LogLevel.Debug, "Adding paths to playlist.");
            var index = this.GetInsertIndex();
            return this.Insert(index, paths);
        }

        public Task Insert(int index, IEnumerable<string> paths)
        {
            Logger.Write(this, LogLevel.Debug, "Inserting paths into playlist at index: {0}", index);
            var task = new AddPathsToPlaylistTask(index, paths);
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run();
        }

        public Task Add(LibraryHierarchyNode libraryHierarchyNode)
        {
            Logger.Write(this, LogLevel.Debug, "Adding library node to playlist.");
            var index = this.GetInsertIndex();
            return this.Insert(index, libraryHierarchyNode);
        }

        public Task Insert(int index, LibraryHierarchyNode libraryHierarchyNode)
        {
            Logger.Write(this, LogLevel.Debug, "Inserting library node into playlist at index: {0}", index);
            var task = new AddLibraryHierarchyNodeToPlaylistTask(index, libraryHierarchyNode);
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run();
        }

        public int GetInsertIndex()
        {
            var playlistItem = this.GetLastPlaylistItem();
            if (playlistItem == null)
            {
                return 0;
            }
            else
            {
                return playlistItem.Sequence + 1;
            }
        }

        public bool CanNavigate { get; private set; }

        public PlaylistItem GetNext()
        {
            var playlistItem = default(PlaylistItem);
            if (this.CurrentItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming first item.");
                playlistItem = this.GetFirstPlaylistItem();
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is sequence: {0}", this.CurrentItem.Sequence);
                playlistItem = this.GetNextPlaylistItem(this.CurrentItem.Sequence);
                if (playlistItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Sequence was too large, wrapping around to first item.");
                    playlistItem = this.GetFirstPlaylistItem();
                }
            }
            if (playlistItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Playlist was empty.");
            }
            return playlistItem;
        }

        public async Task Next()
        {
            Logger.Write(this, LogLevel.Debug, "Navigating to next playlist item.");
            if (this.IsNavigating)
            {
                Logger.Write(this, LogLevel.Debug, "Already navigating, ignoring request.");
                return;
            }
            try
            {
                this.IsNavigating = true;
                var playlistItem = this.GetNext();
                if (playlistItem == null)
                {
                    return;
                }
                Logger.Write(this, LogLevel.Debug, "Playing playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                await this.Play(playlistItem);
            }
            finally
            {
                this.IsNavigating = false;
            }
        }

        public PlaylistItem GetPrevious()
        {
            var playlistItem = default(PlaylistItem);
            if (this.CurrentItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming last item.");
                playlistItem = this.GetLastPlaylistItem();
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Previous playlist item is sequence: {0}", this.CurrentItem.Sequence);
                playlistItem = this.GetPreviousPlaylistItem(this.CurrentItem.Sequence);
                if (playlistItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Sequence was too small, wrapping around to last item.");
                    playlistItem = this.GetLastPlaylistItem();
                }
            }
            if (playlistItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Playlist was empty.");
            }
            return playlistItem;
        }

        public async Task Previous()
        {
            Logger.Write(this, LogLevel.Debug, "Navigating to previous playlist item.");
            if (this.IsNavigating)
            {
                Logger.Write(this, LogLevel.Debug, "Already navigating, ignoring request.");
                return;
            }
            try
            {
                this.IsNavigating = true;
                var playlistItem = this.GetPrevious();
                Logger.Write(this, LogLevel.Debug, "Playing playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                await this.Play(playlistItem);
            }
            finally
            {
                this.IsNavigating = false;
            }
        }

        protected virtual PlaylistItem GetFirstPlaylistItem()
        {
            return this.Database.AsQueryable<PlaylistItem>()
                .OrderBy(playlistItem => playlistItem.Sequence)
                .FirstOrDefault();
        }

        protected virtual PlaylistItem GetLastPlaylistItem()
        {
            return this.Database.AsQueryable<PlaylistItem>()
                .OrderByDescending(playlistItem => playlistItem.Sequence)
                .FirstOrDefault();
        }

        protected virtual PlaylistItem GetNextPlaylistItem(int sequence)
        {
            return this.Database.AsQueryable<PlaylistItem>()
                .Where(playlistItem => playlistItem.Sequence > sequence)
                .OrderBy(playlistItem => playlistItem.Sequence)
                .FirstOrDefault();
        }

        protected virtual PlaylistItem GetPreviousPlaylistItem(int sequence)
        {
            return this.Database.AsQueryable<PlaylistItem>()
                .Where(playlistItem => playlistItem.Sequence < sequence)
                .OrderByDescending(playlistItem => playlistItem.Sequence)
                .FirstOrDefault();
        }

        public async Task Play(PlaylistItem playlistItem)
        {
            await this.PlaybackManager.Load(playlistItem, true)
                .ContinueWith(_ =>
                {
                    if (this.CurrentItem == null)
                    {
                        Logger.Write(this, LogLevel.Warn, "Expected current output stream to be {0} => {1} but it empty.", playlistItem.Id, playlistItem.FileName);
                        return;
                    }
                    if (this.CurrentItem != playlistItem)
                    {
                        Logger.Write(this, LogLevel.Warn, "Expected current output stream to be {0} => {1} but it was {2} => {3}", playlistItem.Id, playlistItem.FileName, this.CurrentItem.Id, this.CurrentItem.FileName);
                        return;
                    }
                    if (this.PlaybackManager.CurrentStream.PlaylistItem != playlistItem)
                    {
                        Logger.Write(this, LogLevel.Warn, "Expected current output stream to be {0} => {1} but it was {2} => {3}", playlistItem.Id, playlistItem.FileName, this.PlaybackManager.CurrentStream.PlaylistItem.Id, this.PlaybackManager.CurrentStream.PlaylistItem.FileName);
                        return;
                    }
                    Logger.Write(this, LogLevel.Debug, "Playing current output stream: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                    try
                    {
                        this.PlaybackManager.CurrentStream.Play();
                    }
                    catch (Exception e)
                    {
                        this.OnError(e);
                    }
                });
        }

        public Task Clear()
        {
            var task = new ClearPlaylistTask();
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run();
        }

        private PlaylistItem _CurrentItem { get; set; }

        public PlaylistItem CurrentItem
        {
            get
            {
                return this._CurrentItem;
            }
            private set
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

        public event EventHandler CurrentItemChanged = delegate { };

        protected virtual void OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
                return;
            }
            this.BackgroundTask(this, new BackgroundTaskEventArgs(backgroundTask));
        }

        public event BackgroundTaskEventHandler BackgroundTask = delegate { };

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, CLEAR_PLAYLIST, "Clear");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case CLEAR_PLAYLIST:
                    return this.Clear();
            }
            return Task.CompletedTask;
        }
    }
}
