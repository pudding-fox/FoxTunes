using NUnit.Framework;
using System.Linq;

namespace FoxTunes
{
    [TestFixture]
    public class FileAssociationTests : TestBase
    {
        [Test]
        public void CanAddFetchRemove_FileAssociation()
        {
            var extension = ".9a8bb147";
            var fileAssociations = new FileAssociations();
            var association = fileAssociations.Create(extension);
            try
            {
                fileAssociations.Enable();
                fileAssociations.Enable(new[] { association });
                Assert.IsTrue(fileAssociations.Associations.Contains(association));
            }
            finally
            {
                fileAssociations.Disable();
                fileAssociations.Disable(new[] { association });
            }
        }
    }
}
