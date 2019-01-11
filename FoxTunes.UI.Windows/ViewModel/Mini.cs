using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Mini : ViewModelBase
    {
        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private BooleanConfigurationElement _ShowArtwork { get; set; }

        public BooleanConfigurationElement ShowArtwork
        {
            get
            {
                return this._ShowArtwork;
            }
            set
            {
                this._ShowArtwork = value;
                this.OnShowArtworkChanged();
            }
        }

        protected virtual void OnShowArtworkChanged()
        {
            if (this.ShowArtworkChanged != null)
            {
                this.ShowArtworkChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowArtwork");
        }

        public event EventHandler ShowArtworkChanged = delegate { };

        private BooleanConfigurationElement _ResetPlaylist { get; set; }

        public BooleanConfigurationElement ResetPlaylist
        {
            get
            {
                return this._ResetPlaylist;
            }
            set
            {
                this._ResetPlaylist = value;
                this.OnResetPlaylistChanged();
            }
        }

        protected virtual void OnResetPlaylistChanged()
        {
            if (this.ResetPlaylistChanged != null)
            {
                this.ResetPlaylistChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ResetPlaylist");
        }

        public event EventHandler ResetPlaylistChanged = delegate { };

        public ICommand DragEnterCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragEnter);
            }
        }

        protected virtual void OnDragEnter(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    effects = DragDropEffects.Copy;
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to query clipboard contents: {0}", exception.Message);
            }
            e.Effects = effects;
        }

        public ICommand DropCommand
        {
            get
            {
                return new AsyncCommand<DragEventArgs>(this.BackgroundTaskRunner, this.OnDrop);
            }
        }

        protected virtual Task OnDrop(DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                    return this.AddToPlaylist(paths);
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to process clipboard contents: {0}", exception.Message);
            }
            return Task.CompletedTask;
        }

        private async Task AddToPlaylist(IEnumerable<string> paths)
        {
            var index = await this.PlaylistManager.GetInsertIndex();
            await this.PlaylistManager.Add(paths, this.ResetPlaylist.Value);
            await this.PlaylistManager.Play(index);
        }

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.Configuration = this.Core.Components.Configuration;
            this.ShowArtwork = this.Configuration.GetElement<BooleanConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.SHOW_ARTWORK_ELEMENT
            );
            this.ResetPlaylist = this.Configuration.GetElement<BooleanConfigurationElement>(
              MiniPlayerBehaviourConfiguration.SECTION,
              MiniPlayerBehaviourConfiguration.RESET_PLAYLIST_ELEMENT
            );
            this.OnCommandsChanged();
            base.InitializeComponent(core);
        }

        protected virtual void OnCommandsChanged()
        {
            this.OnPropertyChanged("DragEnterCommand");
            this.OnPropertyChanged("DropCommand");
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Mini();
        }
    }
}
