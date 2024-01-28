using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Text;

namespace FoxTunes
{
    public class LibraryHierarchyBrowser : StandardComponent, ILibraryHierarchyBrowser
    {
        public ICore Core { get; private set; }

        public IDataManager DataManager { get; private set; }

        private string _Filter { get; set; }

        public string Filter
        {
            get
            {
                return this._Filter;
            }
            set
            {
                this._Filter = value;
                this.OnFilterChanged();
            }
        }

        protected virtual void OnFilterChanged()
        {
            if (this.FilterChanged != null)
            {
                this.FilterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Filter");
        }

        public event EventHandler FilterChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.DataManager = core.Managers.Data;
            base.InitializeComponent(core);
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchy libraryHierarchy)
        {
            using (var command = this.GetCommand(libraryHierarchy.Id))
            {
                return this.GetNodes(command).ToArray();
            }
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode)
        {
            using (var command = this.GetCommand(libraryHierarchyNode.LibraryHierarchyId, libraryHierarchyNode.Id))
            {
                return this.GetNodes(command).ToArray();
            }
        }

        private IDbCommand GetCommand(int libraryHierarchyId, int? libraryHierarchyItemId = null)
        {
            var command = default(IDbCommand);
            var parameters = default(IDbParameterCollection);
            if (string.IsNullOrEmpty(this.Filter))
            {
                command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyNodes, new[] { "libraryHierarchyId", "libraryHierarchyItemId" }, out parameters);
            }
            else
            {
                command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyNodesWithFilter, new[] { "libraryHierarchyId", "libraryHierarchyItemId", "filter" }, out parameters);
                parameters["filter"] = this.GetFilter();
            }
            parameters["libraryHierarchyId"] = libraryHierarchyId;
            parameters["libraryHierarchyItemId"] = libraryHierarchyItemId.HasValue ? (object)libraryHierarchyItemId.Value : DBNull.Value;
            return command;
        }

        private IEnumerable<LibraryHierarchyNode> GetNodes(IDbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var libraryHierarchyId = reader.GetInt32(1);
                    var value = reader.GetString(2);
                    var isLeaf = reader.GetBoolean(3);
                    var libraryHierarchyNode = new LibraryHierarchyNode(id, libraryHierarchyId, value, isLeaf);
                    libraryHierarchyNode.InitializeComponent(this.Core);
                    yield return libraryHierarchyNode;
                }
            }
        }

        public IEnumerable<MetaDataItem> GetMetaData(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyMetaDataItems, new[] { "libraryHierarchyItemId", "type" }, out parameters))
            {
                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                parameters["type"] = metaDataItemType;
                return this.GetMetaData(command).ToArray();
            }
        }

        private IEnumerable<MetaDataItem> GetMetaData(IDbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    var type = reader.GetValue<MetaDataItemType>(1);
                    var numericValue = reader.IsDBNull(2) ? null : reader.GetValue<int?>(2);
                    var textValue = reader.IsDBNull(3) ? null : reader.GetString(3);
                    var fileValue = reader.IsDBNull(4) ? null : reader.GetString(4);
                    var metaDataItem = new MetaDataItem(name, type)
                    {
                        NumericValue = numericValue,
                        TextValue = textValue,
                        FileValue = fileValue
                    };
                    metaDataItem.InitializeComponent(this.Core);
                    yield return metaDataItem;
                }
            }
        }

        private string GetFilter()
        {
            if (string.IsNullOrEmpty(this.Filter))
            {
                return null;
            }
            var builder = new StringBuilder();
            builder.Append('%');
            builder.Append(this.Filter.Replace(' ', '%'));
            builder.Append('%');
            return builder.ToString();
        }
    }
}
