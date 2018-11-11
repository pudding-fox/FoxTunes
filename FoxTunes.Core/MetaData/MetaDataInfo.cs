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

        public static IEnumerable<MetaDataItem> GetMetaData(ICore core, IDatabaseComponent database, LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType, ITransactionSource transaction = null)
        {
            return database.ExecuteEnumerator<MetaDataItem>(database.Queries.GetLibraryHierarchyMetaDataItems, (parameters, phase) =>
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
