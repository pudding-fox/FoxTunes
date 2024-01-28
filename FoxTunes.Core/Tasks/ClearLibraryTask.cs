#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearLibraryTask : LibraryTaskBase
    {
        public const string ID = "08D1EDBA-B9FF-4327-88BE-0CDB6DD4531C";

        public ClearLibraryTask()
            : base(ID)
        {

        }

        protected virtual IEnumerable<ITableConfig> Tables
        {
            get
            {
                yield return this.Database.Config.Table("LibraryHierarchy_LibraryHierarchyItem", TableFlags.None);
                yield return this.Database.Config.Table("LibraryHierarchyItem_LibraryItem", TableFlags.None);
                yield return this.Database.Config.Table("LibraryHierarchyItems", TableFlags.None);
                yield return this.Database.Config.Table("LibraryItems", TableFlags.None);
                yield return this.Database.Config.Table("LibraryItem_MetaDataItem", TableFlags.None);
            }
        }

        protected override async Task OnRun()
        {
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Clearing library.");
            using (var transaction = this.Database.BeginTransaction())
            {
                foreach (var table in this.Tables)
                {
                    var query = this.Database.QueryFactory.Build();
                    query.Delete.Touch();
                    query.Source.AddTable(table);
                    await this.Database.ExecuteAsync(query, transaction);
                }
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
        }
    }
}
