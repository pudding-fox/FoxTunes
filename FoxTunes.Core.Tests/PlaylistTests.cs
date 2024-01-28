using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;

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
