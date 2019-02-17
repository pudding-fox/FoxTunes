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
        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IEnumerable<LibraryHierarchy> Hierarchies
        {
            get
            {
                if (this.LibraryHierarchyBrowser != null)
                {
                    return this.LibraryHierarchyBrowser.GetHierarchies();
                }
                return Enumerable.Empty<LibraryHierarchy>();
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
                if (this.LibraryManager != null)
                {
                    return this.LibraryManager.SelectedHierarchy;
                }
                return null;
            }
            set
            {
                this.LibraryManager.SelectedHierarchy = value;
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

        public abstract void Refresh();

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

        protected abstract LibraryHierarchyNode GetSelectedItem();

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
                                        return this.AddToPlaylist(this.GetSelectedItem(), false);
                                    case LibraryActionsBehaviour.REPLACE_PLAYLIST:
                                        return this.AddToPlaylist(this.GetSelectedItem(), true);
                                }
                                break;
                        }
                    }
                    break;
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
                    () => this.AddToPlaylist(this.GetSelectedItem(), false),
                    () => this.GetSelectedItem() != null && this.GetSelectedItem().IsLeaf
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
            base.Dispose(disposing);
        }
    }
}
