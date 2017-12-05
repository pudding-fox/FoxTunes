using FoxTunes.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class StreamPosition : ViewModelBase
    {
        public ICommand StopCommand
        {
            get
            {
                return new Command<IPlaybackManager>(playback =>
                    {
                        if (playback.CurrentStream.IsPlaying)
                        {
                            playback.CurrentStream.Stop();
                        }
                    },
                    playback => playback != null && playback.CurrentStream != null
                );
            }
        }

        public ICommand PlayCommand
        {
            get
            {
                return new Command<IPlaybackManager>(playback =>
                    {
                        if (playback.CurrentStream.IsStopped)
                        {
                            playback.CurrentStream.Play();
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
