#pragma warning disable 612, 618
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class MetaDataBrowser : StandardComponent, IMetaDataBrowser
    {
        public IMetaDataCache MetaDataCache { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataCache = core.Components.MetaDataCache;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public IEnumerable<MetaDataItem> GetMetaDatas(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType)
        {
            var key = new MetaDataCacheKey(libraryHierarchyNode, metaDataItemType);
            return this.MetaDataCache.GetMetaDatas(key, () => this.GetMetaDatasCore(libraryHierarchyNode, metaDataItemType));
        }


        private IEnumerable<MetaDataItem> GetMetaDatasCore(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = database.ExecuteReader(database.Queries.GetLibraryHierarchyMetaData, (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                                parameters["type"] = metaDataItemType;
                                break;
                        }
                    }, transaction))
                    {
                        foreach (var record in reader)
                        {
                            yield return new MetaDataItem()
                            {
                                Value = record.Get<string>("Value")
                            };
                        }
                    }
                }
            }
        }
    }
}
