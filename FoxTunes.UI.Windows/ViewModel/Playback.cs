using FoxTunes.Interfaces;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playback : ViewModelBase
    {
        public Playback()
        {

        }

        public Playback(bool togglePlayback) : this()
        {
            this.TogglePlayback = togglePlayback;
        }

        public bool TogglePlayback { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ICommand PlayCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
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
                        else if (this.TogglePlayback)
                        {
                            return this.PlaybackManager.CurrentStream.Pause();
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                );
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream != null)
                        {
                            if (this.PlaybackManager.CurrentStream.IsPaused)
                            {
                                return this.PlaybackManager.CurrentStream.Resume();
                            }
                            else if (this.PlaybackManager.CurrentStream.IsPlaying)
                            {
                                return this.PlaybackManager.CurrentStream.Pause();
                            }
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                );
            }
        }

        public ICommand StopStreamCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream != null)
                        {
                            return this.PlaybackManager.CurrentStream.Stop();
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                );
            }
        }

        public ICommand StopOutputCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaybackManager.Stop()
                );
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaylistManager.Previous()
                );
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaylistManager.Next()
                );
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playback();
        }
    }
}
