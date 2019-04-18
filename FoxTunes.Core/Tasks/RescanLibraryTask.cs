using FoxDb;
using FoxDb.Interfaces;
using System;
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

        public override bool Cancellable
        {
            get
            {
                return true;
            }
        }

        protected override async Task OnStarted()
        {
            await this.SetName("Getting file list");
            await this.SetIsIndeterminate(true);
            await base.OnStarted();
        }

        protected override async Task OnRun()
        {
            var paths = this.GetLibraryDirectories().ToArray();
            await this.RescanLibrary();
            await this.RemoveHierarchies(LibraryItemStatus.Remove);
            await this.RemoveItems(LibraryItemStatus.Remove);
            await this.AddPaths(paths, true);
        }

        protected virtual async Task RescanLibrary()
        {
            var predicate = new Func<LibraryItem, bool>(libraryItem =>
            {
                var file = new FileInfo(libraryItem.FileName);
                if (!file.Exists)
                {
                    return true;
                }
                if (file.LastWriteTimeUtc > libraryItem.GetImportDate())
                {
                    return true;
                }
                return false;
            });
            var action = new Func<IDatabaseSet<LibraryItem>, LibraryItem, Task>((set, libraryItem) =>
            {
                libraryItem.Status = LibraryItemStatus.Remove;
                return set.AddOrUpdateAsync(libraryItem);
            });
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    using (var libraryUpdater = new LibraryUpdater(this.Database, predicate, action, this.Visible, transaction))
                    {
                        libraryUpdater.InitializeComponent(this.Core);
                        await this.WithPopulator(libraryUpdater,
                            async () => await libraryUpdater.Populate(cancellationToken)
                        );
                    }
                    transaction.Commit();
                }
            }))
            {
                await task.Run();
            }
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
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
                        if (this.IsCancellationRequested)
                        {
                            break;
                        }
                        yield return record.Get<string>(column.Identifier);
                    }
                }
            }
        }
    }
}
