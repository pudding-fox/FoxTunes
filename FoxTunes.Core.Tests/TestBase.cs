using FoxTunes.Interfaces;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

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

        public static string Unique
        {
            get
            {
                return Guid.NewGuid().ToString("d").Split('-')[0];
            }
        }

        protected TestBase()
            : this(DEFAULT)
        {

        }

        protected TestBase(long configuration)
        {
            this.Configuration = configuration;
        }

        public ParallelOptions ParallelOptions
        {
            get
            {
                return new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
            }
        }

        public long Configuration { get; private set; }

        public ICore Core { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            this.Core = new TestCore();
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
