using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            this.TileSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            LayoutManager.Instance.ActiveComponentsChanged += this.OnActiveComponentsChanged;
            this.OnIsSlaveChanged();
            base.InitializeComponent(core);

        }

        public override void Refresh()
        {
            this.Frames = new ObservableCollection<LibraryBrowserFrame>(new[]
            {
                new LibraryBrowserFrame(LibraryHierarchyNode.Empty, this.Items)
            });
            base.Refresh();
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
            if (this.AddToPlaylistCommand.CanExecute(null))
            {
                this.AddToPlaylistCommand.Execute(null);
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
            var frame = this.Frames.LastOrDefault();
            if (frame == null)
            {
                return;
            }
            this.Frames.Remove(frame);
            this.SelectedItem = frame.ItemsSource;
        }

        private void Down()
        {
            this.Down(this.SelectedItem);
        }

        private void Down(LibraryHierarchyNode libraryHierarchyNode)
        {
            this.Frames.Add(
                new LibraryBrowserFrame(
                    libraryHierarchyNode,
                    new[] { LibraryHierarchyNode.Empty }.Concat(this.LibraryHierarchyBrowser.GetNodes(libraryHierarchyNode))
                )
            );
        }

        private void Synchronize()
        {
            if (this.SelectedItem == null || LibraryHierarchyNode.Empty.Equals(this.SelectedItem))
            {
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
                if (position >= this.Frames.Count)
                {
                    this.Down(libraryHierarchyNode);
                }
                else
                {
                    var frame = this.Frames[position];
                    if (!frame.ItemsSource.Equals(libraryHierarchyNode))
                    {
                        for (; position < this.Frames.Count; position++)
                        {
                            this.Frames.RemoveAt(this.Frames.Count - 1);
                        }
                        this.Down(libraryHierarchyNode);
                    }
                }
                position++;
            }
            for (; position < this.Frames.Count; position++)
            {
                this.Frames.RemoveAt(this.Frames.Count - 1);
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
