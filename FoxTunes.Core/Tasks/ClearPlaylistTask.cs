using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearPlaylistTask : BackgroundTask
    {
        public const string ID = "D2F22C47-386F-4333-AD4F-693951C0E5A1";

        public ClearPlaylistTask()
            : base(ID)
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

        protected override async Task OnRun()
        {
            this.IsIndeterminate = true;
            await this.Clear();
            await this.SaveChanges();
        }

        private Task Clear()
        {
            this.Name = "Clearing playlist";
            Logger.Write(this, LogLevel.Debug, "Clearing playlist.");
            return this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => this.Playlist.PlaylistItemSet.Clear()));
        }

        private Task SaveChanges()
        {
            this.Name = "Saving changes";
            Logger.Write(this, LogLevel.Debug, "Saving changes to playlist.");
            return this.Database.Interlocked(async () => await this.Database.SaveChangesAsync());
        }
    }
}
