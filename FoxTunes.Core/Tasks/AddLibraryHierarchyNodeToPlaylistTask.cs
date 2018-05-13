using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddLibraryHierarchyNodeToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "4E0DD392-1138-4DA8-84C2-69B27D1E34EA";

        public AddLibraryHierarchyNodeToPlaylistTask(int sequence, LibraryHierarchyNode libraryHierarchyNode, bool clear) : base(ID, sequence)
        {
            this.LibraryHierarchyNode = libraryHierarchyNode;
            this.Clear = clear;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

        public bool Clear { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            using (var transaction = this.Database.BeginTransaction())
            {
                if (this.Clear)
                {
                    this.ClearItems(transaction);
                }
                this.AddPlaylistItems(transaction);
                this.ShiftItems(transaction);
                this.SequenceItems(transaction);
                this.SetPlaylistItemsStatus(transaction);
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }

        private void AddPlaylistItems(ITransactionSource transaction)
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            this.Offset = this.Database.ExecuteScalar<int>(this.Database.Queries.AddLibraryHierarchyNodeToPlaylist, parameters =>
            {
                parameters["libraryHierarchyItemId"] = this.LibraryHierarchyNode.Id;
                parameters["sequence"] = this.Sequence;
                parameters["status"] = PlaylistItemStatus.Import;
            }, transaction);
        }
    }
}
