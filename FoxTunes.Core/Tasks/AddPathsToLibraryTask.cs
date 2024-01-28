using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        protected override void OnRun()
        {
            this.EnumerateFiles();
            this.SanitizeFiles();
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

        private void SanitizeFiles()
        {
            var fileNames = this.FileNames.ToList();
            this.SetName("Preparing file list");
            this.SetPosition(0);
            this.SetCount(fileNames.Count);
            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            for (var a = 0; a < fileNames.Count;)
            {
                var fileName = fileNames[a];
                if (this.Database.Interlocked(() => this.Library.LibraryItemSet.Any(libraryItem => libraryItem.FileName == fileName)))
                {
                    fileNames.RemoveAt(a);
                }
                else
                {
                    a++;
                }
                if (position % interval == 0)
                {
                    this.SetDescription(new FileInfo(fileName).Name);
                    this.SetPosition(position);
                }
                position++;
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
                select this.LibraryItemFactory.Create(fileName);
            foreach (var libraryItem in query)
            {
                this.Database.Interlocked(() => this.Library.LibraryItemSet.Add(libraryItem));
                if (position % interval == 0)
                {
                    this.SetDescription(Path.GetFileName(libraryItem.FileName));
                    this.SetPosition(position);
                }
                position++;
            }
            this.SetPosition(this.Count);
        }

        private void SaveChanges()
        {
            this.SetName("Saving changes");
            this.SetPosition(this.Count);
            this.Database.Interlocked(() => this.Database.SaveChanges());
        }
    }
}
