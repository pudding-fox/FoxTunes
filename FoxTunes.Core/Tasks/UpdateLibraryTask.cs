using System.Threading.Tasks;

namespace FoxTunes
{
    public class UpdateLibraryTask : LibraryTaskBase
    {
        public UpdateLibraryTask(LibraryItemStatus status) : base()
        {
            this.Status = status;
        }

        public LibraryItemStatus Status { get; private set; }

        protected override Task OnRun()
        {
            return SetLibraryItemsStatus(this.Database, this.Status);
        }
    }
}
