using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Artwork : ViewModelBase
    {
        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IArtworkProvider ArtworkProvider { get; private set; }

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
            var image = default(MetaDataItem);
            var playlistItem = this.PlaylistManager.CurrentItem;
            if (playlistItem != null)
            {
                image = this.ArtworkProvider.Find(playlistItem, ArtworkType.FrontCover);
                if (image == null)
                {
                    image = this.ArtworkProvider.Find(playlistItem.FileName, ArtworkType.FrontCover);
                }
            }
            //TODO: Bad awaited Task.
            this.ForegroundTaskRunner.Run(() => this.Image = image);
        }

        public override void InitializeComponent(ICore core)
        {
            this.ForegroundTaskRunner = this.Core.Components.ForegroundTaskRunner;
            this.ArtworkProvider = this.Core.Components.ArtworkProvider;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaylistManager.CurrentItemChanged += (sender, e) => this.Refresh();
            this.Refresh();
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Artwork();
        }
    }
}
