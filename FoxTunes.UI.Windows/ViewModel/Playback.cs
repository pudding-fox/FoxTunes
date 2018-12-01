using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playback : ViewModelBase
    {
        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IDatabase Database { get; private set; }

        public IOutput Output { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ICommand PlayCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream == null)
                        {
                            return this.PlaylistManager.Next();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsPaused)
                        {
                            return this.PlaybackManager.CurrentStream.Resume();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsStopped)
                        {
                            return this.PlaybackManager.CurrentStream.Play();
                        }
                        return Task.CompletedTask;
                    },
                    () => this.PlaybackManager != null && this.PlaylistManager != null && this.PlaylistManager.CanNavigate && (this.PlaybackManager.CurrentStream == null || (this.PlaybackManager.CurrentStream.IsPaused || this.PlaybackManager.CurrentStream.IsStopped))
                );
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream.IsPaused)
                        {
                            return this.PlaybackManager.CurrentStream.Resume();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsPlaying)
                        {
                            return this.PlaybackManager.CurrentStream.Pause();
                        }
                        return Task.CompletedTask;
                    },
                    () => this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && (this.PlaybackManager.CurrentStream.IsPlaying || this.PlaybackManager.CurrentStream.IsPaused)
                );
            }
        }

        public ICommand StopStreamCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () => this.PlaybackManager.CurrentStream.Stop(),
                    () => this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && this.PlaybackManager.CurrentStream.IsPlaying
                );
            }
        }

        public ICommand StopOutputCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
                    () => this.PlaybackManager.Stop(),
                    () => this.PlaybackManager != null && this.Output != null && this.Output.IsStarted
                );
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return new AsyncCommand(
                    this.BackgroundTaskRunner,
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
                    this.BackgroundTaskRunner,
                    () => this.PlaylistManager.Next(),
                    () => this.PlaylistManager != null && this.PlaylistManager.CanNavigate
                );
            }
        }

        protected override void OnCoreChanged()
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            this.Database = this.Core.Components.Database;
            this.Output = this.Core.Components.Output;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            this.Core.Components.Output.IsStartedChanged += (sender, e) => Command.InvalidateRequerySuggested();
            this.OnCommandsChanged();
            base.OnCoreChanged();
        }

        protected virtual void OnCommandsChanged()
        {
            this.OnPropertyChanged("PlayCommand");
            this.OnPropertyChanged("PauseCommand");
            this.OnPropertyChanged("StopStreamCommand");
            this.OnPropertyChanged("StopOutputCommand");
            this.OnPropertyChanged("PreviousCommand");
            this.OnPropertyChanged("NextCommand");
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playback();
        }
    }
}
