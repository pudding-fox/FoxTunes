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

        bool ShowBuffering { get; }

        event AsyncEventHandler IsStartedChanged;

        Task Start();

        IEnumerable<string> SupportedExtensions { get; }

        bool IsSupported(string fileName);

        bool IsLoaded(string fileName);

        Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate);

        Task<IOutputStream> Duplicate(IOutputStream stream);

        event OutputStreamEventHandler Loaded;

        Task<bool> Preempt(IOutputStream stream);

        Task Unload(IOutputStream stream);

        event OutputStreamEventHandler Unloaded;

        Task Shutdown();

        int GetData(float[] buffer);
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
}
