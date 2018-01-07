using NUnit.Framework;
using System.Linq;

namespace FoxTunes
{
    [TestFixture]
    public class DatabaseTests : TestBase
    {
        [Test]
        public void CanAddUpdateDeleteRecords()
        {
            using (var transaction = this.Core.Components.Database.BeginTransaction())
            {
                var set = this.Core.Components.Database.Set<PlaylistColumn>(transaction);
                set.Delete(set);
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
                set.Delete(set);
                Assert.AreEqual(0, set.Count);
                transaction.Rollback();
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
