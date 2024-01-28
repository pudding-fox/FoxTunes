using FoxDb.Interfaces;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseComponent : IDatabase, IStandardComponent
    {
        IDatabaseTables Tables { get; }

        IDatabaseQueries Queries { get; }

        IDatabaseComponent New();
    }
}
