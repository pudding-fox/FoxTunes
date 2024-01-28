using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public abstract class LibraryBase : ViewModelBase
    {
        public bool IsNavigating { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        private IConfiguration Configuration { get; set; }

        private LibraryHierarchyCollection _Hierarchies { get; set; }

        public LibraryHierarchyCollection Hierarchies
        {
            get
            {
                return this._Hierarchies;
            }
            set
            {
                this._Hierarchies = value;
                this.OnHierarchiesChanged();
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

        public event EventHandler HierarchiesChanged;

        public LibraryHierarchy SelectedHierarchy
        {
            get
            {
                if (this.LibraryManager == null)
                {
                    return LibraryHierarchy.Empty;
                }
                return this.LibraryManager.SelectedHierarchy;
            }
            set
            {
                if (this.LibraryManager == null || value == null)
                {
                    return;
                }
                this.IsNavigating = true;
                try
                {
                    this.LibraryManager.SelectedHierarchy = value;
                }
                finally
                {
                    this.IsNavigating = false;
                }
            }
        }

        protected virtual void OnSelectedHierarchyChanged()
        {
            if (this.SelectedHierarchyChanged != null)
            {
                this.SelectedHierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedHierarchy");
        }

        private LibraryHierarchyNodeCollection _Items { get; set; }

        public virtual LibraryHierarchyNodeCollection Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
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

        public event EventHandler ItemsChanged;

        public virtual LibraryHierarchyNode SelectedItem
        {
            get
            {
                if (this.LibraryManager == null)
                {
                    return LibraryHierarchyNode.Empty;
                }
                return this.LibraryManager.SelectedItem;
            }
            set
            {
                if (this.LibraryManager == null || value == null)
                {
                    return;
                }
                this.IsNavigating = true;
                try
                {
                    this.LibraryManager.SelectedItem = value;
                }
                finally
                {
                    this.IsNavigating = false;
                }
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

        public event EventHandler SelectedItemChanged;

        private int _SearchInterval { get; set; }

        public virtual int SearchInterval
        {
            get
            {
                return this._SearchInterval;
            }
            set
            {
                this._SearchInterval = value;
                this.OnSearchIntervalChanged();
            }
        }

        protected virtual void OnSearchIntervalChanged()
        {
            if (this.SearchIntervalChanged != null)
            {
                this.SearchIntervalChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SearchInterval");
        }

        public event EventHandler SearchIntervalChanged;

        private SearchCommitBehaviour _SearchCommitBehaviour { get; set; }

        public virtual SearchCommitBehaviour SearchCommitBehaviour
        {
            get
            {
                return this._SearchCommitBehaviour;
            }
            set
            {
                this._SearchCommitBehaviour = value;
                this.OnSearchCommitBehaviourChanged();
            }
        }

        protected virtual void OnSearchCommitBehaviourChanged()
        {
            if (this.SearchCommitBehaviourChanged != null)
            {
                this.SearchCommitBehaviourChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SearchCommitBehaviour");
        }

        public event EventHandler SearchCommitBehaviourChanged;

        private bool _ShowCursorAdorners { get; set; }

        public virtual bool ShowCursorAdorners
        {
            get
            {
                return this._ShowCursorAdorners;
            }
            set
            {
                this._ShowCursorAdorners = value;
                this.OnShowCursorAdornersChanged();
            }
        }

        protected virtual void OnShowCursorAdornersChanged()
        {
            if (this.ShowCursorAdornersChanged != null)
            {
                this.ShowCursorAdornersChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowCursorAdorners");
        }

        public event EventHandler ShowCursorAdornersChanged;

        public string StatusMessage
        {
            get
            {
                if (this.HierarchyManager != null)
                {
                    if (!this.HierarchyManager.CanNavigate)
                    {
                        var isUpdating = global::FoxTunes.BackgroundTask.Active
                            .OfType<LibraryTaskBase>()
                            .Any();
                        if (isUpdating)
                        {
                            return "Updating...";
                        }
                        if (this.LibraryHierarchyBrowser != null)
                        {
                            switch (this.LibraryHierarchyBrowser.State)
                            {
                                case LibraryHierarchyBrowserState.Loading:
                                    return "Loading...";
                            }
                        }
                        return "Add to collection by dropping files here.";
                    }
                }
                return null;
            }
        }

        protected virtual void OnStatusMessageChanged()
        {
            if (this.StatusMessageChanged != null)
            {
                this.StatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("StatusMessage");
        }

        public event EventHandler StatusMessageChanged;

        public bool HasStatusMessage
        {
            get
            {
                if (this.Items != null && this.Items.Count > 0)
                {
                    return false;
                }
                if (this.HierarchyManager != null)
                {
                    if (!this.HierarchyManager.CanNavigate)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected virtual void OnHasStatusMessageChanged()
        {
            if (this.HasStatusMessageChanged != null)
            {
                this.HasStatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasStatusMessage");
        }

        public event EventHandler HasStatusMessageChanged;

        protected virtual Task RefreshStatus()
        {
            return Windows.Invoke(() =>
            {
                this.OnStatusMessageChanged();
                this.OnHasStatusMessageChanged();
            });
        }

        public virtual async Task Refresh()
        {
            await this.RefreshItems().ConfigureAwait(false);
            await Windows.Invoke(() =>
            {
                this.OnSelectedItemChanged();
            }).ConfigureAwait(false);
        }

        protected virtual Task RefreshItems()
        {
            var items = this.LibraryHierarchyBrowser.GetNodes(this.SelectedHierarchy);
            if (this.Items == null)
            {
                return Windows.Invoke(() => this.Items = new LibraryHierarchyNodeCollection(items));
            }
            else
            {
                return Windows.Invoke(this.Items.Update(items));
            }
        }

        protected virtual Task RefreshHierarchies()
        {
            var hierarchies = this.LibraryHierarchyBrowser.GetHierarchies();
            if (this.Hierarchies == null)
            {
                return Windows.Invoke(() => this.Hierarchies = new LibraryHierarchyCollection(hierarchies));
            }
            else
            {
                return Windows.Invoke(this.Hierarchies.Update(hierarchies));
            }
        }

        public virtual async Task Reload()
        {
            await this.RefreshHierarchies().ConfigureAwait(false);
            await Windows.Invoke(() =>
            {
                this.OnSelectedHierarchyChanged();
            }).ConfigureAwait(false);
            await this.Refresh().ConfigureAwait(false);
        }

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged += this.OnActiveChanged;
            this.LibraryHierarchyBrowser = this.Core.Components.LibraryHierarchyBrowser;
            this.LibraryHierarchyBrowser.FilterChanged += this.OnFilterChanged;
            this.LibraryHierarchyBrowser.StateChanged += this.OnStateChanged;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.LibraryManager = this.Core.Managers.Library;
            this.LibraryManager.SelectedHierarchyChanged += this.OnSelectedHierarchyChanged;
            this.LibraryManager.SelectedItemChanged += this.OnSelectedItemChanged;
            this.HierarchyManager = this.Core.Managers.Hierarchy;
            this.HierarchyManager.CanNavigateChanged += this.OnCanNavigateChanged;
            this.Configuration = this.Core.Components.Configuration;
            this.Configuration.GetElement<IntegerConfigurationElement>(
                SearchBehaviourConfiguration.SECTION,
                SearchBehaviourConfiguration.SEARCH_INTERVAL_ELEMENT
            ).ConnectValue(value => this.SearchInterval = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                SearchBehaviourConfiguration.SECTION,
                SearchBehaviourConfiguration.SEARCH_COMMIT_ELEMENT
            ).ConnectValue(option => this.SearchCommitBehaviour = SearchBehaviourConfiguration.GetCommitBehaviour(option));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.SHOW_CURSOR_ADORNERS
            ).ConnectValue(value => this.ShowCursorAdorners = value);
            //TODO: Bad .Wait().
            this.Reload().Wait();
            this.RefreshStatus().Wait();
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            var task = this.RefreshStatus();
        }

        protected virtual void OnFilterChanged(object sender, EventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            var task = this.RefreshStatus();
        }

        protected virtual void OnSelectedHierarchyChanged(object sender, EventArgs e)
        {
            this.OnSelectedHierarchyChanged();
            var task = this.Refresh();
        }

        protected virtual void OnSelectedItemChanged(object sender, EventArgs e)
        {
            this.OnSelectedItemChanged();
        }

        protected virtual void OnCanNavigateChanged(object sender, EventArgs e)
        {
            var task = this.RefreshStatus();
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    return this.Reload();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public ICommand AddToPlaylistCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand<bool>(
                    force => this.AddToPlaylist(this.SelectedItem, false),
                    force => this.SelectedItem != null && (force || this.SelectedItem.IsLeaf)
                );
            }
        }

        private async Task AddToPlaylist(LibraryHierarchyNode libraryHierarchyNode, bool clear)
        {
            await this.PlaylistManager.Add(libraryHierarchyNode, clear).ConfigureAwait(false);
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
            this.UpdateDragDropEffects(e);
        }

        public ICommand DragOverCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragOver);
            }
        }

        protected virtual void OnDragOver(DragEventArgs e)
        {
            this.UpdateDragDropEffects(e);
        }

        protected virtual void UpdateDragDropEffects(DragEventArgs e)
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
#if VISTA
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    effects = DragDropEffects.Copy;
                }
#endif
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
#if VISTA
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    var paths = ShellIDListHelper.GetData(e.Data);
                    return this.LibraryManager.Add(paths);
                }
#endif
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to process clipboard contents: {0}", exception.Message);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public event EventHandler SelectedHierarchyChanged;

        public ICommand SearchCommitCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.SearchCommit);
            }
        }

        public Task SearchCommit()
        {
            var clear = default(bool);
            switch (this.SearchCommitBehaviour)
            {
                case SearchCommitBehaviour.Replace:
                    clear = true;
                    break;
                case SearchCommitBehaviour.Append:
                    clear = false;
                    break;
            }
            return this.PlaylistManager.Add(this.Items, clear);
        }

        protected override void Dispose(bool disposing)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged -= this.OnActiveChanged;
            if (this.LibraryHierarchyBrowser != null)
            {
                this.LibraryHierarchyBrowser.FilterChanged -= this.OnFilterChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            if (this.LibraryManager != null)
            {
                this.LibraryManager.SelectedHierarchyChanged -= this.OnSelectedHierarchyChanged;
                this.LibraryManager.SelectedItemChanged -= this.OnSelectedItemChanged;
            }
            base.Dispose(disposing);
        }
    }
}
