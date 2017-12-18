using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes.Managers
{
    public class PlaylistManager : StandardManager, IPlaylistManager
    {
        private volatile bool IsNavigating = false;

        public ICore Core { get; private set; }

        public IDatabase Database { get; private set; }

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
            base.InitializeComponent(core);
        }

        protected virtual void OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    this.Refresh();
                    break;
            }
        }

        public void Refresh()
        {
            if (this.CurrentItem == null)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Playlist was updated, refreshing current item.");
            if ((this.CurrentItem = this.Database.Sets.PlaylistItem.Find(this.CurrentItem.Id)) == null)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to refresh current item.");
            }
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
            return task.Run().ContinueWith(_ => this.OnUpdated());
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
            return task.Run().ContinueWith(_ => this.OnUpdated());
        }

        private int GetInsertIndex()
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

        protected virtual void OnUpdated()
        {
            Logger.Write(this, LogLevel.Debug, "Playlist was updated.");
            if (this.Updated == null)
            {
                return;
            }
            this.Updated(this, EventArgs.Empty);
        }

        public event EventHandler Updated = delegate { };

        public bool CanNavigate
        {
            get
            {
                return this.Database.Sets.PlaylistItem.Count > 0;
            }
        }

        public PlaylistItem GetNext()
        {
            var sequence = default(int);
            if (this.CurrentItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming first item.");
                sequence = 0;
            }
            else
            {
                sequence = this.CurrentItem.Sequence;
                Logger.Write(this, LogLevel.Debug, "Current playlist item is sequence: {0}", sequence);
            }
            var playlistItem = this.GetNextPlaylistItem(sequence);
            if (playlistItem == null)
            {
                playlistItem = this.GetFirstPlaylistItem();
                if (playlistItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Playlist was empty.");
                    return default(PlaylistItem);
                }
                Logger.Write(this, LogLevel.Debug, "Sequence was too large, wrapping around to first item.");
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
                this.IsNavigating = true; ;
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
            var sequence = default(int);
            if (this.CurrentItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming first item.");
                sequence = 0;
            }
            else
            {
                sequence = this.CurrentItem.Sequence;
                Logger.Write(this, LogLevel.Debug, "Previous playlist item is sequence: {0}", sequence);
            }
            var playlistItem = this.GetPreviousPlaylistItem(sequence);
            if (playlistItem == null)
            {
                playlistItem = this.GetLastPlaylistItem();
                if (playlistItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Playlist was empty.");
                    return default(PlaylistItem);
                }
                Logger.Write(this, LogLevel.Debug, "Sequence was too small, wrapping around to last item.");
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
            return this.Database.Sets.PlaylistItem.Query(this.Database.Queries.GetFirstPlaylistItem).FirstOrDefault();
        }

        protected virtual PlaylistItem GetLastPlaylistItem()
        {
            return this.Database.Sets.PlaylistItem.Query(this.Database.Queries.GetLastPlaylistItem).FirstOrDefault();
        }

        protected virtual PlaylistItem GetNextPlaylistItem(int sequence)
        {
            return this.Database.Sets.PlaylistItem.Query(
                this.Database.Queries.GetNextPlaylistItem,
                parameters => parameters["sequence"] = sequence
            ).FirstOrDefault();
        }

        protected virtual PlaylistItem GetPreviousPlaylistItem(int sequence)
        {
            return this.Database.Sets.PlaylistItem.Query(
                this.Database.Queries.GetPreviousPlaylistItem,
                parameters => parameters["sequence"] = sequence
            ).FirstOrDefault();
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
                    this.PlaybackManager.CurrentStream.Play();
                });
        }

        public Task Clear()
        {
            var task = new ClearPlaylistTask();
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run().ContinueWith(_ => this.OnUpdated());
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
    }
}
