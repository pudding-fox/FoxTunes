using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;
using FoxDb;

namespace FoxTunes
{
    [TestFixture]
    public class PlaylistTests : TestBase
    {
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

        protected virtual Task<PlaylistItem> GetPlaylistItem(string fileName)
        {
            return this.Core.Components.Database.AsQueryable<PlaylistItem>()
                .Where(playlistItem => playlistItem.FileName == fileName)
                .Take(1)
                .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault());
        }

        protected virtual void AssertPlaylistItems(params string[] fileNames)
        {
            var sequence = this.Core.Components.Database.Sets.PlaylistItem;
            var query = (
                from element in sequence
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
