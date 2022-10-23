using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IComponentResolver
    {
        string Get(string slot);

        void Add(string slot, Type component, IEnumerable<Type> components);
    }
}
