using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryUpdater : PopulatorBase
    {
        public const string ID = "00672118-A730-4CD8-8B0A-F3DA42712165";

        public LibraryUpdater(IDatabaseComponent database, Func<LibraryItem, bool> predicate, Func<IDatabaseSet<LibraryItem>, LibraryItem, Task> task, bool reportProgress, ITransactionSource transaction)
            : base(reportProgress)
        {
            this.Database = database;
            this.Predicate = predicate;
            this.Task = task;
            this.Transaction = transaction;
        }

        public IDatabaseComponent Database { get; private set; }

        public Func<LibraryItem, bool> Predicate { get; private set; }

        public Func<IDatabaseSet<LibraryItem>, LibraryItem, Task> Task { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public async Task Populate(CancellationToken cancellationToken)
        {
            var set = this.Database.Set<LibraryItem>(this.Transaction);

            if (this.ReportProgress)
            {
                await this.SetName("Updating library");
                await this.SetPosition(0);
                await this.SetCount(set.Count);
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            foreach (var libraryItem in set)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                if (this.Predicate(libraryItem))
                {
                    await this.Task(set, libraryItem);
                }

                if (this.ReportProgress)
                {
                    if (position % interval == 0)
                    {
#if NET40
                        this.Semaphore.Wait();
#else
                        await this.Semaphore.WaitAsync();
#endif
                        try
                        {
                            await this.SetDescription(new FileInfo(libraryItem.FileName).Name);
                            await this.SetPosition(position);
                        }
                        finally
                        {
                            this.Semaphore.Release();
                        }
                    }
                }
                Interlocked.Increment(ref position);
            }
        }
    }
}
