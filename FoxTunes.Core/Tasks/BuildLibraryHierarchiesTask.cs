using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Data;
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

        protected override Task OnStarted()
        {
            this.Name = "Building hierarchies";
            this.Description = "Preparing";
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var metaDataNames = MetaDataInfo.GetMetaDataNames(this.Database, transaction);
                await this.Database.ExecuteAsync(this.Database.Queries.BeginBuildLibraryHierarchies, transaction);
                using (var reader = this.Database.ExecuteReader(this.Database.Queries.BuildLibraryHierarchies(metaDataNames), null, transaction))
                {
                    await this.AddHiearchies(reader, CancellationToken.None, transaction);
                    this.Description = "Finalizing";
                    this.IsIndeterminate = true;
                }
                await this.Database.ExecuteAsync(this.Database.Queries.EndBuildLibraryHierarchies, transaction);
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
        }

        private async Task AddHiearchies(IDatabaseReader reader, CancellationToken cancellationToken, ITransactionSource transaction)
        {
            using (var libraryHierarchyPopulator = new LibraryHierarchyPopulator(this.Database, true, transaction))
            {
                libraryHierarchyPopulator.InitializeComponent(this.Core);
                libraryHierarchyPopulator.NameChanged += (sender, e) => this.Name = libraryHierarchyPopulator.Name;
                libraryHierarchyPopulator.DescriptionChanged += (sender, e) => this.Description = libraryHierarchyPopulator.Description;
                libraryHierarchyPopulator.PositionChanged += (sender, e) => this.Position = libraryHierarchyPopulator.Position;
                libraryHierarchyPopulator.CountChanged += (sender, e) => this.Count = libraryHierarchyPopulator.Count;
                await libraryHierarchyPopulator.Populate(reader, cancellationToken, transaction);
            }
        }
    }
}
