using System;

namespace FoxTunes.Interfaces
{
    public interface IOutputStream : IBaseComponent, IDisposable
    {
        long Position { get; set; }

        event EventHandler PositionChanged;

        long Length { get; }

        int BlockAlign { get; }

        bool IsPlaying { get; }

        event EventHandler IsPlayingChanged;

        bool IsPaused { get; }

        event EventHandler IsPausedChanged;

        bool IsStopped { get; }

        event EventHandler IsStoppedChanged;

        void Play();

        event PlayedEventHandler Played;

        void Pause();

        event EventHandler Paused;

        void Resume();

        event EventHandler Resumed;

        void Stop();

        event StoppedEventHandler Stopped;
    }

    public delegate void PlayedEventHandler(object sender, PlayedEventArgs e);

    public class PlayedEventArgs : EventArgs
    {
        public PlayedEventArgs(bool manual)
        {
            this.Manual = manual;
        }

        public bool Manual { get; private set; }
    }

    public delegate void StoppedEventHandler(object sender, StoppedEventArgs e);

    public class StoppedEventArgs : EventArgs
    {
        public StoppedEventArgs(bool manual)
        {
            this.Manual = manual;
        }

        public bool Manual { get; private set; }
    }
}
