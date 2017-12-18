using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseQuery
    {
        string CommandText { get; }

        IEnumerable<string> ParameterNames { get; }
    }
}
