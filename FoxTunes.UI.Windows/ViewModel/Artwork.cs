using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Artwork : ViewModelBase
    {
        public Artwork()
        {
        }

        public IDataManager DataManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

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
            this.Refresh();
        }

        public event EventHandler ImageTypeChanged = delegate { };

        private ImageItem _Image { get; set; }

        public ImageItem Image
        {
            get
            {
                return this._Image;
            }
            set
            {
                this._Image = value;
                this.OnImageChanged();
            }
        }

        protected virtual void OnImageChanged()
        {
            if (this.ImageChanged != null)
            {
                this.ImageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Image");
        }

        public event EventHandler ImageChanged = delegate { };

        public void Refresh()
        {
            if (this.PlaylistManager == null)
            {
                this.Image = null;
                return;
            }
            var playlistItem = this.PlaylistManager.CurrentItem;
            if (playlistItem == null)
            {
                this.Image = null;
                return;
            }
            this.Image = playlistItem.Images.FirstOrDefault(
                imageItem => imageItem.MetaDatas.Any(
                    metaDataItem => metaDataItem.Name == "Type" && metaDataItem.TextValue == this.ImageType
                )
            );
        }

        protected override void OnCoreChanged()
        {
            this.DataManager = this.Core.Managers.Data;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaylistManager.CurrentItemChanged += (sender, e) => this.Refresh();
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Artwork();
        }
    }
}
