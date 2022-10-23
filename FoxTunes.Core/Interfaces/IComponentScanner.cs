using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IComponentScanner : IBaseComponent
    {
        IEnumerable<Type> GetStandardComponents();

        [Obsolete("This method has no caching, use it sparingly.")]
        IEnumerable<Type> GetComponents(Type interfaceType);
    }
}
