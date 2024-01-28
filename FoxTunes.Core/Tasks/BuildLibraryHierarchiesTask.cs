using FoxDb.Interfaces;
using FoxTunes.Interfaces;
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

        protected override Task OnStarted()
        {
            this.Name = "Building hierarchies";
            this.Description = "Preparing";
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            using (var transaction = this.Database.BeginTransaction())
            {
                var metaDataNames = MetaDataInfo.GetMetaDataNames(this.Database, transaction);
                using (var reader = this.Database.ExecuteReader(this.Database.Queries.LibraryHierarchyBuilder(metaDataNames), null, transaction))
                {
                    this.AddHiearchies(reader, transaction);
                    this.Description = "Finalizing";
                    this.IsIndeterminate = true;
                }
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
        }

        private void AddHiearchies(IDatabaseReader reader, ITransactionSource transaction)
        {
            using (var libraryHierarchyPopulator = new LibraryHierarchyPopulator(this.Database, true, transaction))
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
