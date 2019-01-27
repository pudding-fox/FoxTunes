using NUnit.Framework;
using System.Threading.Tasks;

namespace FoxTunes.Output.Bass.Tests
{
    [TestFixture]
    public class BassPipelineTests : TestBase
    {
        [Test]
        public async Task CanCreateStream()
        {
            await this.Core.Components.Output.Unload(
                await this.Core.Components.Output.Load(TestInfo.PlaylistItems[0], false)
            );
        }

        [Test]
        public async Task CannotPreemptDifferingStream()
        {
            var outputStreams = new[]
            {
                await this.Core.Components.Output.Load(TestInfo.PlaylistItems[0], false),
                await this.Core.Components.Output.Load(TestInfo.PlaylistItems[1], false)
            };
            await outputStreams[0].Play();
            Assert.IsFalse(await this.Core.Components.Output.Preempt(outputStreams[1]));
        }

        [Test]
        public async Task CanReconfigurePipeline()
        {
            var outputStreams = new[]
            {
                await this.Core.Components.Output.Load(TestInfo.PlaylistItems[0], false),
                await this.Core.Components.Output.Load(TestInfo.PlaylistItems[1], false)
            };
            await outputStreams[0].Play();
            await outputStreams[1].Play();
#if NET40
            await TaskEx.Delay(1000);
#else
            await Task.Delay(1000);
#endif
            Assert.IsTrue(outputStreams[1].Position > 0);
        }
    }
}
