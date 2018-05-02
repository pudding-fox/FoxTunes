using NUnit.Framework;
using System.Threading.Tasks;

namespace FoxTunes.Output.Bass.Tests
{
    [TestFixture]
    public class BassCdTests : TestBase
    {
        [Test]
        public async Task CanOpenCd()
        {
            var behaviour = ComponentRegistry.Instance.GetComponent<BassCdStreamProviderBehaviour>();
            await behaviour.OpenCd();
        }
    }
}
