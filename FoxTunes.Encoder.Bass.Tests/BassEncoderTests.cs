using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes.Encoder.Bass.Tests
{
    [Explicit]
    public class BassEncoderTests : TestBase
    {
        [Test]
        public async Task CanEncodePlaylistItems()
        {
            var profiles = ComponentRegistry.Instance.GetComponents<IBassEncoderSettings>().Select(
                settings => settings.Name
            ).ToArray();
            var behaviour = ComponentRegistry.Instance.GetComponent<BassEncoderBehaviour>();
            foreach (var profile in profiles)
            {
                await behaviour.Encode(TestInfo.PlaylistItems, profile).ConfigureAwait(false);
            }
        }
    }
}
