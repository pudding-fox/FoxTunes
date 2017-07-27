using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : PlaylistTask
    {
        public const string ID = "7B564369-A6A0-4BAF-8C99-08AF27908591";

        public AddPathsToPlaylistTask(IEnumerable<string> paths)
            : base(ID)
        {
            this.Paths = paths;
        }

        public IEnumerable<string> Paths { get; private set; }

        public IEnumerable<string> FileNames { get; private set; }

        public IPlaylist Playlist { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistItemFactory PlaylistItemFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistItemFactory = core.Factories.PlaylistItem;
            base.InitializeComponent(core);
        }

        protected override void OnRun()
        {
            this.EnumerateFiles();
            this.AddFiles();
            this.SaveChanges();
        }

        private void EnumerateFiles()
        {
            this.SetName("Getting file list");
            this.SetPosition(0);
            this.SetCount(this.Paths.Count());
            var fileNames = new List<string>();
            foreach (var path in this.Paths)
            {
                if (Directory.Exists(path))
                {
                    this.SetDescription(new DirectoryInfo(path).Name);
                    fileNames.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
                }
                else if (File.Exists(path))
                {
                    this.SetDescription(new FileInfo(path).Name);
                    fileNames.Add(path);
                }
                this.SetPosition(this.Position + 1);
            }
            this.FileNames = fileNames;
            this.SetPosition(this.Count);
        }

        private void AddFiles()
        {
            this.SetName("Processing files");
            this.SetPosition(0);
            this.SetCount(this.FileNames.Count());
            var query =
                from fileName in this.FileNames
                where this.PlaybackManager.IsSupported(fileName)
                select this.PlaylistItemFactory.Create(fileName);
            foreach (var playlistItem in this.OrderBy(query))
            {
                this.SetDescription(Path.GetFileName(playlistItem.FileName));
                this.ForegroundTaskRunner.Run(() => this.Playlist.Set.Add(playlistItem));
                this.SetPosition(this.Position + 1);
            }
            this.SetPosition(this.Count);
        }
    }
}
