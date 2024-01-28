using FoxTunes.Interfaces;
using FoxTunes.Utilities.Templates;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BuildLibraryHierarchiesTask : LibraryTaskBase
    {
        public const string ID = "B6AF297E-F334-481D-8D60-BD5BE5935BD9";

        public BuildLibraryHierarchiesTask()
            : base(ID)
        {
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            this.Name = "Building hierarchies";
            this.Description = "Preparing";
            this.IsIndeterminate = true;
            using (var databaseContext = this.DataManager.CreateWriteContext())
            {
                using (var transaction = databaseContext.Connection.BeginTransaction())
                {
                    var metaDataNames =
                        from metaDataItem in databaseContext.GetQuery<MetaDataItem>().Detach()
                        group metaDataItem by metaDataItem.Name into name
                        select name.Key;
                    var libraryHierarchyBuilder = new LibraryHierarchyBuilder(metaDataNames);
                    using (var command = databaseContext.Connection.CreateCommand(libraryHierarchyBuilder.TransformText()))
                    {
                        command.Transaction = transaction;
                        using (var reader = EnumerableDataReader.Create(command.ExecuteReader()))
                        {
                            this.AddHiearchies(databaseContext, transaction, reader);
                            this.Description = "Finalizing";
                            this.IsIndeterminate = true;
                        }
                    }
                    transaction.Commit();
                }
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
            return Task.CompletedTask;
        }

        private void AddHiearchies(IDatabaseContext databaseContext, IDbTransaction transaction, EnumerableDataReader reader)
        {
            using (var libraryHierarchyPopulator = new LibraryHierarchyPopulator(databaseContext, transaction))
            {
                libraryHierarchyPopulator.InitializeComponent(this.Core);
                libraryHierarchyPopulator.NameChanged += (sender, e) => this.Name = libraryHierarchyPopulator.Name;
                libraryHierarchyPopulator.DescriptionChanged += (sender, e) => this.Description = libraryHierarchyPopulator.Description;
                libraryHierarchyPopulator.PositionChanged += (sender, e) => this.Position = libraryHierarchyPopulator.Position;
                libraryHierarchyPopulator.CountChanged += (sender, e) => this.Count = libraryHierarchyPopulator.Count;
                libraryHierarchyPopulator.Populate(reader);
            }
        }
    }
}
