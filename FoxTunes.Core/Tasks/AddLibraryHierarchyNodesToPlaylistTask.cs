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
            await this.SetName("Creating playlist");
            await this.SetCount(this.LibraryHierarchyNodes.Count());
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
                        await this.SetDescription(libraryHierarchyNode.Value);
                        await this.AddPlaylistItems(query, libraryHierarchyNode, transaction);
                        await this.SetPosition(++position);
                    }
                    transaction.Commit();
                }
            }))
            {
                await task.Run();
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
            }, transaction);
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
