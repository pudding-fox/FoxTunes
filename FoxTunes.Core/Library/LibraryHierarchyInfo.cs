using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace FoxTunes
{
    public static class LibraryHierarchyInfo
    {
        public static IEnumerable<LibraryHierarchyLevel> GetLevels(ICore core, IDatabase database, LibraryHierarchy libraryHierarchy, IDbTransaction transaction = null)
        {
            return new RecordEnumerator<LibraryHierarchyLevel>(core, database, database.Queries.Select<LibraryHierarchyLevel>("libraryHierarchy_Id"), parameters =>
           {
               parameters["libraryHierarchy_Id"] = libraryHierarchy.Id;
           }, transaction);
        }
    }
}
