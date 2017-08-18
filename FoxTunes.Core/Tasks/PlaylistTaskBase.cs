using FoxTunes.Interfaces;
using System.Linq;

namespace FoxTunes
{
    public abstract class PlaylistTaskBase : BackgroundTask
    {
        protected PlaylistTaskBase(string id, bool visible = true) : base(id, visible)
        {
        }

        public IPlaylist Playlist { get; private set; }

        public IDatabase Database { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        protected virtual void ShiftItems(int sequence, int offset)
        {
            Logger.Write(this, LogLevel.Debug, "Shifting playlist items from {0}", sequence);
            var query =
                from playlistItem in this.Playlist.Query
                where playlistItem.Sequence >= sequence
                orderby playlistItem.Sequence
                select playlistItem;
            foreach (var playlistItem in query)
            {
                Logger.Write(this, LogLevel.Debug, "Shifting playlist item: {0} => {1} => {2} => {3}", playlistItem.Id, playlistItem.FileName, playlistItem.Sequence, playlistItem.Sequence + offset);
                playlistItem.Sequence = playlistItem.Sequence + offset;
                this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => this.Playlist.Set.Update(playlistItem)));
            }
        }
    }
}
