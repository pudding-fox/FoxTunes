using FoxTunes.Interfaces;

namespace FoxTunes
{
    [Component("D892408D-471F-4D97-83B8-5BEA4227D146", ComponentSlots.Playlist)]
    public class Playlist : StandardComponent, IPlaylist
    {
        public Playlist()
        {

        }

        public IDatabase Database { get; private set; }

        public IDatabaseSet<PlaylistItem> PlaylistItemSet { get; private set; }

        public IDatabaseQuery<PlaylistItem> PlaylistItemQuery { get; private set; }

        public IDatabaseSet<PlaylistColumn> PlaylistColumnSet { get; private set; }

        public IDatabaseQuery<PlaylistColumn> PlaylistColumnQuery { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.PlaylistItemSet = this.Database.GetSet<PlaylistItem>();
            this.PlaylistItemQuery = this.Database.GetQuery<PlaylistItem>();
            this.PlaylistItemQuery.Include("MetaDatas");
            this.PlaylistItemQuery.Include("Properties");
            this.PlaylistItemQuery.Include("Images");
            this.PlaylistColumnSet = this.Database.GetSet<PlaylistColumn>();
            this.PlaylistColumnQuery = this.Database.GetQuery<PlaylistColumn>();
            base.InitializeComponent(core);
        }
    }
}