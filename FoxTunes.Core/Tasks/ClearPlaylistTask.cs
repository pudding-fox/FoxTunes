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
            await this.Clear();
            await this.SaveChanges();
        }

        private Task Clear()
        {
            this.Name = "Clearing playlist";
            return this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => this.Playlist.Set.Clear()));
        }

        private Task SaveChanges()
        {
            this.Name = "Saving changes";
            this.Position = this.Count;
            return this.Database.Interlocked(() => this.Database.SaveChangesAsync());
        }
    }
}
