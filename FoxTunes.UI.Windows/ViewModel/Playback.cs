using FoxTunes.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playback : ViewModelBase
    {
        public IDatabase Database { get; private set; }

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
                return new AsyncCommand(
                    () => this.PlaybackManager.Stop(),
                    () => this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && this.PlaybackManager.CurrentStream.IsPlaying
                );
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return new AsyncCommand(
                    () => this.PlaylistManager.Previous(),
                    () => this.PlaylistManager != null && this.PlaylistManager.CanNavigate
                );
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return new AsyncCommand(
                    () => this.PlaylistManager.Next(),
                    () => this.PlaylistManager != null && this.PlaylistManager.CanNavigate
                );
            }
        }

        protected override void OnCoreChanged()
        {
            this.Database = this.Core.Components.Database;
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
