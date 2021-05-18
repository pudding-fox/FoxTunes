using System;
using System.IO;

namespace FoxTunes.Interfaces
{
    public interface IFileAbstraction : IDisposable
    {
        string FileName { get; }

        Stream ReadStream { get; }

        Stream WriteStream { get; }

        void CloseStream(Stream stream);
    }
}
