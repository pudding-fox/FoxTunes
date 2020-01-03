using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddLibraryHierarchyNodesToPlaylistTask : PlaylistTaskBase
    {
        public AddLibraryHierarchyNodesToPlaylistTask(int sequence, IEnumerable<LibraryHierarchyNode> libraryHierarchyNodes, bool clear)
            : base(sequence)
        {
            this.LibraryHierarchyNodes = libraryHierarchyNodes;
            this.Clear = clear;
        }

        public override bool Visible
        {
            get
            {
                return this.LibraryHierarchyNodes.Count() > 1;
            }
        }

        public override bool Cancellable
        {
            get
            {
                return this.LibraryHierarchyNodes.Count() > 1;
            }
        }

        public IEnumerable<LibraryHierarchyNode> LibraryHierarchyNodes { get; private set; }

        public bool Clear { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement ShowFavorites { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.Configuration = core.Components.Configuration;
            this.ShowFavorites = this.Configuration.GetElement<BooleanConfigurationElement>(
                LibraryFavoritesBehaviourConfiguration.SECTION,
                LibraryFavoritesBehaviourConfiguration.SHOW_FAVORITES_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            if (this.Clear)
            {
                await this.RemoveItems(PlaylistItemStatus.None).ConfigureAwait(false);
            }
            await this.AddPlaylistItems().ConfigureAwait(false);
            if (!this.Clear)
            {
                await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset).ConfigureAwait(false);
            }
            await this.SequenceItems().ConfigureAwait(false);
            await this.SetPlaylistItemsStatus(PlaylistItemStatus.None).ConfigureAwait(false);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
        }

        private async Task AddPlaylistItems()
        {
            await this.SetName("Creating playlist").ConfigureAwait(false);
            await this.SetCount(this.LibraryHierarchyNodes.Count()).ConfigureAwait(false);
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    var query = default(IDatabaseQuery);
                    if (string.IsNullOrEmpty(this.LibraryHierarchyBrowser.Filter))
                    {
                        query = this.Database.Queries.AddLibraryHierarchyNodeToPlaylist;
                    }
                    else
                    {
                        query = this.Database.Queries.AddLibraryHierarchyNodeToPlaylistWithFilter;
                    }
                    var position = 0;
                    foreach (var libraryHierarchyNode in this.LibraryHierarchyNodes)
                    {
                        if (this.IsCancellationRequested)
                        {
                            break;
                        }
                        await this.SetDescription(libraryHierarchyNode.Value).ConfigureAwait(false);
                        await this.AddPlaylistItems(query, libraryHierarchyNode, transaction).ConfigureAwait(false);
                        await this.SetPosition(++position).ConfigureAwait(false);
                    }
                    transaction.Commit();
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        private async Task AddPlaylistItems(IDatabaseQuery query, LibraryHierarchyNode libraryHierarchyNode, ITransactionSource transaction)
        {
            this.Offset += await this.Database.ExecuteScalarAsync<int>(query, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                        parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                        parameters["sequence"] = this.Sequence;
                        parameters["status"] = PlaylistItemStatus.Import;
                        if (parameters.Contains("filter"))
                        {
                            parameters["filter"] = this.GetFilter();
                        }
                        if (this.ShowFavorites.Value)
                        {
                            parameters["favorite"] = true;
                        }
                        break;
                }
            }, transaction).ConfigureAwait(false);
        }

        private string GetFilter()
        {
            if (string.IsNullOrEmpty(this.LibraryHierarchyBrowser.Filter))
            {
                return null;
            }
            var builder = new StringBuilder();
            builder.Append('%');
            builder.Append(this.LibraryHierarchyBrowser.Filter.Replace(' ', '%'));
            builder.Append('%');
            return builder.ToString();
        }
    }
}
