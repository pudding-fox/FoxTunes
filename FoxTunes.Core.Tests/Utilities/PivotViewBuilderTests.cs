using FoxTunes.Utilities.Templates;
using NUnit.Framework;

namespace FoxTunes
{
    [TestFixture]
    public class PivotViewBuilderTests : TestBase
    {
        [Test]
        public void Test001()
        {
            var metaDataNames = new[] 
            {
                "Test001",
                "Test002",
                "Test003"
            };
            var metaDataViewBuilder = new LibraryHierarchyViewBuilder(metaDataNames);
            var view = metaDataViewBuilder.TransformText();
        }
    }
}
