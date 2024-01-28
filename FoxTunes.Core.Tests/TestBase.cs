using FoxTunes.Interfaces;
using NUnit.Framework;
using System;

namespace FoxTunes
{
    public abstract class TestBase
    {
        static TestBase()
        {
            AssemblyResolver.Instance.Enable();
        }

        private static readonly Type[] References = new[]
        {
            typeof(Configuration),
            typeof(SQLiteDatabase),
            typeof(TagLibMetaDataSource),
            typeof(WindowsUserInterface),
            typeof(JSScriptingRuntime)
        };

        public ICore Core { get; private set; }

        [SetUp]
        public void SetUp()
        {
            this.Core = new Core();
            this.Core.Load();
        }

        [TearDown]
        public void TearDown()
        {
            this.Core.Dispose();
            this.Core = null;
        }
    }
}
