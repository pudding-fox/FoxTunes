using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class Playlist : StandardComponent, IPlaylist
    {
        public Playlist()
        {
            this.Items = new PlaylistItems();
        }

        public IDatabase Database { get; private set; }

        public IPlaylistItems Items { get; private set; }

        private IPlaylistItem _SelectedItem { get; set; }

        public IPlaylistItem SelectedItem
        {
            get
            {
                return this._SelectedItem;
            }
            set
            {
                this.OnSelectedItemChanging();
                this._SelectedItem = value;
                this.OnSelectedItemChanged();
            }
        }

        protected virtual void OnSelectedItemChanging()
        {
            if (this.SelectedItemChanging != null)
            {
                this.SelectedItemChanging(this, EventArgs.Empty);
            }
            this.OnPropertyChanging("SelectedItem");
        }

        public event EventHandler SelectedItemChanging = delegate { };

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItemChanged != null)
            {
                this.SelectedItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItem");
        }

        public event EventHandler SelectedItemChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }
    }
}