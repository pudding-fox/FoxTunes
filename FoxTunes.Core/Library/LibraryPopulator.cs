using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace FoxTunes
{
    public class LibraryPopulator : PopulatorBase
    {
        public LibraryPopulator(IDatabaseComponent database, IPlaybackManager playbackManager, bool reportProgress, ITransactionSource transaction)
            : base(reportProgress)
        {
            this.Database = database;
            this.PlaybackManager = playbackManager;
            this.Transaction = transaction;
        }

        public IDatabaseComponent Database { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public string Current { get; private set; }

        public async Task Populate(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            if (this.ReportProgress)
            {
                await this.SetName("Populating library");
                await this.SetPosition(0);
                this.Timer.Interval = FAST_INTERVAL;
                this.Timer.Start();
            }

            using (var writer = new LibraryWriter(this.Database, this.Transaction))
            {
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var fileName in FileSystemHelper.EnumerateFiles(path, "*.*", FileSystemHelper.SearchOption.Recursive))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            var success = await this.AddLibraryItem(writer, fileName);
                            if (success && this.ReportProgress)
                            {
                                this.Current = fileName;
                            }
                        }
                    }
                    else if (File.Exists(path))
                    {
                        var success = await this.AddLibraryItem(writer, path);
                        if (success && this.ReportProgress)
                        {
                            this.Current = path;
                        }
                    }
                }
            }
        }

        protected virtual async Task<bool> AddLibraryItem(LibraryWriter writer, string fileName)
        {
            if (!this.PlaybackManager.IsSupported(fileName))
            {
                Logger.Write(this, LogLevel.Debug, "File is not supported: {0}", fileName);
                return false;
            }
            Logger.Write(this, LogLevel.Trace, "Adding file to library: {0}", fileName);
            var libraryItem = new LibraryItem()
            {
                DirectoryName = Path.GetDirectoryName(fileName),
                FileName = fileName,
                Status = LibraryItemStatus.Import
            };
            libraryItem.SetImportDate(DateTime.UtcNow);
            await writer.Write(libraryItem);
            return true;
        }

        protected override async void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (this.Current != null)
            {
                await this.SetDescription(new FileInfo(this.Current).Name);
            }
            base.OnElapsed(sender, e);
        }
    }
}
