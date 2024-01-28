using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IManagerLoader
    {
        IEnumerable<IBaseManager> Load();
    }
}
