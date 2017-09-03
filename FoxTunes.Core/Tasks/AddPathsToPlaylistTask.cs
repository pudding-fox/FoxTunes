using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "7B564369-A6A0-4BAF-8C99-08AF27908591";

        public AddPathsToPlaylistTask(int sequence, IEnumerable<string> paths)
            : base(ID)
        {
            this.Sequence = sequence;
            this.Paths = paths;
        }

        public int Sequence { get; private set; }

        public IEnumerable<string> Paths { get; private set; }

        public IEnumerable<string> FileNames { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistItemFactory PlaylistItemFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistItemFactory = core.Factories.PlaylistItem;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            this.EnumerateFiles();
            this.ShiftItems(this.Sequence,this.FileNames.Count());
            this.AddFiles();
            return this.SaveChanges();
        }

        private void EnumerateFiles()
        {
            this.Name = "Getting file list";
            this.Position = 0;
            this.Count = this.Paths.Count();
            var fileNames = new List<string>();
            foreach (var path in this.Paths)
            {
                Logger.Write(this, LogLevel.Debug, "Enumerating files in path: {0}", path);
                if (Directory.Exists(path))
                {
                    this.Description = new DirectoryInfo(path).Name;
                    fileNames.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
                }
                else if (File.Exists(path))
                {
                    this.Description = new FileInfo(path).Name;
                    fileNames.Add(path);
                }
                this.Position = this.Position + 1;
            }
            Logger.Write(this, LogLevel.Debug, "Enumerated {0} files.", fileNames.Count);
            this.FileNames = fileNames;
            this.Position = this.Count;
        }

        private void AddFiles()
        {
            this.Name = "Processing files";
            this.Position = 0;
            this.Count = this.FileNames.Count();
            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            Logger.Write(this, LogLevel.Debug, "Converting file names to playlist items.");
            var query =
                from fileName in this.FileNames
                where this.PlaybackManager.IsSupported(fileName)
                select this.PlaylistItemFactory.Create(this.Sequence++, fileName);
            foreach (var playlistItem in this.OrderBy(query))
            {
                Logger.Write(this, LogLevel.Debug, "Adding item to playlist: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => this.Playlist.PlaylistItemSet.Add(playlistItem)));
                if (position % interval == 0)
                {
                    this.Description = Path.GetFileName(playlistItem.FileName);
                    this.Position = position;
                }
                position++;
            }
            this.Position = this.Count;
        }

        private Task SaveChanges()
        {
            this.Name = "Saving changes";
            this.Position = this.Count;
            Logger.Write(this, LogLevel.Debug, "Saving changes to playlist.");
            return this.Database.Interlocked(async () => await this.Database.SaveChangesAsync());
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
