using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowser : LibraryBase
    {
        public ArtworkGridProvider ArtworkGridProvider { get; private set; }

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

        private DoubleConfigurationElement _ScalingFactor { get; set; }

        public DoubleConfigurationElement ScalingFactor
        {
            get
            {
                return this._ScalingFactor;
            }
            set
            {
                this._ScalingFactor = value;
                this.OnScalingFactorChanged();
            }
        }

        protected virtual void OnScalingFactorChanged()
        {
            if (this.ScalingFactorChanged != null)
            {
                this.ScalingFactorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ScalingFactor");
        }

        public event EventHandler ScalingFactorChanged;

        private IntegerConfigurationElement _TileSize { get; set; }

        public IntegerConfigurationElement TileSize
        {
            get
            {
                return this._TileSize;
            }
            set
            {
                this._TileSize = value;
                this.OnTileSizeChanged();
            }
        }

        protected virtual void OnTileSizeChanged()
        {
            if (this.TileSizeChanged != null)
            {
                this.TileSizeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TileSize");
        }

        public event EventHandler TileSizeChanged;

        public bool IsSlave
        {
            get
            {
                return LayoutManager.Instance.IsComponentActive(typeof(global::FoxTunes.LibraryTree));
            }
        }

        protected virtual void OnIsSlaveChanged()
        {
            if (this.IsSlaveChanged != null)
            {
                this.IsSlaveChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSlave");
        }

        public event EventHandler IsSlaveChanged;

        public override void InitializeComponent(ICore core)
        {
            this.ArtworkGridProvider = ComponentRegistry.Instance.GetComponent<ArtworkGridProvider>();
            if (this.ArtworkGridProvider != null)
            {
                this.ArtworkGridProvider.Cleared += this.OnCleared;
            }
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            if (this.ScalingFactor != null)
            {
                this.ScalingFactor.ValueChanged += this.OnValueChanged;
            }
            this.TileSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            if (this.TileSize != null)
            {
                this.TileSize.ValueChanged += this.OnValueChanged;
            }
            LayoutManager.Instance.ActiveComponentsChanged += this.OnActiveComponentsChanged;
            this.OnIsSlaveChanged();
            base.InitializeComponent(core);
        }

        protected virtual void OnCleared(object sender, EventArgs e)
        {
#if NET40
            var task = TaskEx.Run(() => this.Refresh());
#else
            var task = Task.Run(() => this.Refresh());
#endif
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
#if NET40
            var task = TaskEx.Run(() => this.Reload());
#else
            var task = Task.Run(() => this.Reload());
#endif
        }

        public bool IsRefreshing { get; private set; }

        public override void Refresh()
        {
            this.IsRefreshing = true;
            try
            {
                this.Synchronize(new List<LibraryBrowserFrame>(new[]
                {
                    new LibraryBrowserFrame(LibraryHierarchyNode.Empty, this.Items)
                }));
                this.OnItemsChanged();
            }
            finally
            {
                this.IsRefreshing = false;
            }
        }

        public bool IsReloading { get; private set; }

        public override async Task Reload()
        {
            this.IsReloading = true;
            try
            {
                this.Synchronize(new List<LibraryBrowserFrame>(new[]
                {
                    new LibraryBrowserFrame(LibraryHierarchyNode.Empty, this.Items)
                }));
                await base.Reload().ConfigureAwait(false);
            }
            finally
            {
                this.IsReloading = false;
            }
        }

        protected virtual void OnActiveComponentsChanged(object sender, EventArgs e)
        {
            this.OnIsSlaveChanged();
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
                return new Command<bool>(this.Browse);
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

        private bool Down(LibraryHierarchyNode libraryHierarchyNode, bool updateSelection)
        {
            return this.Down(libraryHierarchyNode, this.Frames, updateSelection);
        }

        private bool Down(LibraryHierarchyNode libraryHierarchyNode, IList<LibraryBrowserFrame> frames, bool updateSelection)
        {
            var libraryHierarchyNodes = this.LibraryHierarchyBrowser.GetNodes(libraryHierarchyNode);
            if (!libraryHierarchyNodes.Any())
            {
                return false;
            }
            frames.Add(
                new LibraryBrowserFrame(
                    libraryHierarchyNode,
                    new[] { LibraryHierarchyNode.Empty }.Concat(libraryHierarchyNodes)
                )
            );
            if (updateSelection && object.ReferenceEquals(this.Frames, frames))
            {
                this.SelectedItem = libraryHierarchyNodes.FirstOrDefault();
            }
            return true;
        }

        private void Synchronize()
        {
            this.Synchronize(this.Frames);
        }

        private void Synchronize(IList<LibraryBrowserFrame> frames)
        {
            if (this.SelectedItem == null || LibraryHierarchyNode.Empty.Equals(this.SelectedItem))
            {
                this.Frames = new ObservableCollection<LibraryBrowserFrame>(frames);
                return;
            }
            var stack = new Stack<LibraryHierarchyNode>();
            var libraryHierarchyNode = this.SelectedItem;
            while (libraryHierarchyNode.Parent != null)
            {
                libraryHierarchyNode = libraryHierarchyNode.Parent;
                stack.Push(libraryHierarchyNode);
            }
            stack.Push(LibraryHierarchyNode.Empty);
            var position = 0;
            while (stack.Count > 0)
            {
                libraryHierarchyNode = stack.Pop();
                if (position >= frames.Count)
                {
                    if (!this.Down(libraryHierarchyNode, frames, false))
                    {
                        break;
                    }
                }
                else
                {
                    var frame = frames[position];
                    if (!frame.ItemsSource.Equals(libraryHierarchyNode))
                    {
                        for (; position < frames.Count; position++)
                        {
                            frames.RemoveAt(frames.Count - 1);
                        }
                        if (!this.Down(libraryHierarchyNode, frames, false))
                        {
                            break;
                        }
                    }
                }
                position++;
            }
            for (var count = frames.Count; position < count; position++)
            {
                frames.RemoveAt(frames.Count - 1);
            }
            if (!object.ReferenceEquals(this.Frames, frames))
            {
                this.Frames = new ObservableCollection<LibraryBrowserFrame>(frames);
            }
        }

        protected override void OnDisposing()
        {
            if (this.ArtworkGridProvider != null)
            {
                this.ArtworkGridProvider.Cleared -= this.OnCleared;
            }
            if (this.ScalingFactor != null)
            {
                this.ScalingFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.TileSize != null)
            {
                this.TileSize.ValueChanged -= this.OnValueChanged;
            }
            LayoutManager.Instance.ActiveComponentsChanged -= this.OnActiveComponentsChanged;
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryBrowser();
        }
    }
}
