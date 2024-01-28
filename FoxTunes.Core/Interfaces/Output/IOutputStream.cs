using System;

namespace FoxTunes.Interfaces
{
    public interface IOutputStream : IBaseComponent, IDisposable
    {
        long Position { get; set; }

        event EventHandler PositionChanged;

        long Length { get; }

        int BlockAlign { get; }

        bool Paused { get; set; }

        void Play();

        void Stop();

        event StoppedEventHandler Stopped;
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
