using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [TestFixture]
    public class LibraryTests : TestBase
    {
        [Test]
        public async Task CanAddFilesToLibrary()
        {
            await this.Core.Managers.Library.Clear();
            await this.Core.Managers.Library.Add(new[]
            {
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            });
            this.AssertLibraryItems(
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            );
            this.AssertLibraryHierarchy(
                TestInfo.AudioFileNames[0],
                TestInfo.AudioFileNames[2],
                TestInfo.AudioFileNames[3]
            );
            await this.Core.Managers.Library.Clear();
        }

        protected virtual void AssertLibraryItems(params string[] fileNames)
        {
            var sequence = this.Core.Components.Database.Sets.LibraryItem;
            var query = (
                from element in sequence
                select element
            ).ToArray();
            Assert.AreEqual(fileNames.Length, query.Length);
            for (var a = 0; a < fileNames.Length; a++)
            {
                Assert.AreEqual(fileNames[a], query[a].FileName);
            }
        }

        protected virtual void AssertLibraryHierarchy(params string[] fileNames)
        {
            foreach (var hierarchy in this.Core.Components.Database.Sets.LibraryHierarchy)
            {
                var nodes = this.Core.Components.LibraryHierarchyBrowser.GetNodes(hierarchy);
            }
        }
    }
}
