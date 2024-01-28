#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MetaDataInfo
    {
        public static string BitRateDescription(int rate)
        {
            return string.Format("{0} kb/s", rate);
        }

        public static string SampleRateDescription(int rate)
        {
            var a = default(float);
            var suffix = default(string);
            if (rate < 1000000)
            {
                a = rate / 1000f;
                suffix = "k";
            }
            else
            {
                a = rate / 1000000f;
                suffix = "m";
            }
            return string.Format("{0:0.#}{1}Hz", a, suffix);
        }

        public static string ChannelDescription(int channels)
        {
            switch (channels)
            {
                case 1:
                    return "mono";
                case 2:
                    return "stereo";
                case 4:
                    return "quad";
                case 6:
                    return "5.1";
                case 8:
                    return "7.1";
            }
            return string.Format("{0} channels", channels);
        }

        public static string SampleDescription(int depth)
        {
            switch (depth)
            {
                case 0:
                    //It's as good a guess as any.
                    depth = 16;
                    break;
                case 1:
                    return "DSD";
            }
            return string.Format("{0} bit", depth);
        }

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

        public static IDatabaseReader GetMetaData(IDatabaseComponent database, LibraryItem libraryItem, MetaDataItemType metaDataItemType, ITransactionSource transaction = null)
        {
            return database.ExecuteReader(database.Queries.GetLibraryMetaData, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["libraryItemId"] = libraryItem.Id;
                        parameters["type"] = metaDataItemType;
                        break;
                }
            }, transaction);
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
