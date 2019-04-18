using FoxDb.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddLibraryHierarchyNodeToPlaylistTask : PlaylistTaskBase
    {
        public AddLibraryHierarchyNodeToPlaylistTask(int sequence, LibraryHierarchyNode libraryHierarchyNode, bool clear)
            : base(sequence)
        {
            this.LibraryHierarchyNode = libraryHierarchyNode;
            this.Clear = clear;
        }

        public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

        public bool Clear { get; private set; }

        protected override async Task OnRun()
        {
            if (this.Clear)
            {
                await this.RemoveItems(PlaylistItemStatus.None);
            }
            await this.AddPlaylistItems();
            if (!this.Clear)
            {
                await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset);
            }
            await this.SequenceItems();
            await this.SetPlaylistItemsStatus(PlaylistItemStatus.None);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }

        private async Task AddPlaylistItems()
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    this.Offset = await this.Database.ExecuteScalarAsync<int>(this.Database.Queries.AddLibraryHierarchyNodeToPlaylist, (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["libraryHierarchyItemId"] = this.LibraryHierarchyNode.Id;
                                parameters["sequence"] = this.Sequence;
                                parameters["status"] = PlaylistItemStatus.Import;
                                break;
                        }
                    }, transaction);
                    transaction.Commit();
                }
            }))
            {
                await task.Run();
            }
        }
    }
}
