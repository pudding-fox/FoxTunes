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
    public class Artwork : ViewModelBase
    {
        public Artwork()
        {
        }

        public IDatabase Database { get; private set; }

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
            if (playlistItem == null || !this.Database.CanQuery(playlistItem))
            {
                this.Image = null;
                return;
            }
            var query =
                from imageItem in this.Database.GetMemberQuery<PlaylistItem, ImageItem>(playlistItem, _ => _.Images)
                where imageItem.MetaDatas.Any(metaDataItem => metaDataItem.Name == CommonImageMetaData.Type && metaDataItem.TextValue == this.ImageType)
                select imageItem;
            this.Image = query.FirstOrDefault();
        }

        protected override void OnCoreChanged()
        {
            this.Database = this.Core.Components.Database;
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
