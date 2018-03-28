using FoxTunes.Interfaces;
using NUnit.Framework;
using System;

namespace FoxTunes
{
    [TestFixture]
    public abstract class TestBase
    {
        public const long DEFAULT = 0;

        static TestBase()
        {
            AssemblyResolver.Instance.Enable();
        }

        protected TestBase()
            : this(DEFAULT)
        {

        }

        protected TestBase(long configuration)
        {
            this.Configuration = configuration;
        }

        public long Configuration { get; private set; }

        public ICore Core { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            this.Core = new Core();
            this.Core.Load();
        }

        [TearDown]
        public virtual void TearDown()
        {
            this.Core.Dispose();
            this.Core = null;
        }
    }
}
