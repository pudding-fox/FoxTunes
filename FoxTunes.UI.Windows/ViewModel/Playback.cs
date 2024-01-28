using FoxTunes.Interfaces;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playback : ViewModelBase
    {
        protected override Freezable CreateInstanceCore()
        {
            return new Playback();
        }

        public ICommand PlayCommand
        {
            get
            {
                return new Command<IPlaybackManager>(
                    playback => playback.CurrentStream.Play(),
                    playback => playback != null && playback.CurrentStream != null && !playback.CurrentStream.IsPlaying
                );
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return new Command<IPlaybackManager>(playback =>
                    {
                        if (playback.CurrentStream.IsPaused)
                        {
                            playback.CurrentStream.Resume();
                        }
                        else if (playback.CurrentStream.IsPlaying)
                        {
                            playback.CurrentStream.Pause();
                        }
                    },
                    playback => playback != null && playback.CurrentStream != null
                );
            }
        }

        public ICommand StopCommand
        {
            get
            {
                return new Command<IPlaybackManager>(
                    playback => playback.CurrentStream.Stop(),
                    playback => playback != null && playback.CurrentStream != null && playback.CurrentStream.IsPlaying
                );
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return new Command<IPlaylistManager>(
                    playlist => playlist.Previous(),
                    playlist => playlist != null && playlist.Items.Any()
                );
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return new Command<IPlaylistManager>(
                    playlist => playlist.Next(),
                    playlist => playlist != null && playlist.Items.Any()
                );
            }
        }
    }
}
