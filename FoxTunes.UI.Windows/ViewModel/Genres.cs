using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Genres : ViewModelBase
    {
        public IEnumerable<string> Names { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            this.Names = this.GetNames(core).ToArray();
            base.InitializeComponent(core);
        }

        protected virtual IEnumerable<string> GetNames(ICore core)
        {
            using (var database = core.Factories.Database.Create())
            {
#pragma warning disable CS0612 
                var name = database.Tables.MetaDataItem.Column("Name");
                var value = database.Tables.MetaDataItem.Column("Value");
#pragma warning restore CS0612
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var builder = database.QueryFactory.Build();
                    builder.Output.AddColumn(value);
                    builder.Filter.AddColumn(name);
                    builder.Source.AddTable(database.Tables.MetaDataItem);
                    builder.Aggregate.AddColumn(value);
                    builder.Sort.AddColumn(value);
                    var query = builder.Build();
                    using (var reader = database.ExecuteReader(query, (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters[name] = CommonMetaData.Genre;
                                break;
                        }
                    }, transaction))
                    {
                        foreach (var record in reader)
                        {
                            yield return record.Get<string>(value);
                        }
                    }
                }
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Genres();
        }
    }
}
