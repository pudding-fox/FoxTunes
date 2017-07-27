using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public abstract class PlaylistTask : BackgroundTask
    {
        protected PlaylistTask(string id)
            : base(id)
        {

        }

        public IDatabase Database { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        protected virtual IEnumerable<PlaylistItem> OrderBy(IEnumerable<PlaylistItem> playlistItems)
        {
            var query =
                from playlistItem in playlistItems
                orderby
                    Path.GetDirectoryName(playlistItem.FileName),
                    playlistItem.MetaDatas.Value<int>(CommonMetaData.Track),
                    playlistItem.FileName
                select playlistItem;
            return query;
        }

        protected virtual void SaveChanges()
        {
            this.Database.SaveChanges();
        }
    }
}
