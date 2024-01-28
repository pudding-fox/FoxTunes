using FoxDb.Interfaces;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseComponent : IDatabase, IStandardComponent
    {
        IDatabaseSets Sets { get; }

        IDatabaseTables Tables { get; }

        IDatabaseQueries Queries { get; }
    }
}
