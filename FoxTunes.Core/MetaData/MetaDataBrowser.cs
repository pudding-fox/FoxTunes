#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class MetaDataBrowser : StandardComponent, IMetaDataBrowser
    {
        public IDatabaseFactory DatabaseFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public IEnumerable<string> GetMetaDataNames()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                var query = database.QueryFactory.Build();
                var name = database.Tables.MetaDataItem.Column("Name");
                query.Output.AddColumn(name);
                query.Source.AddTable(database.Tables.MetaDataItem);
                query.Aggregate.AddColumn(name);
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = database.ExecuteReader(query, null, transaction))
                    {
                        foreach (var record in reader)
                        {
                            yield return record.Get<string>(name.Identifier);
                        }
                    }
                }
            }
        }

        public IEnumerable<MetaDataItem> GetMetaDatas(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType)
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
