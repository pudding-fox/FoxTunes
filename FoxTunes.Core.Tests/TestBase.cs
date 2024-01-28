using FoxTunes.Interfaces;
using NUnit.Framework;

namespace FoxTunes
{
    [TestFixture]
    public abstract class TestBase
    {
        static TestBase()
        {
            AssemblyResolver.Instance.Enable();
        }

        public ICore Core { get; private set; }

        [SetUp]
        public void SetUp()
        {
            this.Core = new Core();
            this.Core.Load();
        }
    }
}
