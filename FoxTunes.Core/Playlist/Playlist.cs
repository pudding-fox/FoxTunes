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

        public IDatabaseSet<PlaylistItem> Set { get; private set; }

        public IDatabaseQuery<PlaylistItem> Query { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.Set = this.Database.GetSet<PlaylistItem>();
            this.Query = this.Database.GetQuery<PlaylistItem>();
            this.Query.Include("MetaDatas");
            this.Query.Include("Properties");
            base.InitializeComponent(core);
        }
    }
}