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
    public class LibraryHierarchyPopulator : PopulatorBase
    {
        public readonly object SyncRoot = new object();

        public LibraryHierarchyPopulator(IDatabaseComponent database, bool reportProgress, ITransactionSource transaction)
            : base(reportProgress)
        {
            this.Database = database;
            this.Transaction = transaction;
#if NET40
            this.Contexts = new TrackingThreadLocal<IScriptingContext>();
#else
            this.Contexts = new ThreadLocal<IScriptingContext>(true);
#endif
            this.Writer = new LibraryHierarchyWriter(this.Database, this.Transaction);
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

#if NET40
        private TrackingThreadLocal<IScriptingContext> Contexts { get; set; }
#else
        private ThreadLocal<IScriptingContext> Contexts { get; set; }
#endif

        private LibraryHierarchyWriter Writer { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        public async Task Populate(LibraryItemStatus? status, CancellationToken cancellationToken, ITransactionSource transaction = null)
        {
            var metaDataNames = MetaDataInfo.GetMetaDataNames(this.Database, transaction).ToArray();

            var libraryHierarchies = this.GetHierarchies(transaction);
            var libraryHierarchyLevels = this.GetLevels(libraryHierarchies);

            if (this.ReportProgress)
            {
                await this.SetName("Populating library hierarchies");
                await this.SetPosition(0);
                await this.SetCount(await this.GetCount(status, transaction));
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            using (var reader = this.Database.ExecuteReader(this.Database.Queries.BuildLibraryHierarchies(metaDataNames), (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["status"] = status;
                        break;
                }
            }, transaction))
            {
                await AsyncParallel.ForEach(reader, async record =>
                {
                    foreach (var libraryHierarchy in libraryHierarchies)
                    {
                        await this.Populate(record, libraryHierarchy, libraryHierarchyLevels[libraryHierarchy]);
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
                                await this.SetDescription(new FileInfo(record["FileName"] as string).Name);
                                await this.SetPosition(position);
                            }
                            finally
                            {
                                this.Semaphore.Release();
                            }
                        }
                    }
                    Interlocked.Increment(ref position);
                }, cancellationToken, this.ParallelOptions);
            }

            await this.Cleanup(libraryHierarchies);
        }

        private async Task Populate(IDatabaseReaderRecord record, LibraryHierarchy libraryHierarchy, LibraryHierarchyLevel[] libraryHierarchyLevels)
        {
            var parentId = default(int?);
            switch (libraryHierarchy.Type)
            {
                case LibraryHierarchyType.Script:
                    for (int a = 0, b = libraryHierarchyLevels.Length - 1; a <= b; a++)
                    {
                        parentId = await this.Populate(record, libraryHierarchy, libraryHierarchyLevels[a], parentId, a == b);
                    }
                    break;
                case LibraryHierarchyType.FileSystem:
                    var pathSegments = this.GetPathSegments(record);
                    for (int a = 0, b = pathSegments.Length - 1; a <= b; a++)
                    {
                        parentId = await this.Populate(record, libraryHierarchy, pathSegments[a], parentId, a == b);
                    }
                    break;
            }
        }

        private Task<int> Populate(IDatabaseReaderRecord record, LibraryHierarchy libraryHierarchy, LibraryHierarchyLevel libraryHierarchyLevel, int? parentId, bool isLeaf)
        {
            var value = this.ExecuteScript(record, libraryHierarchyLevel.Script);
            return this.Populate(record, libraryHierarchy, value, parentId, isLeaf);
        }

        private async Task<int> Populate(IDatabaseReaderRecord record, LibraryHierarchy libraryHierarchy, string value, int? parentId, bool isLeaf)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync();
#endif
            try
            {
                return await this.Writer.Write(libraryHierarchy, record.Get<int>("Id"), parentId, value, isLeaf);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        private LibraryHierarchy[] GetHierarchies(ITransactionSource transaction)
        {
            var queryable = this.Database.AsQueryable<LibraryHierarchy>(transaction);
            return queryable.Where(libraryHierarchy => libraryHierarchy.Enabled).ToArray();
        }

        private IDictionary<LibraryHierarchy, LibraryHierarchyLevel[]> GetLevels(IEnumerable<LibraryHierarchy> libraryHierarchies)
        {
            return libraryHierarchies.ToDictionary(
                libraryHierarchy => libraryHierarchy,
                libraryHierarchy => libraryHierarchy.Levels.OrderBy(libraryHierarchyLevel => libraryHierarchyLevel.Sequence).ToArray()
            );
        }

        private async Task<int> GetCount(LibraryItemStatus? status, ITransactionSource transaction)
        {
            if (status.HasValue)
            {
                var queryable = this.Database.AsQueryable<LibraryItem>(transaction);
                return queryable.Count(libraryItem => libraryItem.Status == status.Value && libraryItem.MetaDatas.Any());
            }
            var set = this.Database.Set<LibraryItem>(transaction);
            return await set.CountAsync;
        }

        private string ExecuteScript(IDatabaseReaderRecord record, string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                return string.Empty;
            }
            var fileName = record.Get<string>("FileName");
            var metaData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (var a = 0; true; a++)
            {
                var keyName = string.Format("Key_{0}", a);
                if (!record.Contains(keyName))
                {
                    break;
                }
                var key = record.Get<string>(keyName).ToLower();
                var valueName = string.Format("Value_{0}_Value", a);
                var value = record.Get<string>(valueName);
                metaData.Add(key, value);
            }
            var context = this.GetOrAddContext();
            context.SetValue("fileName", fileName);
            context.SetValue("tag", metaData);
            try
            {
                return Convert.ToString(context.Run(script));
            }
            catch (ScriptingException e)
            {
                return e.Message;
            }
        }

        private string[] GetPathSegments(IDatabaseReaderRecord record)
        {
            var fileName = record.Get<string>("FileName");
            return fileName.Split(
                new[] { Path.DirectorySeparatorChar.ToString() },
                StringSplitOptions.RemoveEmptyEntries
            ).Skip(1).ToArray();
        }

        private IScriptingContext GetOrAddContext()
        {
            if (this.Contexts.IsValueCreated)
            {
                return this.Contexts.Value;
            }
            return this.Contexts.Value = this.ScriptingRuntime.CreateContext();
        }

        private async Task Cleanup(IEnumerable<LibraryHierarchy> libraryHierarchies)
        {
            foreach (var libraryHierarchy in libraryHierarchies)
            {
                switch (libraryHierarchy.Type)
                {
                    case LibraryHierarchyType.FileSystem:
                        await this.Cleanup(libraryHierarchy);
                        break;
                }
            }
        }

        private Task Cleanup(LibraryHierarchy libraryHierarchy)
        {
            return this.Database.ExecuteAsync(
                this.Database.Queries.CleanupLibraryHierarchyNodes, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["libraryHierarchyId"] = libraryHierarchy.Id;
                            break;
                    }
                }, this.Transaction);
        }

        protected override void OnDisposing()
        {
            foreach (var context in this.Contexts.Values)
            {
                context.Dispose();
            }
            this.Contexts.Dispose();
            this.Writer.Dispose();
            base.OnDisposing();
        }
    }
}
