using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MetaDataInfo
    {
        public static IEnumerable<string> GetMetaDataNames(IDatabaseComponent database, ITransactionSource transaction = null)
        {
            using (var reader = database.ExecuteReader(database.Queries.GetMetaDataNames, null, transaction))
            {
                foreach (var record in reader)
                {
                    yield return record.Get<string>("Name");
                }
            }
        }

        public static IEnumerable<MetaDataItem> GetMetaData(ICore core, IDatabaseComponent database, LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType, ITransactionSource transaction = null)
        {
            return database.ExecuteEnumerator<MetaDataItem>(database.Queries.GetLibraryHierarchyMetaDataItems, parameters =>
            {
                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                parameters["type"] = metaDataItemType;
            }, transaction);
        }
    }
}
