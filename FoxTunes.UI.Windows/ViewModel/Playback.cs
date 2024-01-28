using FoxTunes.Interfaces;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playback : ViewModelBase
    {
        public IPlaylist Playlist { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ICommand PlayCommand
        {
            get
            {
                return new Command(
                    () => this.PlaybackManager.CurrentStream.Play(),
                    () => this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && this.PlaybackManager.CurrentStream.IsStopped
                );
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return new Command(() =>
                    {
                        if (this.PlaybackManager.CurrentStream.IsPaused)
                        {
                            this.PlaybackManager.CurrentStream.Resume();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsPlaying)
                        {
                            this.PlaybackManager.CurrentStream.Pause();
                        }
                    },
                    () => this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null
                );
            }
        }

        public ICommand StopCommand
        {
            get
            {
                return new Command(
                    () => this.PlaybackManager.CurrentStream.Stop(),
                    () => this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && this.PlaybackManager.CurrentStream.IsPlaying
                );
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return new Command(
                    () => this.PlaylistManager.Previous(),
                    () => this.PlaylistManager != null && this.Playlist.Query.Any()
                );
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return new Command(
                    () => this.PlaylistManager.Next(),
                    () => this.PlaylistManager != null && this.Playlist != null && this.Playlist.Query.Any()
                );
            }
        }

        protected override void OnCoreChanged()
        {
            this.Playlist = this.Core.Components.Playlist;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playback();
        }
    }
}
