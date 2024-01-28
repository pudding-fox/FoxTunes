using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryPopulator : PopulatorBase
    {
        public LibraryPopulator(IDatabaseComponent database, IPlaybackManager playbackManager, bool reportProgress, ITransactionSource transaction) : base(reportProgress)
        {
            this.Database = database;
            this.PlaybackManager = playbackManager;
            this.Transaction = transaction;
        }

        public IDatabaseComponent Database { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public async Task Populate(IEnumerable<string> paths)
        {
            using (var writer = new LibraryWriter(this.Database, this.Transaction))
            {
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var fileName in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                        {
                            await this.AddLibraryItem(writer, fileName);
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
                return Task.CompletedTask;
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
