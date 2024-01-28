#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryHierarchyWriter : Disposable
    {
        public LibraryHierarchyWriter(IDatabaseComponent database, ITransactionSource transaction)
        {
            this.Command = CreateCommand(database, transaction);
        }

        public IDatabaseCommand Command { get; private set; }

        public async Task<int> Write(LibraryHierarchy libraryHierarchy, LibraryHierarchyLevel libraryHierarchyLevel, int libraryItemId, int? parentId, string value, bool isLeaf)
        {
            this.Command.Parameters["libraryHierarchyId"] = libraryHierarchy.Id;
            this.Command.Parameters["libraryHierarchyLevelId"] = libraryHierarchyLevel.Id;
            this.Command.Parameters["libraryItemId"] = libraryItemId;
            this.Command.Parameters["parentId"] = parentId;
            this.Command.Parameters["value"] = value;
            this.Command.Parameters["isLeaf"] = isLeaf;
            return Converter.ChangeType<int>(await this.Command.ExecuteScalarAsync());
        }

        protected override void OnDisposing()
        {
            this.Command.Dispose();
            base.OnDisposing();
        }

        private static IDatabaseCommand CreateCommand(IDatabaseComponent database, ITransactionSource transaction)
        {
            return database.CreateCommand(
                database.Queries.AddLibraryHierarchyNode,
                DatabaseCommandFlags.NoCache,
                transaction
            );
        }
    }
}
