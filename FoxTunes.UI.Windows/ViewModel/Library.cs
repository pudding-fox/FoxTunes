using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public abstract class Library : ViewModelBase
    {
        public Library()
        {
            this._Items = new Dictionary<LibraryHierarchy, ObservableCollection<LibraryHierarchyNode>>();
            this._SelectedItem = LibraryHierarchyNode.Empty;
        }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IEnumerable Hierarchies
        {
            get
            {
                if (this.DatabaseFactory != null)
                {
                    using (var database = this.DatabaseFactory.Create())
                    {
                        using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
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
                if (object.ReferenceEquals(this._SelectedItem, value))
                {
                    return;
                }
                this.OnSelectedItemChanging();
                this._SelectedItem = value;
                this.OnSelectedItemChanged();
            }
        }

        protected virtual void OnSelectedItemChanging()
        {
            if (this.SelectedItem != null)
            {
                this.SelectedItem.IsSelected = false;
            }
            if (this.SelectedItemChanging != null)
            {
                this.SelectedItemChanging(this, EventArgs.Empty);
            }
            this.OnPropertyChanging("SelectedItem");
        }

        public event EventHandler SelectedItemChanging = delegate { };

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItem != null)
            {
                this.SelectedItem.IsSelected = true;
            }
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

        public virtual Task Reload()
        {
            var selectedHierarchy = default(LibraryHierarchy);
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var queryable = database.AsQueryable<LibraryHierarchy>(transaction);
                    selectedHierarchy = queryable.OrderBy(libraryHierarchy => libraryHierarchy.Sequence).FirstOrDefault();
                }
            }
            return Windows.Invoke(() =>
            {
                this.SelectedHierarchy = selectedHierarchy;
                this.OnHierarchiesChanged();
                this._Items.Clear();
                this.Refresh();
            });
        }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryHierarchyBrowser = this.Core.Components.LibraryHierarchyBrowser;
            this.LibraryHierarchyBrowser.FilterChanged += this.OnFilterChanged;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.LibraryManager = this.Core.Managers.Library;
            var task = this.Reload();
            base.InitializeComponent(core);
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
                    return this.Reload();
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
                return CommandFactory.Instance.CreateCommand(
                    () => this.AddToPlaylist(this.SelectedItem, false),
                    () => this.SelectedItem != null && this.SelectedItem.IsLeaf
                );
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
                return CommandFactory.Instance.CreateCommand<DragEventArgs>(
                    new Func<DragEventArgs, Task>(this.OnDrop)
                );
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

        protected override void Dispose(bool disposing)
        {
            if (this.LibraryHierarchyBrowser != null)
            {
                this.LibraryHierarchyBrowser.FilterChanged -= this.OnFilterChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.Dispose(disposing);
        }
    }
}
