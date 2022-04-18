using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

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
                await behaviour.Encode(TestInfo.PlaylistItems, profile, true).ConfigureAwait(false);
            }
        }

        [Test]
        public async Task CanEncodePlaylistItems_PCM16()
        {
            var behaviour = ComponentRegistry.Instance.GetComponent<BassEncoderBehaviour>();
            await behaviour.Encode(TestInfo.PlaylistItems, RawProfile.PCM16, true).ConfigureAwait(false);
        }
    }
}
