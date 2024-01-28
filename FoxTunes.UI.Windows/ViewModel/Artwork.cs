using FoxTunes.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes.ViewModel
{
    public class Artwork : ViewModelBase, IValueConverter
    {
        public Artwork()
        {
            this.EmbeddedImages = new ObservableCollection<EmbeddedImage>();
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ObservableCollection<EmbeddedImage> EmbeddedImages { get; private set; }

        protected virtual void OnEmbeddedImagesChanged()
        {
            this.OnPropertyChanged("EmbeddedImages");
            this.OnEmbeddedImageChanged();
        }

        private string _ImageType { get; set; }

        public string ImageType
        {
            get
            {
                return this._ImageType;
            }
            set
            {
                this._ImageType = value;
                this.OnImageTypeChanged();
            }
        }

        protected virtual void OnImageTypeChanged()
        {
            if (this.ImageTypeChanged != null)
            {
                this.ImageTypeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ImageType");
            this.OnEmbeddedImageChanged();
        }

        public event EventHandler ImageTypeChanged = delegate { };

        public EmbeddedImage EmbeddedImage
        {
            get
            {
                return this.EmbeddedImages.FirstOrDefault(embeddedImage => string.Equals(embeddedImage.ImageType, this.ImageType, StringComparison.OrdinalIgnoreCase));
            }
        }

        protected virtual void OnEmbeddedImageChanged()
        {
            this.OnPropertyChanged("EmbeddedImage");
        }

        public async Task Refresh()
        {
            if (this.PlaylistManager.CurrentItem == null)
            {
                this.EmbeddedImages = new ObservableCollection<EmbeddedImage>();
                return;
            }
            this.EmbeddedImages = new ObservableCollection<EmbeddedImage>(
                await this.PlaylistManager.CurrentItem.GetEmbeddedImages()
            );
            await this.ForegroundTaskRunner.Run(this.OnEmbeddedImagesChanged);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var embeddedImage = value as EmbeddedImage;
            if (embeddedImage == null)
            {
                return null;
            }
            var fileName = FileMetaDataStore.GetFileName(embeddedImage.Encode().Result);
            return new BitmapImage(new Uri(fileName));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override void OnCoreChanged()
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            this.ForegroundTaskRunner = this.Core.Components.ForegroundTaskRunner;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaylistManager.CurrentItemChanged += (sender, e) => this.BackgroundTaskRunner.Run(this.Refresh);
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Artwork();
        }
    }
}
