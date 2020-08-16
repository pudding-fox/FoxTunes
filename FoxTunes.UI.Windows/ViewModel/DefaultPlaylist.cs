using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class DefaultPlaylist : GridPlaylist
    {
        protected override Task<Playlist> GetPlaylist()
        {
#if NET40
            return TaskEx.FromResult(this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist);
#else
            return Task.FromResult(this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist);
#endif
        }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.PlaylistManager.CurrentPlaylistChanged += this.OnCurrentPlaylistChanged;
            this.PlaylistManager.SelectedPlaylistChanged += this.OnSelectedPlaylistChanged;
            this.Dispatch(this.Refresh);
        }

        protected virtual void OnCurrentPlaylistChanged(object sender, EventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual void OnSelectedPlaylistChanged(object sender, EventArgs e)
        {
            if (this.PlaylistManager.CurrentPlaylist != null)
            {
                return;
            }
            var task = this.Refresh();
        }

        protected override void OnDisposing()
        {
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.CurrentPlaylistChanged -= this.OnCurrentPlaylistChanged;
                this.PlaylistManager.SelectedPlaylistChanged -= this.OnSelectedPlaylistChanged;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new DefaultPlaylist();
        }
    }
}
