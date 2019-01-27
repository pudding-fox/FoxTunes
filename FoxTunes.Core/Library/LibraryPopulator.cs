using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryPopulator : PopulatorBase
    {
        public const string ID = "00672118-A730-4CD8-8B0A-F3DA42712165";

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

        public async Task Populate(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            var interval = 10;
            var position = 0;
            using (var writer = new LibraryWriter(this.Database, this.Transaction))
            {
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var fileName in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            await this.AddLibraryItem(writer, fileName);
                            if (this.ReportProgress)
                            {
                                if (position % interval == 0)
                                {
                                    await this.SetDescription(new FileInfo(fileName).Name);
                                    await this.SetPosition(position);
                                }
                                position++;
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
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var libraryItem = new LibraryItem()
            {
                DirectoryName = Path.GetDirectoryName(fileName),
                FileName = fileName,
                Status = LibraryItemStatus.Import
            };
            return writer.Write(libraryItem);
        }
    }
}
