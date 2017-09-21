using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FoxTunes
{
    public class LibraryHierarchyBrowser : StandardComponent, ILibraryHierarchyBrowser
    {
        public ICore Core { get; private set; }

        public IDataManager DataManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.DataManager = core.Managers.Data;
            base.InitializeComponent(core);
        }

        public IEnumerable<LibraryHierarchyNode> GetRootNodes(LibraryHierarchy libraryHierarchy)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyRootNodes, new[] { "libraryHierarchyId" }, out parameters))
            {
                parameters["libraryHierarchyId"] = libraryHierarchy.Id;
                return this.GetNodes(command).ToArray();
            }
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyNodes, new[] { "libraryHierarchyId", "libraryHierarchyLevelId", "displayValue" }, out parameters))
            {
                parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                parameters["libraryHierarchyLevelId"] = libraryHierarchyNode.LibraryHierarchyLevelId;
                parameters["displayValue"] = libraryHierarchyNode.Value;
                return this.GetNodes(command).ToArray();
            }
        }

        private IEnumerable<LibraryHierarchyNode> GetNodes(IDbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var libraryHierarchyId = reader.GetInt32(0);
                    var libraryHierarchyLevelId = reader.GetInt32(1);
                    var value = reader.GetString(2);
                    var isLeaf = reader.GetBoolean(3);
                    var libraryHierarchyNode = new LibraryHierarchyNode(libraryHierarchyId, libraryHierarchyLevelId, value, isLeaf);
                    libraryHierarchyNode.InitializeComponent(this.Core);
                    yield return libraryHierarchyNode;
                }
            }
        }
    }
}
