using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowser : LibraryBase
    {
        public IConfiguration Configuration { get; private set; }

        public LayoutManager LayoutManager { get; private set; }

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
                if (this.LayoutManager == null)
                {
                    return false;
                }
                return this.LayoutManager.ActiveControls.Contains(typeof(global::FoxTunes.LibraryTree));
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

        private LibraryHierarchyNode ItemsSource { get; set; }

        public override IEnumerable<LibraryHierarchyNode> Items
        {
            get
            {
                if (this.ItemsSource != null && !LibraryHierarchyNode.Empty.Equals(this.ItemsSource))
                {
                    return new[] { LibraryHierarchyNode.Empty }.Concat(this.LibraryHierarchyBrowser.GetNodes(this.ItemsSource));
                }
                return base.Items;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.TileSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            this.LayoutManager = ComponentRegistry.Instance.GetComponent<LayoutManager>();
            this.LayoutManager.ActiveControlsChanged += this.OnActiveControlsChanged;
            this.OnIsSlaveChanged();
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveControlsChanged(object sender, EventArgs e)
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
                return new Command(this.Browse);
            }
        }

        public void Browse()
        {
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
            var itemsSource = this.ItemsSource;
            if (itemsSource != null)
            {
                this.ItemsSource = itemsSource.Parent;
            }
            this.OnItemsChanged();
            this.SelectedItem = itemsSource;
        }

        private void Down()
        {
            this.ItemsSource = this.SelectedItem;
            this.OnItemsChanged();
            this.SelectedItem = LibraryHierarchyNode.Empty;
        }

        private void Synchronize()
        {
            if (this.SelectedItem == null || LibraryHierarchyNode.Empty.Equals(this.SelectedItem))
            {
                if (this.ItemsSource == null)
                {
                    return;
                }
                this.ItemsSource = null;
            }
            else
            {
                if (this.ItemsSource == this.SelectedItem.Parent)
                {
                    return;
                }
                this.ItemsSource = this.SelectedItem.Parent;
            }
            this.OnItemsChanged();
        }

        protected override void OnDisposing()
        {
            if (this.LayoutManager != null)
            {
                this.LayoutManager.ActiveControlsChanged -= this.OnActiveControlsChanged;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryBrowser();
        }
    }
}
