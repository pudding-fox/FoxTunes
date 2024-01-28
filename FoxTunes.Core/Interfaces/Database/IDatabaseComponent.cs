using FoxDb.Interfaces;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseComponent : IDatabase, IStandardComponent
    {
        IDatabaseSets Sets { get; }

        IDatabaseQueries Queries { get; }
    }
}
