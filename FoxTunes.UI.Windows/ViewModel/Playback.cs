using FoxTunes.Interfaces;
using System;
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
                return new Command<IOutputStream>(stream => stream.Play(), stream => stream != null);
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return new Command<IOutputStream>(stream =>
                {
                    if (stream.IsPaused)
                    {
                        stream.Resume();
                    }
                    else if (stream.IsPlaying)
                    {
                        stream.Pause();
                    }
                }, stream => stream != null);
            }
        }

        public ICommand StopCommand
        {
            get
            {
                return new Command<IOutputStream>(stream => stream.Stop(), stream => stream != null && stream.IsPlaying);
            }
        }
    }
}
