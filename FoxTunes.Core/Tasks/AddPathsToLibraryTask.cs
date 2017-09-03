using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToLibraryTask : BackgroundTask
    {
        public const string ID = "972222C8-8F6E-44CF-8EBE-DA4FCFD7CD80";

        public AddPathsToLibraryTask(IEnumerable<string> paths)
            : base(ID)
        {
            this.Paths = paths;
        }

        public IEnumerable<string> Paths { get; private set; }

        public IEnumerable<string> FileNames { get; private set; }

        public ILibrary Library { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ILibraryItemFactory LibraryItemFactory { get; private set; }

        public IDatabase Database { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Library = core.Components.Library;
            this.PlaybackManager = core.Managers.Playback;
            this.LibraryItemFactory = core.Factories.LibraryItem;
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            this.EnumerateFiles();
            this.SanitizeFiles();
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

        private void SanitizeFiles()
        {
            var fileNames = this.FileNames.ToList();
            this.Name = "Preparing file list";
            this.Position = 0;
            this.Count = fileNames.Count;
            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            Logger.Write(this, LogLevel.Debug, "Sanitizing files.");
            for (var a = 0; a < fileNames.Count; )
            {
                var fileName = fileNames[a];
                if (this.Database.Interlocked(() => this.Library.LibraryItemSet.Any(libraryItem => libraryItem.FileName == fileName)))
                {
                    Logger.Write(this, LogLevel.Debug, "File already exists in library: {0}", fileName);
                    fileNames.RemoveAt(a);
                }
                else
                {
                    a++;
                }
                if (position % interval == 0)
                {
                    this.Description = new FileInfo(fileName).Name;
                    this.Position = position;
                }
                position++;
            }
            Logger.Write(this, LogLevel.Debug, "Sanitized {0} files.", fileNames.Count);
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
            Logger.Write(this, LogLevel.Debug, "Converting file names to library items.");
            var query =
                from fileName in this.FileNames
                where this.PlaybackManager.IsSupported(fileName)
                select this.LibraryItemFactory.Create(fileName);
            foreach (var libraryItem in query)
            {
                Logger.Write(this, LogLevel.Debug, "Adding item to library: {0} => {1}", libraryItem.Id, libraryItem.FileName);
                this.Database.Interlocked(() => this.Library.LibraryItemSet.Add(libraryItem));
                if (position % interval == 0)
                {
                    this.Description = Path.GetFileName(libraryItem.FileName);
                    this.Position = position;
                }
                position++;
            }
            this.Position = this.Count;
        }

        private Task SaveChanges()
        {
            this.Name = "Saving changes";
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Saving changes to library.");
            return this.Database.Interlocked(async () => await this.Database.SaveChangesAsync());
        }
    }
}
