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
                this._Frames = value;
                this.OnFramesChanged();
            }
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
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.ScalingFactor.ValueChanged += this.OnValueChanged;
            this.TileSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            this.TileSize.ValueChanged += this.OnValueChanged;
            LayoutManager.Instance.ActiveComponentsChanged += this.OnActiveComponentsChanged;
            this.OnIsSlaveChanged();
            base.InitializeComponent(core);

        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
#if NET40
            var task = TaskEx.Run(() => this.Refresh());
#else
            var task = Task.Run(() => this.Refresh());
#endif
        }

        public override void Refresh()
        {
            this.Synchronize(new List<LibraryBrowserFrame>(new[]
            {
                new LibraryBrowserFrame(LibraryHierarchyNode.Empty, this.Items)
            }));
            this.OnItemsChanged();
        }

        public override Task Reload()
        {
            this.Synchronize(new List<LibraryBrowserFrame>(new[]
            {
                new LibraryBrowserFrame(LibraryHierarchyNode.Empty, this.Items)
            }));
            return base.Reload();
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
            this.Up(this.Frames);
        }

        private void Up(IList<LibraryBrowserFrame> frames)
        {
            var frame = frames.LastOrDefault();
            if (frame == null)
            {
                return;
            }
            frames.Remove(frame);
            this.SelectedItem = frame.ItemsSource;
        }

        private void Down()
        {
            this.Down(this.SelectedItem);
        }

        private void Down(LibraryHierarchyNode libraryHierarchyNode)
        {
            this.Down(libraryHierarchyNode, this.Frames);
        }

        private void Down(LibraryHierarchyNode libraryHierarchyNode, IList<LibraryBrowserFrame> frames)
        {
            frames.Add(
                new LibraryBrowserFrame(
                    libraryHierarchyNode,
                    new[] { LibraryHierarchyNode.Empty }.Concat(this.LibraryHierarchyBrowser.GetNodes(libraryHierarchyNode))
                )
            );
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
                    this.Down(libraryHierarchyNode, frames);
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
                        this.Down(libraryHierarchyNode, frames);
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
            LayoutManager.Instance.ActiveComponentsChanged -= this.OnActiveComponentsChanged;
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryBrowser();
        }
    }
}
