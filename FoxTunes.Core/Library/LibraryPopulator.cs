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
                        foreach (var fileName in FileSystemHelper.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            await this.AddLibraryItem(writer, fileName);
                            if (this.ReportProgress)
                            {
                                this.Current = fileName;
                            }
                        }
                    }
                    else if (File.Exists(path))
                    {
                        await this.AddLibraryItem(writer, path);
                    }
                }
            }
        }

        protected virtual Task AddLibraryItem(LibraryWriter writer, string fileName)
        {
            if (!this.PlaybackManager.IsSupported(fileName))
            {
                Logger.Write(this, LogLevel.Debug, "File is not supported: {0}", fileName);
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            Logger.Write(this, LogLevel.Trace, "Adding file to library: {0}", fileName);
            var libraryItem = new LibraryItem()
            {
                DirectoryName = Path.GetDirectoryName(fileName),
                FileName = fileName,
                Status = LibraryItemStatus.Import
            };
            libraryItem.SetImportDate(DateTime.UtcNow);
            return writer.Write(libraryItem);
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
