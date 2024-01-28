using FoxTunes.Interfaces;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class Playlist : StandardComponent, IPlaylist
    {
        public Playlist()
        {

        }

        public IDatabase Database { get; private set; }

        public IPersistableSet<PlaylistItem> Set { get; private set; }

        public ObservableCollection<PlaylistItem> Items
        {
            get
            {
                return this.Set.AsObservable();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.Set = this.Database.GetSet<PlaylistItem>();
            this.Set.LoadCollection(item => item.MetaDatas);
            this.Set.LoadCollection(item => item.Properties);
            base.InitializeComponent(core);
        }
    }
}