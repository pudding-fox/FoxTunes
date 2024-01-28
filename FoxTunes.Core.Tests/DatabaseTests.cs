using FoxDb;
using NUnit.Framework;
using System.Data;
using System.Linq;

namespace FoxTunes
{
    [TestFixture(SQLITE)]
    [TestFixture(SQLSERVER)]
    public class DatabaseTests : TestBase
    {
        public const long SQLITE = 128;

        public const long SQLSERVER = 256;

        public DatabaseTests(long configuration)
            : base(configuration)
        {

        }

        public override void SetUp()
        {
            if ((this.Configuration & SQLITE) != 0)
            {
                ComponentResolver.Slots.Add(ComponentSlots.Database, "13A75018-8A24-413D-A731-C558C8FAF08F");
            }
            else if ((this.Configuration & SQLSERVER) != 0)
            {
                ComponentResolver.Slots.Add(ComponentSlots.Database, "ECF542D9-ABB3-4E82-8045-7E13F1727695");
            }
            base.SetUp();
        }

        public override void TearDown()
        {
            ComponentResolver.Slots.Remove(ComponentSlots.Database);
            base.TearDown();
        }

        [Test]
        [Explicit]
        public void CanAddUpdateDeleteRecords()
        {
            using (var database = this.Core.Factories.Database.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var set = database.Set<PlaylistColumn>(transaction);
                    set.Remove(set);
                    Assert.AreEqual(0, set.Count);
                    set.AddOrUpdate(new[] {
                        new PlaylistColumn() { Name = "Test1", DisplayScript = "N/A" },
                        new PlaylistColumn() { Name = "Test2", DisplayScript = "N/A" },
                        new PlaylistColumn() { Name = "Test3", DisplayScript = "N/A" }
                    });
                    {
                        var items = set.ToArray();
                        this.AssertRecords(items, "Test1", "Test2", "Test3");
                        items[1].Name = "Test_Updated_2";
                        set.AddOrUpdate(items);
                    }
                    {
                        var items = set.ToArray();
                        this.AssertRecords(items, "Test1", "Test_Updated_2", "Test3");
                    }
                    set.Remove(set);
                    Assert.AreEqual(0, set.Count);
                    transaction.Rollback();
                }
            }
        }

        protected virtual void AssertRecords(PlaylistColumn[] items, params string[] names)
        {
            Assert.AreEqual(names.Length, items.Length);
            for (var a = 0; a < names.Length; a++)
            {
                Assert.AreEqual(items[a].Name, names[a]);
            }
        }
    }
}
