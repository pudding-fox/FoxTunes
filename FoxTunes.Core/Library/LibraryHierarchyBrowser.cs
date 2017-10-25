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
            var parameters = default(IDbParameterCollection);
            using (var command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyNodes, new[] { "libraryHierarchyId", "libraryHierarchyItemId", "filter" }, out parameters))
            {
                parameters["libraryHierarchyId"] = libraryHierarchy.Id;
                parameters["libraryHierarchyItemId"] = DBNull.Value;
                parameters["filter"] = this.GetFilter();
                return this.GetNodes(command).ToArray();
            }
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyNodes, new[] { "libraryHierarchyId", "libraryHierarchyItemId", "filter" }, out parameters))
            {
                parameters["libraryHierarchyId"] = DBNull.Value;
                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                parameters["filter"] = this.GetFilter();
                return this.GetNodes(command).ToArray();
            }
        }

        private IEnumerable<LibraryHierarchyNode> GetNodes(IDbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var value = reader.GetString(1);
                    var isLeaf = reader.GetBoolean(2);
                    var libraryHierarchyNode = new LibraryHierarchyNode(id, value, isLeaf);
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
