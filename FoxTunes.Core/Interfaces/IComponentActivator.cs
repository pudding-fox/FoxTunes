using System;

namespace FoxTunes.Interfaces
{
    public interface IComponentActivator
    {
        T Activate<T>(Type type) where T : IBaseComponent;
    }
}
