using System;

namespace FoxTunes.Interfaces
{
    public interface IOutputStreamQueue: IStandardComponent
    {
        void Enqueue(IOutputStream outputStream);

        event EventHandler Enqueued;

        IOutputStream Dequeue();
    }
}
