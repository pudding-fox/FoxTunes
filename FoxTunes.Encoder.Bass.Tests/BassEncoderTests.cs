using NUnit.Framework;
using System.Threading.Tasks;

namespace FoxTunes.Encoder.Bass.Tests
{
    [Explicit]
    public class BassEncoderTests : TestBase
    {
        [Test]
        public async Task CanEncodePlaylistItems()
        {
            var behaviour = ComponentRegistry.Instance.GetComponent<BassEncoderBehaviour>();
            await behaviour.Encode(TestInfo.PlaylistItems).ConfigureAwait(false);
        }
    }
}
