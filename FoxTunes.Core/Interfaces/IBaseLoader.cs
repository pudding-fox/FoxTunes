using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBaseLoader<T> where T : IBaseComponent
    {
        IEnumerable<T> Load(ICore core);
    }
}
