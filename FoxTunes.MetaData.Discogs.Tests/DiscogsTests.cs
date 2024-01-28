using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes.MetaData.Discogs.Tests
{
    [TestFixture]
    public class DiscogsTests : TestBase
    {
        [TestCase("nirvana", "nevermind")]
        public async Task CanGetReleases(string artist, string album)
        {
            var discogs = new global::FoxTunes.Discogs();
            discogs.InitializeComponent(this.Core);
            var releases = await discogs.GetReleases(artist, album).ConfigureAwait(false);
            Assert.GreaterOrEqual(releases.ToArray().Length, 1);
        }

        [TestCase("https://img.discogs.com/yHidVUUKaW6hnmCxhmUDY1hwnHs=/fit-in/320x317/filters:strip_icc():format(jpeg):mode_rgb():quality(90)/discogs-images/R-3686829-1340531254-3543.jpeg.jpg")]
        public async Task CanGetData(string url)
        {
            var discogs = new global::FoxTunes.Discogs();
            discogs.InitializeComponent(this.Core);
            var data = await discogs.GetData(url).ConfigureAwait(false);
            Assert.GreaterOrEqual(data.Length, 1);
        }
    }
}
