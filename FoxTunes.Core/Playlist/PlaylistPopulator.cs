using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistPopulator : PopulatorBase
    {
        public PlaylistPopulator(IDatabaseComponent database, IPlaybackManager playbackManager, int sequence, int offset, bool reportProgress, ITransactionSource transaction)
            : base(reportProgress)
        {
            this.Database = database;
            this.PlaybackManager = playbackManager;
            this.Sequence = sequence;
            this.Offset = offset;
            this.Transaction = transaction;
        }

        public IDatabaseComponent Database { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public int Sequence { get; private set; }

        public int Offset { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public async Task Populate(IEnumerable<string> paths)
        {
            var interval = 10;
            var position = 0;
            using (var writer = new PlaylistWriter(this.Database, this.Transaction))
            {
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var fileName in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                        {
                            Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", fileName);
                            await this.AddPlaylistItem(writer, fileName);
                            if (this.ReportProgress)
                            {
                                if (position % interval == 0)
                                {
                                    this.Description = new FileInfo(fileName).Name;
                                    this.Position = position;
                                }
                                position++;
                            }
                        }
                    }
                    else if (File.Exists(path))
                    {
                        Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", path);
                        await this.AddPlaylistItem(writer, path);
                    }
                }
            }
        }

        protected virtual async Task AddPlaylistItem(PlaylistWriter writer, string fileName)
        {
            if (!this.PlaybackManager.IsSupported(fileName))
            {
                Logger.Write(this, LogLevel.Debug, "File is not supported: {0}", fileName);
                return;
            }
            var playlistItem = new PlaylistItem()
            {
                DirectoryName = Path.GetDirectoryName(fileName),
                FileName = fileName,
                Sequence = this.Sequence
            };
            await writer.Write(playlistItem);
            this.Offset++;
        }
    }
}
