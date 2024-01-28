using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Artwork.xaml
    /// </summary>
    [UIComponent("66C8A9E7-0891-48DD-8086-E40F72D4D030", UIComponentSlots.NONE, "Artwork")]
    [UIComponentDependency(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)]
    public partial class Artwork : UIComponentBase
    {
        public static readonly IArtworkProvider ArtworkProvider = ComponentRegistry.Instance.GetComponent<IArtworkProvider>();

        public static readonly IPlaybackManager PlaybackManager = ComponentRegistry.Instance.GetComponent<IPlaybackManager>();

        public static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

        public static readonly ImageLoader ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();

        public Artwork()
        {
            this.InitializeComponent();
            if (PlaybackManager != null)
            {
                PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            }
            if (ThemeLoader != null)
            {
                ThemeLoader.ThemeChanged += this.OnThemeChanged;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var task = this.Refresh();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Background = null;
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.Refresh());
#else
            var task = Task.Run(() => this.Refresh());
#endif
        }

        protected virtual void OnThemeChanged(object sender, EventArgs e)
        {
            var task = this.Refresh();
        }

        public async Task Refresh()
        {
            var fileName = default(string);
            var outputStream = PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                fileName = ArtworkProvider.Find(
                    outputStream.PlaylistItem,
                    ArtworkType.FrontCover
                );
            }
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                var source = ImageLoader.Load(
                    ThemeLoader.Theme.Id,
                    () => ThemeLoader.Theme.ArtworkPlaceholder,
                    true
                );
                var brush = new ImageBrush(source)
                {
                    Stretch = Stretch.Uniform
                };
                brush.Freeze();
                await Windows.Invoke(() =>
                {
                    this.Background = brush;
                    this.IsComponentEnabled = false;
                }).ConfigureAwait(false);
            }
            else
            {
                var source = ImageLoader.Load(
                    fileName,
                    0,
                    0,
                    true
                );
                var brush = new ImageBrush(source)
                {
                    Stretch = Stretch.Uniform
                };
                brush.Freeze();
                await Windows.Invoke(() =>
                {
                    this.Background = brush;
                    this.IsComponentEnabled = true;
                }).ConfigureAwait(false);
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (this.Parent != null)
            {
                var row = default(RowDefinition);
                var grid = this.Parent.FindAncestor<Grid>();
                if (grid != null)
                {
                    var index = Grid.GetRow(this);
                    if (index < grid.RowDefinitions.Count)
                    {
                        row = grid.RowDefinitions[index];
                        BindingHelper.AddHandler(row, RowDefinition.HeightProperty, typeof(RowDefinition), (sender, e) =>
                        {
                            this.UpdateLayoutSource(row);
                        });
                    }
                }
                this.UpdateLayoutSource(row);
            }
            base.OnVisualParentChanged(oldParent);
        }

        protected virtual void UpdateLayoutSource(RowDefinition row = null)
        {
            if (row == null || row.Height.IsAuto || row.Height.IsStar)
            {
                this.SizeChanged += this.OnSizeChanged;
            }
            else
            {
                BindingOperations.ClearBinding(this, WidthProperty);
                BindingOperations.ClearBinding(this, HeightProperty);
            }
        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            BindingOperations.ClearBinding(this, WidthProperty);
            BindingOperations.ClearBinding(this, HeightProperty);
            if (this.ActualWidth > 0)
            {
                BindingOperations.SetBinding(this, HeightProperty, new Binding("ActualWidth")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                });
            }
            else if (this.ActualHeight > 0)
            {
                BindingOperations.SetBinding(this, WidthProperty, new Binding("ActualHeight")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                });
            }
            else
            {
                return;
            }
            this.SizeChanged -= this.OnSizeChanged;
        }
    }
}
