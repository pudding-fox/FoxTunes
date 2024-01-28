using FoxTunes.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class StreamPosition : ViewModelBase
    {
        public ICommand BeginSeekCommand
        {
            get
            {
                return new Command<IPlaybackManager>(
                    playback => playback.CurrentStream.BeginSeek(),
                    playback => playback != null && playback.CurrentStream != null
                );
            }
        }

        public ICommand EndSeekCommand
        {
            get
            {
                return new Command<IPlaybackManager>(
                    playback => playback.CurrentStream.EndSeek(),
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
