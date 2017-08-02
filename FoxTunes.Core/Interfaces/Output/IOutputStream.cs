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

        event EventHandler PositionChanged;

        long Length { get; }

        int BytesPerSample { get; }

        int ExtraSize { get; }

        int BitsPerSample { get; }

        int BlockAlign { get; }

        int BytesPerSecond { get; }

        int SampleRate { get; }

        string Format { get; }

        int BytesPerBlock { get; }

        int Channels { get; }

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

        Task Stop();

        event StoppedEventHandler Stopped;

        string Description { get; }
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
