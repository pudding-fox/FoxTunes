using FoxTunes.Interfaces;

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

        protected override void OnRun()
        {
            this.Clear();
            this.SaveChanges();
        }

        private void Clear()
        {
            this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => this.Playlist.Set.Clear()));
        }

        private void SaveChanges()
        {
            this.SetName("Saving changes");
            this.SetPosition(this.Count);
            this.Database.Interlocked(() => this.Database.SaveChanges());
        }
    }
}
