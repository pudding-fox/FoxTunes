using FoxDb.Interfaces;
using System.Data;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseComponent : IDatabase, IBaseComponent
    {
        IsolationLevel PreferredIsolationLevel { get; }

        IDatabaseTables Tables { get; }

        IDatabaseQueries Queries { get; }
    }
}
