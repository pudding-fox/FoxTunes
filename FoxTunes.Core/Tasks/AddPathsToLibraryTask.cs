using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToLibraryTask : LibraryTaskBase
    {
        public const string ID = "972222C8-8F6E-44CF-8EBE-DA4FCFD7CD80";

        public const int SAVE_INTERVAL = 1000;

        public AddPathsToLibraryTask(IEnumerable<string> paths)
            : base(ID)
        {
            this.Paths = paths;
        }

        public IEnumerable<string> Paths { get; private set; }

        public IEnumerable<string> FileNames { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ILibraryItemFactory LibraryItemFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.LibraryItemFactory = core.Factories.LibraryItem;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            this.EnumerateFiles();
            using (var context = this.DataManager.CreateWriteContext())
            {
                this.SanitizeFiles(context);
                await this.AddFiles(context);
                await this.SaveChanges(context, true);
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
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

        private void SanitizeFiles(IDatabaseContext context)
        {
            var fileNames = this.FileNames.ToList();
            this.Name = "Preparing file list";
            this.Position = 0;
            this.Count = fileNames.Count;
            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            Logger.Write(this, LogLevel.Debug, "Sanitizing files.");
            for (var a = 0; a < fileNames.Count;)
            {
                var fileName = fileNames[a];
                if (context.Queries.LibraryItem.Any(libraryItem => libraryItem.FileName == fileName))
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

        private async Task AddFiles(IDatabaseContext context)
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
                context.Sets.LibraryItem.Add(libraryItem);
                if (position % interval == 0)
                {
                    this.Description = Path.GetFileName(libraryItem.FileName);
                    this.Position = position;
                }
                if (position > 0 && position % SAVE_INTERVAL == 0)
                {
                    await this.SaveChanges(context, false);
                }
                position++;
            }
            this.Position = this.Count;
        }
    }
}
