using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowser : LibraryBase
    {
        public IConfiguration Configuration { get; private set; }

        private ObservableCollection<LibraryBrowserFrame> _Frames { get; set; }

        public ObservableCollection<LibraryBrowserFrame> Frames
        {
            get
            {
                return this._Frames;
            }
            set
            {
                if (object.ReferenceEquals(this._Frames, value))
                {
                    return;
                }
                this.OnFramesChanging();
                this._Frames = value;
                this.OnFramesChanged();
            }
        }

        protected virtual void OnFramesChanging()
        {
            if (this._Frames == null)
            {
                return;
            }
            this._Frames.ForEach(frame => frame.Dispose());
        }

        protected virtual void OnFramesChanged()
        {
            if (this.FramesChanged != null)
            {
                this.FramesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Frames");
        }

        public event EventHandler FramesChanged;

        private int _GridTileSize { get; set; }

        public int GridTileSize
        {
            get
            {
                return this._GridTileSize;
            }
            set
            {
                this._GridTileSize = value;
                this.OnGridTileSizeChanged();
            }
        }

        protected virtual void OnGridTileSizeChanged()
        {
            if (this.GridTileSizeChanged != null)
            {
                this.GridTileSizeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("GridTileSize");
        }

        public event EventHandler GridTileSizeChanged;

        private int _ListTileSize { get; set; }

        public int ListTileSize
        {
            get
            {
                return this._ListTileSize;
            }
            set
            {
                this._ListTileSize = value;
                this.OnListTileSizeChanged();
            }
        }

        protected virtual void OnListTileSizeChanged()
        {
            if (this.ListTileSizeChanged != null)
            {
                this.ListTileSizeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ListTileSize");
        }

        public event EventHandler ListTileSizeChanged;

        private LibraryBrowserImageMode _ImageMode { get; set; }

        public LibraryBrowserImageMode ImageMode
        {
            get
            {
                return this._ImageMode;
            }
            set
            {
                this._ImageMode = value;
                this.OnImageModeChanged();
            }
        }

        protected virtual void OnImageModeChanged()
        {
            if (this.IsInitialized)
            {
                this.Debouncer.Exec(this.Reload);
            }
            if (this.ImageModeChanged != null)
            {
                this.ImageModeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ImageMode");
        }

        public event EventHandler ImageModeChanged;

        protected override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_GRID_TILE_SIZE
            ).ConnectValue(value => this.GridTileSize = value);
            this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_LIST_TILE_SIZE
            ).ConnectValue(value => this.ListTileSize = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_IMAGE
            ).ConnectValue(option => this.ImageMode = LibraryBrowserBehaviourConfiguration.GetLibraryImage(option));
            base.InitializeComponent(core);
        }

        protected override async Task OnSignal(object sender, ISignal signal)
        {
            await base.OnSignal(sender, signal).ConfigureAwait(false);
            switch (signal.Name)
            {
                case CommonSignals.MetaDataUpdated:
                    await this.OnMetaDataUpdated(signal.State as MetaDataUpdatedSignalState).ConfigureAwait(false);
                    break;
                case CommonSignals.ImagesUpdated:
                    await this.Refresh().ConfigureAwait(false);
                    break;
            }
        }

        protected virtual Task OnMetaDataUpdated(MetaDataUpdatedSignalState state)
        {
            if (state != null)
            {
                return this.Refresh(state.FileDatas, state.Names, state.UpdateType);
            }
            else
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
        }

        protected virtual Task Refresh(IEnumerable<IFileData> fileDatas, IEnumerable<string> names, MetaDataUpdateType updateType)
        {
            if (updateType == MetaDataUpdateType.System)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            if (names != null && names.Any())
            {
                if (!names.Contains(CommonImageTypes.FrontCover, StringComparer.OrdinalIgnoreCase))
                {
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            if (fileDatas != null && fileDatas.Any())
            {
                return this.Refresh(fileDatas);
            }
            return this.Refresh();
        }

        protected virtual Task Refresh(IEnumerable<IFileData> fileDatas)
        {
            //Updates for specific items are handled by LibraryHierarchyBrowser.
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected override async Task OnRefresh()
        {
            await base.OnRefresh().ConfigureAwait(false);
            await Windows.Invoke(() => this.Synchronize(new List<LibraryBrowserFrame>()
            {
                new LibraryBrowserFrame(LibraryHierarchyNode.Empty, this.Items)
            })).ConfigureAwait(false);
        }

        public override async Task Reload()
        {
            await base.Reload().ConfigureAwait(false);
            await Windows.Invoke(() => this.Synchronize(new List<LibraryBrowserFrame>()
            {
                new LibraryBrowserFrame(LibraryHierarchyNode.Empty, this.Items)
            })).ConfigureAwait(false);
        }

        protected override void OnSelectedItemChanged(object sender, EventArgs e)
        {
            if (!this.IsNavigating)
            {
                this.Synchronize();
            }
            base.OnSelectedItemChanged(sender, e);
        }

        public ICommand BrowseCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand<bool>(this.Browse);
            }
        }

        public void Browse(bool up)
        {
            if (up)
            {
                this.Up();
                return;
            }
            if (this.AddToPlaylistCommand.CanExecute(false))
            {
                this.AddToPlaylistCommand.Execute(false);
                return;
            }
            if (this.SelectedItem == null || LibraryHierarchyNode.Empty.Equals(this.SelectedItem))
            {
                this.Up();
            }
            else
            {
                this.Down();
            }
        }

        private void Up()
        {
            this.Up(this.Frames, true);
        }

        private void Up(IList<LibraryBrowserFrame> frames, bool updateSelection)
        {
            if (frames.Count <= 1)
            {
                return;
            }
            var frame = frames.LastOrDefault();
            if (frame == null)
            {
                return;
            }
            frames.Remove(frame);
            if (updateSelection && object.ReferenceEquals(this.Frames, frames))
            {
                this.SelectedItem = frame.ItemsSource;
            }
        }

        private void Down()
        {
            this.Down(this.SelectedItem, true);
        }

        private void Down(LibraryHierarchyNode libraryHierarchyNode, bool updateSelection)
        {
            this.Down(libraryHierarchyNode, this.Frames, updateSelection);
        }

        private void Down(LibraryHierarchyNode libraryHierarchyNode, IList<LibraryBrowserFrame> frames, bool updateSelection)
        {
            var libraryHierarchyNodes = this.LibraryHierarchyBrowser.GetNodes(libraryHierarchyNode);
            if (!libraryHierarchyNodes.Any())
            {
                return;
            }
            var frame = new LibraryBrowserFrame(
                libraryHierarchyNode,
                new[]
                {
                    LibraryHierarchyNode.Empty
                }.Concat(libraryHierarchyNodes)
            );
            frames.Add(frame);
            if (updateSelection && object.ReferenceEquals(this.Frames, frames))
            {
                this.SelectedItem = libraryHierarchyNodes.FirstOrDefault();
            }
        }

        private void Synchronize()
        {
            this.Synchronize(this.Frames);
        }

        private void Synchronize(IList<LibraryBrowserFrame> frames)
        {
            if (this.SelectedItem == null || LibraryHierarchyNode.Empty.Equals(this.SelectedItem))
            {
                if (!object.ReferenceEquals(this.Frames, frames))
                {
                    this.Frames = new ObservableCollection<LibraryBrowserFrame>(frames);
                }
                return;
            }
            var path = new List<LibraryHierarchyNode>();
            var libraryHierarchyNode = this.SelectedItem;
            while (libraryHierarchyNode.Parent != null)
            {
                path.Insert(0, libraryHierarchyNode.Parent);
                libraryHierarchyNode = libraryHierarchyNode.Parent;
            }
            path.Insert(0, LibraryHierarchyNode.Empty);
            for (var a = 0; a < path.Count; a++)
            {
                libraryHierarchyNode = path[a];
                if (frames.Count > a)
                {
                    while (frames[a].ItemsSource != libraryHierarchyNode)
                    {
                        while (frames.Count > a)
                        {
                            this.Up(frames, false);
                        }
                        this.Down(libraryHierarchyNode, frames, false);
                    }
                }
                else
                {
                    this.Down(libraryHierarchyNode, frames, false);
                }
            }
            while (frames.Count > path.Count)
            {
                this.Up(frames, false);
            }
            if (!object.ReferenceEquals(this.Frames, frames))
            {
                this.Frames = new ObservableCollection<LibraryBrowserFrame>(frames);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryBrowser();
        }
    }
}
