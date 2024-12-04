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
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = database.ExecuteReader(database.Queries.GetLibraryMetaData, (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["name"] = CommonMetaData.Genre;
                                parameters["type"] = MetaDataItemType.Tag;
                                break;
                        }
                    }, transaction))
                    {
                        foreach (var record in reader)
                        {
                            yield return record.Get<string>("value");
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
