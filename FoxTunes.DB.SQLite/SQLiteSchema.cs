using FoxTunes.Interfaces;
using FoxTunes.Templates;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class SQLiteSchema
    {
        public static IEnumerable<string> GetFieldNames(IDatabase database, string table)
        {
            var query = new DatabaseQuery(new GetTableInfo(table).TransformText());
            using (var command = database.CreateCommand(query))
            {
                using (var reader = EnumerableDataReader.Create(command.ExecuteReader()))
                {
                    return reader.Select(element => element["name"]).OfType<string>().ToArray();
                }
            }
        }
    }
}
