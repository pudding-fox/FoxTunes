using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

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

        public string Current { get; private set; }

        public async Task Populate(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            if (this.ReportProgress)
            {
                this.Timer.Interval = FAST_INTERVAL;
                this.Timer.Start();
            }

            using (var writer = new PlaylistWriter(this.Database, this.Transaction))
            {
                foreach (var path in paths)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    if (Directory.Exists(path))
                    {
                        foreach (var fileName in FileSystemHelper.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", fileName);
                            await this.AddPlaylistItem(writer, fileName);
                            if (this.ReportProgress)
                            {
                                this.Current = fileName;
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
            Logger.Write(this, LogLevel.Trace, "Adding file to playlist: {0}", fileName);
            var playlistItem = new PlaylistItem()
            {
                DirectoryName = Path.GetDirectoryName(fileName),
                FileName = fileName,
                Sequence = this.Sequence
            };
            await writer.Write(playlistItem);
            this.Offset++;
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
