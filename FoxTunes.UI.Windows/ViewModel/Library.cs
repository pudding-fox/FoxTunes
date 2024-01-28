using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using FoxDb;
using System.Windows.Input;
using System.Collections;

namespace FoxTunes.ViewModel
{
    public abstract class Library : ViewModelBase
    {
        public Library()
        {
            this._Items = new Dictionary<LibraryHierarchy, ObservableCollection<LibraryHierarchyNode>>();
            this._SelectedItem = LibraryHierarchyNode.Empty;
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IEnumerable Hierarchies
        {
            get
            {
                if (this.Database != null)
                {
                    using (var database = this.Database.New())
                    {
                        using (var transaction = database.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            var set = database.Set<LibraryHierarchy>(transaction);
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
        }

        protected virtual void OnHierarchiesChanged()
        {
            if (this.HierarchiesChanged != null)
            {
                this.HierarchiesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Hierarchies");
        }

        public event EventHandler HierarchiesChanged = delegate { };

        private LibraryHierarchy _SelectedHierarchy { get; set; }

        public LibraryHierarchy SelectedHierarchy
        {
            get
            {
                return this._SelectedHierarchy;
            }
            set
            {
                this._SelectedHierarchy = value;
                this.OnSelectedHierarchyChanged();
            }
        }

        protected virtual void OnSelectedHierarchyChanged()
        {
            if (this.SelectedHierarchyChanged != null)
            {
                this.SelectedHierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedHierarchy");
            this.Refresh();
        }

        private Dictionary<LibraryHierarchy, ObservableCollection<LibraryHierarchyNode>> _Items { get; set; }

        public ObservableCollection<LibraryHierarchyNode> Items
        {
            get
            {
                if (this.LibraryHierarchyBrowser == null || this.SelectedHierarchy == null)
                {
                    return null;
                }
                if (!this._Items.ContainsKey(this.SelectedHierarchy))
                {
                    this._Items[this.SelectedHierarchy] = new ObservableCollection<LibraryHierarchyNode>(
                        this.LibraryHierarchyBrowser.GetNodes(this.SelectedHierarchy)
                    );
                }
                return this._Items[this.SelectedHierarchy];
            }
            protected set
            {
                this._Items[this.SelectedHierarchy] = value;
                this.OnItemsChanged();
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

        private LibraryHierarchyNode _SelectedItem { get; set; }

        public LibraryHierarchyNode SelectedItem
        {
            get
            {
                return this._SelectedItem;
            }
            set
            {
                this._SelectedItem = value;
                this.OnSelectedItemChanged();
            }
        }

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItemChanged != null)
            {
                this.SelectedItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItem");
        }

        public event EventHandler SelectedItemChanged = delegate { };

        public virtual void Refresh()
        {
            this.OnItemsChanged();
        }

        public virtual void Reload()
        {
            using (var database = this.Core.Components.Database.New())
            {
                using (var transaction = database.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var queryable = database.AsQueryable<LibraryHierarchy>(transaction);
                    this.SelectedHierarchy = queryable.OrderBy(libraryHierarchy => libraryHierarchy.Sequence).FirstOrDefault();
                }
            }
            this.OnHierarchiesChanged();
            this._Items.Clear();
            this.Refresh();
        }

        protected override void OnCoreChanged()
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            this.ForegroundTaskRunner = this.Core.Components.ForegroundTaskRunner;
            this.LibraryHierarchyBrowser = this.Core.Components.LibraryHierarchyBrowser;
            this.LibraryHierarchyBrowser.FilterChanged += this.OnFilterChanged;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.Database = this.Core.Components.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.LibraryManager = this.Core.Managers.Library;
            this.Reload();
            this.OnCommandsChanged();
            base.OnCoreChanged();
        }

        protected virtual void OnCommandsChanged()
        {
            this.OnPropertyChanged("AddToPlaylistCommand");
            this.OnPropertyChanged("DropCommand");
        }

        protected virtual void OnFilterChanged(object sender, EventArgs e)
        {
            //Nothing to do.
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    return this.ForegroundTaskRunner.Run(() => this.Reload());
                case CommonSignals.PluginInvocation:
                    var invocation = signal.State as IInvocationComponent;
                    if (invocation != null)
                    {
                        switch (invocation.Category)
                        {
                            case InvocationComponent.CATEGORY_LIBRARY:
                                switch (invocation.Id)
                                {
                                    case LibraryActionsBehaviour.APPEND_PLAYLIST:
                                        return this.AddToPlaylist(this.SelectedItem, false);
                                    case LibraryActionsBehaviour.REPLACE_PLAYLIST:
                                        return this.AddToPlaylist(this.SelectedItem, true);
                                }
                                break;
                        }
                    }
                    break;
            }
            return Task.CompletedTask;
        }

        public ICommand AddToPlaylistCommand
        {
            get
            {
                return new AsyncCommand(this.BackgroundTaskRunner, () => this.AddToPlaylist(this.SelectedItem, false), () => this.SelectedItem != null && this.SelectedItem.IsLeaf);
            }
        }

        private async Task AddToPlaylist(LibraryHierarchyNode libraryHierarchyNode, bool clear)
        {
            if (this.SelectedItem == null)
            {
                return;
            }
            await this.PlaylistManager.Add(libraryHierarchyNode, clear);
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
                    return this.LibraryManager.Add(paths);
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to process clipboard contents: {0}", exception.Message);
            }
            return Task.CompletedTask;
        }

        public event EventHandler SelectedHierarchyChanged = delegate { };
    }
}
