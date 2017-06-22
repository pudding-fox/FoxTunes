using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IComponentScanner
    {
        string Location { get; }

        IEnumerable<string> FileNames { get; }

        IEnumerable<Type> GetComponents(Type interfaceType);
    }
}
