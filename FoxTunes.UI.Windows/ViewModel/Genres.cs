using FoxDb;
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
                    var set = database.Set<MetaDataItem>(transaction);
                    var query = set.AsQueryable()
                        .Where(metaDataItem => metaDataItem.Name == CommonMetaData.Genre)
                        .GroupBy(metaDataItem => metaDataItem.Value)
                        .Select(group => group.Key)
                        .OrderBy(name => name);
                    return query.ToArray();
                }
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Genres();
        }
    }
}
