using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : BackgroundTask
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

        public IDatabase Database { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistItemFactory = core.Factories.PlaylistItem;
            this.Database = core.Components.Database;
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
            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            var query =
                from fileName in this.FileNames
                where this.PlaybackManager.IsSupported(fileName)
                select this.PlaylistItemFactory.Create(fileName);
            foreach (var playlistItem in this.OrderBy(query))
            {
                this.ForegroundTaskRunner.Run(() => this.Playlist.Set.Add(playlistItem));
                if (position % interval == 0)
                {
                    this.SetDescription(Path.GetFileName(playlistItem.FileName));
                    this.SetPosition(position);
                }
                position++;
            }
            this.SetPosition(this.Count);
        }

        private void SaveChanges()
        {
            this.ForegroundTaskRunner.Run(() => this.Database.SaveChanges());
        }

        private IEnumerable<PlaylistItem> OrderBy(IEnumerable<PlaylistItem> playlistItems)
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
    }
}
