using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IComponentRegistry
    {
        void AddComponents(params IBaseComponent[] components);

        void AddComponents(IEnumerable<IBaseComponent> components);

        T GetComponent<T>() where T : IBaseComponent;

        IEnumerable<T> GetComponents<T>() where T : IBaseComponent;

        void ForEach(Action<IBaseComponent> action);
    }
}
