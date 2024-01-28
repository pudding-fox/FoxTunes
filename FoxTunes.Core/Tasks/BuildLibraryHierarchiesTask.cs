using FoxTunes.Utilities.Templates;
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

        protected override Task OnRun()
        {
            this.Name = "Building hierarchies";
            this.Description = "This could take a while, the UI may freeze :(";
            this.IsIndeterminate = true;
            using (var databaseContext = this.DataManager.CreateWriteContext())
            {
                using (var transaction = databaseContext.Connection.BeginTransaction())
                {
                    var query =
                        from metaDataItem in databaseContext.GetQuery<MetaDataItem>().Detach()
                        group metaDataItem by metaDataItem.Name into name
                        select name.Key;
                    var metaDataViewBuilder = new LibraryHierarchyViewBuilder(query);
                    var view = metaDataViewBuilder.TransformText();
                    using (var command = databaseContext.Connection.CreateCommand(view))
                    {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
            return Task.CompletedTask;
        }
    }
}
