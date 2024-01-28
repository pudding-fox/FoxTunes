using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOutputStream : IBaseComponent, IDisposable
    {
        int Id { get; }

        string FileName { get; }

        PlaylistItem PlaylistItem { get; }

        long Position { get; set; }

        long Length { get; }

        int Rate { get; }

        int Channels { get; }

        bool IsPlaying { get; }

        event AsyncEventHandler IsPlayingChanged;

        bool IsPaused { get; }

        event AsyncEventHandler IsPausedChanged;

        bool IsStopped { get; }

        event AsyncEventHandler IsStoppedChanged;

        Task Play();

        Task Pause();

        Task Resume();

        Task Stop();

        event AsyncEventHandler Stopping;

        event StoppedEventHandler Stopped;

        string Description { get; }

        Task BeginSeek();

        Task EndSeek();

        bool IsDisposed { get; }
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

    public class StoppedEventArgs : AsyncEventArgs
    {
        public StoppedEventArgs(bool manual)
        {
            this.Manual = manual;
        }

        public bool Manual { get; private set; }
    }
}
