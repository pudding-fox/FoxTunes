using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IComponentLoader
    {
        IEnumerable<IBaseComponent> Load();
    }
}
