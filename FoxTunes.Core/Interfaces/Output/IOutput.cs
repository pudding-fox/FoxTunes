using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOutput : IStandardComponent
    {
        string Name { get; }

        string Description { get; }

        bool IsStarted { get; }

        event AsyncEventHandler IsStartedChanged;

        bool ShowBuffering { get; }

        Task Start();

        IEnumerable<string> SupportedExtensions { get; }

        bool IsSupported(string fileName);

        bool IsLoaded(string fileName);

        Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate);

        IOutputStream Duplicate(IOutputStream stream);

        event OutputStreamEventHandler Loaded;

        Task<bool> Preempt(IOutputStream stream);

        Task Unload(IOutputStream stream);

        event OutputStreamEventHandler Unloaded;

        Task Shutdown();

        bool GetFormat(out int rate, out int channels, out OutputStreamFormat format);

        bool GetChannelMap(out IDictionary<int, OutputChannel> channels);

        bool CanGetData { get; }

        event EventHandler CanGetDataChanged;

        T[] GetBuffer<T>(TimeSpan duration) where T : struct;

        int GetData(short[] buffer);

        int GetData(float[] buffer);

        float[] GetBuffer(int fftSize);

        int GetData(float[] buffer, int fftSize);
    }

    public delegate void OutputStreamEventHandler(object sender, OutputStreamEventArgs e);

    public class OutputStreamEventArgs : EventArgs
    {
        public OutputStreamEventArgs(IOutputStream stream)
        {
            this.Stream = stream;
        }

        public IOutputStream Stream { get; private set; }
    }

    public enum OutputChannel : byte
    {
        None,
        Left,
        Right,
        FrontLeft,
        FrontRight,
        RearLeft,
        RearRight,
        Center,
        LFE
    }
}
