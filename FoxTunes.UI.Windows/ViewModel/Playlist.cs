using FoxDb;
using FoxTunes.Integration;
using FoxTunes.Interfaces;
using FoxTunes.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playlist : ViewModelBase
    {
        public Playlist()
        {
            this.SelectedItems = new ObservableCollection<PlaylistItem>();
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public PlaylistGridViewColumnFactory GridViewColumnFactory { get; private set; }

        public IEnumerable Items
        {
            get
            {
                return new ObservableCollection<PlaylistItem>(this.GetItems());
            }
        }

        protected virtual IEnumerable<PlaylistItem> GetItems()
        {
            if (this.DatabaseFactory != null)
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var set = database.Set<PlaylistItem>(transaction);
                        set.Fetch.Sort.Expressions.Clear();
                        set.Fetch.Sort.AddColumn(set.Table.GetColumn(ColumnConfig.By("Sequence", ColumnFlags.None)));
                        foreach (var element in set)
                        {
                            yield return element;
                        }
                    }
                }
            }
        }

        protected virtual void OnItemsChanged()
        {
            if (this.ItemsChanged != null)
            {
                this.ItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Items");
        }

        public event EventHandler ItemsChanged = delegate { };

        public IList SelectedItems { get; set; }

        private bool _AutoSizeGridColumns { get; set; }

        public bool AutoSizeGridColumns
        {
            get
            {
                return this._AutoSizeGridColumns;
            }
            set
            {
                this._AutoSizeGridColumns = value;
                this.OnAutoSizeGridColumnsChanged();
            }
        }

        protected virtual void OnAutoSizeGridColumnsChanged()
        {
            if (this.AutoSizeGridColumnsChanged != null)
            {
                this.AutoSizeGridColumnsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AutoSizeGridColumns");
        }

        public event EventHandler AutoSizeGridColumnsChanged = delegate { };

        private bool _InsertActive { get; set; }

        public bool InsertActive
        {
            get
            {
                return this._InsertActive;
            }
            set
            {
                this._InsertActive = value;
                this.OnInsertActiveChanged();
            }
        }

        protected virtual void OnInsertActiveChanged()
        {
            if (this.InsertActiveChanged != null)
            {
                this.InsertActiveChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("InsertActive");
        }

        public event EventHandler InsertActiveChanged = delegate { };

        private int _InsertIndex { get; set; }

        public int InsertIndex
        {
            get
            {
                return this._InsertIndex;
            }
            set
            {
                this._InsertIndex = value;
                this.OnInsertIndexChanged();
            }
        }

        protected virtual void OnInsertIndexChanged()
        {
            if (this.InsertIndexChanged != null)
            {
                this.InsertIndexChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("InsertIndex");
        }

        public event EventHandler InsertIndexChanged = delegate { };

        private int _InsertOffset { get; set; }

        public int InsertOffset
        {
            get
            {
                return this._InsertOffset;
            }
            set
            {
                this._InsertOffset = value;
                this.OnInsertOffsetChanged();
            }
        }

        protected virtual void OnInsertOffsetChanged()
        {
            if (this.InsertOffsetChanged != null)
            {
                this.InsertOffsetChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("InsertOffset");
        }

        public event EventHandler InsertOffsetChanged = delegate { };

        private ObservableCollection<GridViewColumn> _GridColumns { get; set; }

        public ObservableCollection<GridViewColumn> GridColumns
        {
            get
            {
                return this._GridColumns;
            }
            set
            {
                this._GridColumns = value;
                this.OnGridColumnsChanged();
            }
        }

        protected virtual void OnGridColumnsChanged()
        {
            if (this.GridColumnsChanged != null)
            {
                this.GridColumnsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("GridColumns");
        }

        public event EventHandler GridColumnsChanged = delegate { };

        protected override void OnCoreChanged()
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            this.ForegroundTaskRunner = this.Core.Components.ForegroundTaskRunner;
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            //TODO: This is a hack in order to make the playlist's "is playing" field update.
            this.PlaybackManager.CurrentStreamChanged += (sender, e) => this.RefreshColumns();
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.GridViewColumnFactory = new PlaylistGridViewColumnFactory(this.PlaybackManager, this.ScriptingRuntime);
            this.RefreshColumns();
            this.ReloadItems();
            this.OnCommandsChanged();
            base.OnCoreChanged();
        }

        protected virtual void OnCommandsChanged()
        {
            this.OnPropertyChanged("RemovePlaylistItemsCommand");
            this.OnPropertyChanged("PlaySelectedItemCommand");
            this.OnPropertyChanged("DragEnterCommand");
            this.OnPropertyChanged("DropCommand");
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    return this.ForegroundTaskRunner.Run(() => this.ReloadItems());
                case CommonSignals.PlaylistColumnsUpdated:
                    return this.ForegroundTaskRunner.Run(() => this.ReloadColumns());
                case CommonSignals.PluginInvocation:
                    var invocation = signal.State as IInvocationComponent;
                    if (invocation != null)
                    {
                        switch (invocation.Category)
                        {
                            case InvocationComponent.CATEGORY_PLAYLIST:
                                switch (invocation.Id)
                                {
                                    case PlaylistActionsBehaviour.REMOVE_PLAYLIST_ITEMS:
                                        return this.RemovePlaylistItems();
                                    case PlaylistActionsBehaviour.CROP_PLAYLIST_ITEMS:
                                        return this.CropPlaylistItems();
                                    case PlaylistActionsBehaviour.LOCATE_PLAYLIST_ITEMS:
                                        return this.LocatePlaylistItems();
                                }
                                break;
                        }
                    }
                    break;
            }
            return Task.CompletedTask;
        }

        public ICommand RemovePlaylistItemsCommand
        {
            get
            {
                return new AsyncCommand(this.BackgroundTaskRunner, this.RemovePlaylistItems);
            }
        }

        protected virtual Task RemovePlaylistItems()
        {
            return this.PlaylistManager.Remove(this.SelectedItems.OfType<PlaylistItem>());
        }

        protected virtual Task CropPlaylistItems()
        {
            return this.PlaylistManager.Crop(this.SelectedItems.OfType<PlaylistItem>());
        }

        protected virtual Task LocatePlaylistItems()
        {
            foreach (var item in this.SelectedItems.OfType<PlaylistItem>())
            {
                Explorer.Select(item.FileName);
            }
            return Task.CompletedTask;
        }

        public ICommand PlaySelectedItemCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () =>
                    {
                        var playlistItem = this.SelectedItems[0] as PlaylistItem;
                        return this.PlaylistManager.Play(playlistItem);
                    },
                    () => this.PlaybackManager != null && this.SelectedItems.Count > 0
                );
            }
        }

        public ICommand DragEnterCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragEnter);
            }
        }

        protected virtual void OnDragEnter(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    effects = DragDropEffects.Copy;
                }
                if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
                {
                    effects = DragDropEffects.Copy;
                }
                if (e.Data.GetDataPresent<IEnumerable<PlaylistItem>>(true))
                {
                    effects = DragDropEffects.Copy;
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to query clipboard contents: {0}", exception.Message);
            }
            e.Effects = effects;
        }

        public ICommand DropCommand
        {
            get
            {
                return new AsyncCommand<DragEventArgs>(this.BackgroundTaskRunner, this.OnDrop);
            }
        }

        protected virtual Task OnDrop(DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                    return this.AddToPlaylist(paths);
                }
                if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
                {
                    var libraryHierarchyNode = e.Data.GetData(typeof(LibraryHierarchyNode)) as LibraryHierarchyNode;
                    return this.AddToPlaylist(libraryHierarchyNode);
                }
                if (e.Data.GetDataPresent<IEnumerable<PlaylistItem>>(true))
                {
                    var playlistItems = e.Data.GetData<IEnumerable<PlaylistItem>>(true);
                    return this.AddToPlaylist(playlistItems);
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to process clipboard contents: {0}", exception.Message);
            }
            return Task.CompletedTask;
        }

        private Task AddToPlaylist(IEnumerable<string> paths)
        {
            var sequence = default(int);
            if (this.TryGetInsertSequence(out sequence))
            {
                return this.PlaylistManager.Insert(sequence, paths, false);
            }
            else
            {
                return this.PlaylistManager.Add(paths, false);
            }
        }

        private Task AddToPlaylist(LibraryHierarchyNode libraryHierarchyNode)
        {
            var sequence = default(int);
            if (this.TryGetInsertSequence(out sequence))
            {
                return this.PlaylistManager.Insert(sequence, libraryHierarchyNode, false);
            }
            else
            {
                return this.PlaylistManager.Add(libraryHierarchyNode, false);
            }
        }

        private Task AddToPlaylist(IEnumerable<PlaylistItem> playlistItems)
        {
            var sequence = default(int);
            if (this.TryGetInsertSequence(out sequence))
            {
                return this.PlaylistManager.Move(sequence, playlistItems);
            }
            else
            {
                return this.PlaylistManager.Move(playlistItems);
            }
        }

        protected virtual bool TryGetInsertSequence(out int sequence)
        {
            if (!this.InsertActive)
            {
                sequence = 0;
                return false;
            }
            sequence = this.InsertIndex + this.InsertOffset;
            return true;
        }

        protected virtual IEnumerable<GridViewColumn> GetGridColumns()
        {
            if (this.DatabaseFactory != null && this.GridViewColumnFactory != null)
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var queryable = database.AsQueryable<PlaylistColumn>(transaction);
                        foreach (var column in queryable.OrderBy(playlistColumn => playlistColumn.Sequence))
                        {
                            yield return this.GridViewColumnFactory.Create(column);
                        }
                    }
                }
            }
        }

        protected virtual void RefreshColumns()
        {
            if (this.GridColumns == null)
            {
                this.ReloadColumns();
            }
            if (this.GridColumns != null)
            {
                foreach (var column in this.GridColumns)
                {
                    this.GridViewColumnFactory.Refresh(column);
                    if (this.AutoSizeGridColumns)
                    {
                        if (double.IsNaN(column.Width))
                        {
                            column.Width = column.ActualWidth;
                            column.Width = double.NaN;
                        }
                    }
                }
            }
        }

        protected virtual void ReloadColumns()
        {
            this.GridColumns = new ObservableCollection<GridViewColumn>(this.GetGridColumns());
        }

        protected virtual void ReloadItems()
        {
            this.OnItemsChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playlist();
        }
    }
}
