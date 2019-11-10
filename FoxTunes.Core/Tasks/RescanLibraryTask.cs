using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
            var paths = await this.GetRoots();
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
                    Logger.Write(this, LogLevel.Debug, "Removing dead file: {0} => {1}", libraryItem.Id, libraryItem.FileName);
                    return true;
                }
                if (file.LastWriteTimeUtc > libraryItem.GetImportDate())
                {
                    Logger.Write(this, LogLevel.Debug, "Refreshing modified file: {0} => {1}", libraryItem.Id, libraryItem.FileName);
                    return true;
                }
                return false;
            });
            var action = new Func<IDatabaseSet<LibraryItem>, LibraryItem, Task>(async (set, libraryItem) =>
            {
                libraryItem.Status = LibraryItemStatus.Remove;
                //TODO: Writing to IDatabaseSet is not thread safe.
                Monitor.Enter(set);
                try
                {
                    await set.AddOrUpdateAsync(libraryItem);
                }
                finally
                {
                    Monitor.Exit(set);
                }
            });
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    using (var libraryUpdater = new LibraryUpdater(this.Database, predicate, action, this.Visible, transaction))
                    {
                        libraryUpdater.InitializeComponent(this.Core);
                        await this.WithSubTask(libraryUpdater,
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
    }
}
