using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class MetaDataBrowser : StandardComponent, IMetaDataBrowser
    {
        public IMetaDataCache MetaDataCache { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataCache = core.Components.MetaDataCache;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public IEnumerable<MetaDataItem> GetMetaDatas(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            var key = new MetaDataCacheKey(libraryHierarchyNode, metaDataItemType, metaDataItemName, this.LibraryHierarchyBrowser.Filter);
            return this.MetaDataCache.GetMetaDatas(key, () => this.GetMetaDatasCore(libraryHierarchyNode, metaDataItemType, metaDataItemName));
        }

        private IEnumerable<MetaDataItem> GetMetaDatasCore(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = this.GetReader(database, libraryHierarchyNode, metaDataItemType, metaDataItemName, transaction))
                    {
                        foreach (var record in reader)
                        {
                            yield return new MetaDataItem()
                            {
                                Name = metaDataItemName,
                                Type = metaDataItemType.GetValueOrDefault(),
                                Value = record.Get<string>("Value")
                            };
                        }
                    }
                }
            }
        }

        private IDatabaseReader GetReader(IDatabaseComponent database, LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName, ITransactionSource transaction)
        {
            return database.ExecuteReader(database.Queries.GetLibraryHierarchyMetaData(this.LibraryHierarchyBrowser.Filter), (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                        if (metaDataItemType.HasValue)
                        {
                            parameters["type"] = metaDataItemType.Value;
                        }
                        else
                        {
                            parameters["type"] = null;
                        }
                        if (!string.IsNullOrEmpty(metaDataItemName))
                        {
                            parameters["name"] = metaDataItemName;
                        }
                        else
                        {
                            parameters["name"] = null;
                        }
                        break;
                }
            }, transaction);
        }
    }
}
