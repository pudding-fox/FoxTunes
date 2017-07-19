using FoxTunes.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class StreamPosition : ViewModelBase
    {
        public ICommand PauseCommand
        {
            get
            {
                return new Command<IPlaybackManager>(playback =>
                    {
                        if (playback.CurrentStream.IsPlaying)
                        {
                            playback.CurrentStream.Pause();
                        }
                    },
                    playback => playback != null && playback.CurrentStream != null
                );
            }
        }

        public ICommand ResumeCommand
        {
            get
            {
                return new Command<IPlaybackManager>(playback =>
                    {
                        if (playback.CurrentStream.IsPaused)
                        {
                            playback.CurrentStream.Resume();
                        }
                    },
                    playback => playback != null && playback.CurrentStream != null
                );
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new StreamPosition();
        }
    }
}
