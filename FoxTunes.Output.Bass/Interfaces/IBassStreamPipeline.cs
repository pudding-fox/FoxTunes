using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamPipeline : IBassStreamControllable, IBaseComponent, IDisposable
    {
        IBassStreamInput Input { get; }

        IEnumerable<IBassStreamComponent> Components { get; }

        IBassStreamOutput Output { get; }

        long BufferLength { get; }

        void Connect();

        void ClearBuffer();
    }
}
