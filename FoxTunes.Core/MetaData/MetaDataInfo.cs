#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MetaDataInfo
    {
        public static IEnumerable<string> GetMetaDataNames(IDatabaseComponent database, ITransactionSource transaction = null)
        {
            var query = database.QueryFactory.Build();
            var name = database.Tables.MetaDataItem.Column("Name");
            query.Output.AddColumn(name);
            query.Source.AddTable(database.Tables.MetaDataItem);
            query.Aggregate.AddColumn(name);
            using (var reader = database.ExecuteReader(query, null, transaction))
            {
                foreach (var record in reader)
                {
                    yield return record.Get<string>(name.Identifier);
                }
            }
        }

        public static IDatabaseReader GetMetaData(IDatabaseComponent database, LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType, ITransactionSource transaction = null)
        {
            return database.ExecuteReader(database.Queries.GetLibraryHierarchyMetaData, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                        parameters["type"] = metaDataItemType;
                        break;
                }
            }, transaction);
        }
    }
}
