using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FoxTunes
{
    public static class MetaDataInfo
    {
        public static IEnumerable<string> GetMetaDataNames(IDatabase database, IDbTransaction transaction = null)
        {
            var result = new List<string>();
            using (var reader = database.CreateReader(database.Queries.GetMetaDataNames, transaction))
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }
            return result;
        }

        public static IEnumerable<MetaDataItem> GetMetaData(ICore core, IDatabase database, PlaylistItem playlistItem, MetaDataItemType metaDataItemType, IDbTransaction transaction = null)
        {
            return new RecordEnumerator<MetaDataItem>(core, database, database.Queries.GetPlaylistMetaDataItems, parameters =>
            {
                parameters["playlistItemId"] = playlistItem.Id;
                parameters["type"] = metaDataItemType;
            }, transaction);
        }

        public static IEnumerable<MetaDataItem> GetMetaData(ICore core, IDatabase database, LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType, IDbTransaction transaction = null)
        {
            return new RecordEnumerator<MetaDataItem>(core, database, database.Queries.GetLibraryHierarchyMetaDataItems, parameters =>
            {
                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                parameters["type"] = metaDataItemType;
            }, transaction);
        }
    }
}
