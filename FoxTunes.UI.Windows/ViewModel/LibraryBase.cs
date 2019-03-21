using FoxDb;
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

        public IEnumerable<LibraryHierarchy> Hierarchies
        {
            get
            {
                if (this.LibraryHierarchyBrowser == null)
                {
                    return Enumerable.Empty<LibraryHierarchy>();
                }
                return this.LibraryHierarchyBrowser.GetHierarchies();
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
                if (this.LibraryManager == null)
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

        public virtual IEnumerable<LibraryHierarchyNode> Items
        {
            get
            {
                if (this.LibraryHierarchyBrowser == null || this.SelectedHierarchy == null)
                {
                    return Enumerable.Empty<LibraryHierarchyNode>();
                }
                return this.LibraryHierarchyBrowser.GetNodes(this.SelectedHierarchy);
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

        public virtual void Refresh()
        {
            this.OnItemsChanged();
            this.OnSelectedItemChanged();
        }

        public virtual Task Reload()
        {
            if (this.DatabaseFactory == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return Windows.Invoke(() =>
            {
                this.OnHierarchiesChanged();
                this.OnSelectedHierarchyChanged();
                this.OnItemsChanged();
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
            this.LibraryManager.SelectedHierarchyChanged += this.OnSelectedHierarchyChanged;
            this.LibraryManager.SelectedItemChanged += this.OnSelectedItemChanged;
            var task = this.Reload();
            base.InitializeComponent(core);
        }

        protected virtual void OnFilterChanged(object sender, EventArgs e)
        {
            this.Refresh();
            if (!string.IsNullOrEmpty(this.LibraryHierarchyBrowser.Filter))
            {
                this.OnSearchCompleted();
            }
        }

        protected virtual void OnSearchCompleted()
        {
            if (this.SearchCompleted == null)
            {
                return;
            }
            this.SearchCompleted(this, EventArgs.Empty);
        }

        public event EventHandler SearchCompleted;

        protected virtual void OnSelectedHierarchyChanged(object sender, EventArgs e)
        {
            this.OnSelectedHierarchyChanged();
            this.Refresh();
        }

        protected virtual void OnSelectedItemChanged(object sender, EventArgs e)
        {
            this.OnSelectedItemChanged();
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
                return CommandFactory.Instance.CreateCommand(
                    () => this.AddToPlaylist(this.SelectedItem, false),
                    () => this.SelectedItem != null && this.SelectedItem.IsLeaf
                );
            }
        }

        private async Task AddToPlaylist(LibraryHierarchyNode libraryHierarchyNode, bool clear)
        {
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
            if (this.LibraryManager != null)
            {
                this.LibraryManager.SelectedHierarchyChanged -= this.OnSelectedHierarchyChanged;
                this.LibraryManager.SelectedItemChanged -= this.OnSelectedItemChanged;
            }
            base.Dispose(disposing);
        }
    }
}
