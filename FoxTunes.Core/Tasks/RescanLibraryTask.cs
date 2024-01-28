using FoxDb;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class RescanLibraryTask : LibraryTaskBase
    {
        public RescanLibraryTask()
            : base()
        {

        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        protected override Task OnStarted()
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            await this.RemoveHierarchies();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
            await this.SetLibraryItemsStatus(libraryItem => !File.Exists(libraryItem.FileName), LibraryItemStatus.Remove);
            await this.RemoveItems(LibraryItemStatus.Remove);
            await this.AddPaths(this.GetLibraryDirectories().ToArray());
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
        }

        protected virtual IEnumerable<string> GetLibraryDirectories()
        {
            var table = this.Database.Tables.LibraryItem;
            var column = table.GetColumn(ColumnConfig.By("DirectoryName", ColumnFlags.None));
            var query = this.Database.QueryFactory.Build();
            query.Output.AddColumn(column);
            query.Source.AddTable(table);
            query.Aggregate.AddColumn(column);
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var reader = this.Database.ExecuteReader(query, null, transaction))
                {
                    foreach (var record in reader)
                    {
                        yield return record.Get<string>(column.Identifier);
                    }
                }
            }
        }
    }
}
