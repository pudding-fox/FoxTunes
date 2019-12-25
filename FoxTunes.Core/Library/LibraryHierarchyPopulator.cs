using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FoxTunes
{
    public class LibraryHierarchyPopulator : PopulatorBase
    {
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
            this.Roots = new Lazy<IEnumerable<string>>(() => this.GetRoots());
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

        public Lazy<IEnumerable<string>> Roots { get; private set; }

        public string Current { get; private set; }

        private volatile int position = 0;

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
                if (this.Count <= 100)
                {
                    this.Timer.Interval = FAST_INTERVAL;
                }
                else if (this.Count < 1000)
                {
                    this.Timer.Interval = NORMAL_INTERVAL;
                }
                else
                {
                    this.Timer.Interval = LONG_INTERVAL;
                }
                this.Timer.Start();
            }

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
                        this.Current = record["FileName"] as string;
                        Interlocked.Increment(ref this.position);
                    }
                }, cancellationToken, this.ParallelOptions);
            }
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
            var value = record.Get<string>("FileName");
            //This removes any matching library roots from the path.
            var normalized = value.Replace(this.Roots.Value, string.Empty, true, true);
            var segments = normalized.Split(
                new[] { Path.DirectorySeparatorChar.ToString() },
                StringSplitOptions.RemoveEmptyEntries
            ).ToArray();
            return segments;
        }

        private IScriptingContext GetOrAddContext()
        {
            if (this.Contexts.IsValueCreated)
            {
                return this.Contexts.Value;
            }
            return this.Contexts.Value = this.ScriptingRuntime.CreateContext();
        }

        protected override async void OnElapsed(object sender, ElapsedEventArgs e)
        {
            var count = this.position - this.Position;
            if (count != 0)
            {
                var eta = this.GetEta(count);
                await this.SetName(string.Format("Populating library hierarchies: {0} remaining @ {1} items/s", eta, count));
                if (this.Current != null)
                {
                    await this.SetDescription(new FileInfo(this.Current).Name);
                }
                await this.SetPosition(position);
            }
            base.OnElapsed(sender, e);
        }

        protected IEnumerable<string> GetRoots()
        {
            var roots = this.Database.Set<LibraryRoot>(this.Transaction).Select(
                libraryRoot => libraryRoot.DirectoryName
            ).ToArray();
            //If a single folder has been added to the library then we create root nodes for each sub folder.
            //Otherwise we create a root node for each one.
            if (roots.Length > 1)
            {
                //We try to project the parent folder (of the root).
                //If it's empty then the drive letter will be used.
                roots = roots.Select(root =>
                {
                    var parent = Path.GetDirectoryName(root);
                    if (!string.IsNullOrEmpty(parent))
                    {
                        return parent;
                    }
                    return root;
                    //De-duplicate and sort. Sorting is for performance, it means a match can be made in less iterations (potentially).
                    //See GetPathSegments.
                }).Distinct().OrderBy(root => root).ToArray();
            }
            //TODO: We don't handle cases where a root exists within another.
            return roots;
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
