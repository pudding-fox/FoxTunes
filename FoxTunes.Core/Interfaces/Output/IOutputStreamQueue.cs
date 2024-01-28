using System;

namespace FoxTunes.Interfaces
{
    public interface IOutputStreamQueue : IStandardComponent, IDisposable
    {
        bool IsQueued(PlaylistItem playlistItem);

        IOutputStream Peek(PlaylistItem playlistItem);

        void Enqueue(IOutputStream outputStream, bool dequeue);

        void Dequeue(PlaylistItem playlistItem);

        event OutputStreamQueueEventHandler Dequeued;
    }

    public delegate void OutputStreamQueueEventHandler(object sender, OutputStreamQueueEventArgs e);

    public class OutputStreamQueueEventArgs : EventArgs
    {
        public OutputStreamQueueEventArgs(IOutputStream outputStream)
        {
            this.OutputStream = outputStream;
        }

        public IOutputStream OutputStream { get; private set; }
    }
}
