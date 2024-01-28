using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamInput : IBassStreamComponent
    {
        bool PreserveBuffer { get; }

        bool CheckFormat(BassOutputStream stream);

        void Connect(BassOutputStream stream);

        bool Contains(BassOutputStream stream);

        bool Add(BassOutputStream stream);

        bool Remove(BassOutputStream stream, Action<BassOutputStream> callBack);

        void Reset();
    }
}
