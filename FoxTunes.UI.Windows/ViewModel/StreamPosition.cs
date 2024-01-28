using FoxTunes.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class StreamPosition : ViewModelBase
    {
        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public ICommand BeginSeekCommand
        {
            get
            {
                return new AsyncCommand<IPlaybackManager>(
                    this.BackgroundTaskRunner,
                    playback => playback.CurrentStream.BeginSeek(),
                    playback => playback != null && playback.CurrentStream != null
                );
            }
        }

        public ICommand EndSeekCommand
        {
            get
            {
                return new AsyncCommand<IPlaybackManager>(
                    this.BackgroundTaskRunner,
                    playback => playback.CurrentStream.EndSeek(),
                    playback => playback != null && playback.CurrentStream != null
                );
            }
        }

        protected override void OnCoreChanged()
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            this.OnCommandsChanged();
            base.OnCoreChanged();
        }

        protected virtual void OnCommandsChanged()
        {
            this.OnPropertyChanged("BeginSeekCommand");
            this.OnPropertyChanged("EndSeekCommand");
        }

        protected override Freezable CreateInstanceCore()
        {
            return new StreamPosition();
        }
    }
}
