using FoxTunes.Interfaces;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    [TestFixture]
    public abstract class TestBase : TestBase<HeadlessCore>
    {
        protected TestBase() : this(DEFAULT)
        {

        }

        protected TestBase(long configuration) : base(configuration)
        {

        }
    }

    public abstract class TestBase<TCore>
        where TCore : Core, new()
    {
        public const long DEFAULT = 0;

        static TestBase()
        {
            AssemblyResolver.Instance.EnableReflectionOnly();
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
            this.Core = new TCore();
            this.Core.Load();
            if (this.Core.Factories.Database.Test() != DatabaseTestResult.OK)
            {
                this.Core.Factories.Database.Initialize();
                using (var database = this.Core.Factories.Database.Create())
                {
                    this.Core.InitializeDatabase(database, DatabaseInitializeType.All);
                }
            }
            this.Core.Initialize();
        }

        [TearDown]
        public virtual void TearDown()
        {
            this.Core.Dispose();
            this.Core = null;
        }
    }
}
