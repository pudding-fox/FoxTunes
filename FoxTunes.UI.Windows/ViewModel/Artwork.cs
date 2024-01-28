using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Artwork : ViewModelBase
    {
        public IPlaylistManager PlaylistManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private MetaDataItem _Image { get; set; }

        public MetaDataItem Image
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
            this.Image = playlistItem.MetaDatas.FirstOrDefault(
                metaDataItem => metaDataItem.Type == MetaDataItemType.Image && metaDataItem.Name == CommonImageTypes.FrontCover
            );
        }

        protected override void OnCoreChanged()
        {
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
