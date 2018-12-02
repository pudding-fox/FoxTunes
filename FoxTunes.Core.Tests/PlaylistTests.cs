using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;
using FoxDb;
using System.Data;

namespace FoxTunes
{
    [TestFixture(SQLITE)]
    [TestFixture(SQLSERVER)]
    public class PlaylistTests : DatabaseTests
    {
        public PlaylistTests(long configuration)
            : base(configuration)
        {

        }

        [Test]
        public async Task CanAddFilesToPlaylist()
        {
            await this.Core.Managers.Playlist.Clear();
            await this.Core.Managers.Playlist.Add(new[]
            {
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            }, false);
            this.AssertPlaylistItems(
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            );
            await this.Core.Managers.Playlist.Clear();
            this.AssertPlaylistItems();
        }

        [Test]
        public async Task CanRemoveItemsFromPlaylist()
        {
            await this.Core.Managers.Playlist.Clear();
            await this.Core.Managers.Playlist.Add(new[]
            {
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[1],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            }, false);
            await this.Core.Managers.Playlist.Remove(
                new[]
                {
                    await this.GetPlaylistItem(TestInfo.AudioFileNames[1]),
                    await this.GetPlaylistItem(TestInfo.AudioFileNames[2])
                }
            );
            this.AssertPlaylistItems(
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[3]
            );
        }

        [Test]
        public async Task CanInsertFilesIntoPlaylist()
        {
            await this.Core.Managers.Playlist.Clear();
            await this.Core.Managers.Playlist.Add(new[]
            {
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            }, false);
            this.AssertPlaylistItems(
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            );
            await this.Core.Managers.Playlist.Insert(1, new[]
            {
                TestInfo.AudioFileNames[1]
            }, false);
            this.AssertPlaylistItems(
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[1],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            );
            await this.Core.Managers.Playlist.Clear();
        }

        [Test]
        public async Task CanMoveItemsInPlaylist()
        {
            await this.Core.Managers.Playlist.Clear();
            await this.Core.Managers.Playlist.Add(new[]
            {
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[1],
                TestInfo.AudioFileNames[2]
            }, false);
            await this.Core.Managers.Playlist.Move(
                0,
                new[]
                {
                    await this.GetPlaylistItem(TestInfo.AudioFileNames[2])
                }
            );
            this.AssertPlaylistItems(
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[1]
            );
            await this.Core.Managers.Playlist.Move(
                2,
                new[]
                {
                    await this.GetPlaylistItem(TestInfo.AudioFileNames[2])
                }
            );
            this.AssertPlaylistItems(
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[1],
                TestInfo.AudioFileNames[2]
            );
        }

        protected virtual async Task<PlaylistItem> GetPlaylistItem(string fileName)
        {
            using (var database = this.Core.Components.Database.New())
            {
                using (var transaction = database.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.FileName == fileName)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault());
                }
            }
        }

        protected virtual void AssertPlaylistItems(params string[] fileNames)
        {
            var set = this.Core.Components.Database.Set<PlaylistItem>();
            var query = (
                from element in set
                orderby element.Sequence
                select element
            ).ToArray();
            Assert.AreEqual(fileNames.Length, query.Length);
            for (var a = 0; a < fileNames.Length; a++)
            {
                Assert.AreEqual(fileNames[a], query[a].FileName);
            }
        }
    }
}
