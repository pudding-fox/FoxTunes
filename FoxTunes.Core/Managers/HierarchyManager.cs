using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class HierarchyManager : StandardManager, IHierarchyManager
    {
        public HierarchyManagerState State
        {
            get
            {
                if (global::FoxTunes.BackgroundTask.Active.Any(backgroundTask =>
                {
                    var type = backgroundTask.GetType();
                    return type == typeof(BuildLibraryHierarchiesTask) || type == typeof(ClearLibraryHierarchiesTask);
                }))
                {
                    return HierarchyManagerState.Updating;
                }
                return HierarchyManagerState.None;
            }
        }

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private bool _CanNavigate { get; set; }

        public bool CanNavigate
        {
            get
            {
                return this._CanNavigate;
            }
            set
            {
                this._CanNavigate = value;
                this.OnCanNavigateChanged();
            }
        }

        protected virtual void OnCanNavigateChanged()
        {
            if (this.CanNavigateChanged != null)
            {
                this.CanNavigateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CanNavigate");
        }

        public event EventHandler CanNavigateChanged;

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LibraryManager = core.Managers.Library;
            this.DatabaseFactory = core.Factories.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            //TODO: Bad .Wait().
            this.Refresh().Wait();
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task<bool> HasItems()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.ExecuteScalarAsync<bool>(database.QueryFactory.Build().With(query1 =>
                    {
                        query1.Output.AddCase(
                            query1.Output.CreateCaseCondition(
                                query1.Output.CreateFunction(
                                    QueryFunction.Exists,
                                    query1.Output.CreateSubQuery(
                                        database.QueryFactory.Build().With(query2 =>
                                        {
                                            query2.Output.AddOperator(QueryOperator.Star);
                                            query2.Source.AddTable(database.Tables.LibraryHierarchyNode);
                                        })
                                    )
                                ),
                                query1.Output.CreateConstant(1)
                            ),
                            query1.Output.CreateCaseCondition(
                                query1.Output.CreateConstant(0)
                            )
                        );
                    }), transaction).ConfigureAwait(false);
                }
            }
        }

        public async Task Refresh()
        {
            Logger.Write(this, LogLevel.Debug, "Refresh was requested, determining whether navigation is possible.");
            this.CanNavigate = this.DatabaseFactory != null && await this.HasItems().ConfigureAwait(false);
            if (this.CanNavigate)
            {
                Logger.Write(this, LogLevel.Debug, "Navigation is possible.");
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Navigation is not possible, library is empty.");
            }
        }

        public async Task Build(LibraryItemStatus? status)
        {
            if (this.LibraryManager.State.HasFlag(LibraryManagerState.Updating))
            {
                return;
            }
            using (var task = new BuildLibraryHierarchiesTask(status))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task Clear(LibraryItemStatus? status, bool signal)
        {
            if (this.LibraryManager.State.HasFlag(LibraryManagerState.Updating))
            {
                return;
            }
            using (var task = new ClearLibraryHierarchiesTask(status, signal))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual Task OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new BackgroundTaskEventArgs(backgroundTask);
            this.BackgroundTask(this, e);
            return e.Complete();
        }

        public event BackgroundTaskEventHandler BackgroundTask;

        public void InitializeDatabase(IDatabaseComponent database, DatabaseInitializeType type)
        {
            if (!type.HasFlag(DatabaseInitializeType.Library))
            {
                return;
            }
            var scriptingRuntime = ComponentRegistry.Instance.GetComponent<IScriptingRuntime>();
            if (scriptingRuntime == null)
            {
                return;
            }
            using (var transaction = database.BeginTransaction())
            {
                var set = database.Set<LibraryHierarchy>(transaction);
                set.Clear();
                set.Add(new LibraryHierarchy()
                {
                    Name = "Artist/Album/Title",
                    Type = LibraryHierarchyType.Script,
                    Sequence = 0,
                    Enabled = true,
                    Levels = new ObservableCollection<LibraryHierarchyLevel>()
                    {
                        new LibraryHierarchyLevel() { Sequence = 0, Script = scriptingRuntime.CoreScripts.Artist },
                        new LibraryHierarchyLevel() { Sequence = 1, Script = scriptingRuntime.CoreScripts.Year_Album },
                        new LibraryHierarchyLevel() { Sequence = 2, Script = scriptingRuntime.CoreScripts.Disk_Track_Title }
                    }
                });
                set.Add(new LibraryHierarchy()
                {
                    Name = "Genre/Album/Title",
                    Type = LibraryHierarchyType.Script,
                    Sequence = 1,
                    Enabled = false,
                    Levels = new ObservableCollection<LibraryHierarchyLevel>()
                    {
                        new LibraryHierarchyLevel() { Sequence = 0, Script = scriptingRuntime.CoreScripts.Genre },
                        new LibraryHierarchyLevel() { Sequence = 1, Script = scriptingRuntime.CoreScripts.Year_Album },
                        new LibraryHierarchyLevel() { Sequence = 2, Script = scriptingRuntime.CoreScripts.Disk_Track_Title }
                    }
                });
                set.Add(new LibraryHierarchy()
                {
                    Name = "Folder Structure",
                    Type = LibraryHierarchyType.FileSystem,
                    Sequence = 2,
                    Enabled = false
                });
                transaction.Commit();
            }
        }
    }
}
