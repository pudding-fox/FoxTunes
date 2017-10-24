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

        public IEnumerable<LibraryHierarchyNode> GetRootNodes(LibraryHierarchy libraryHierarchy)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyRootNodes, new[] { "libraryHierarchyId", "filter" }, out parameters))
            {
                parameters["libraryHierarchyId"] = libraryHierarchy.Id;
                parameters["filter"] = this.GetFilter();
                return this.GetNodes(command).ToArray();
            }
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyNodes, new[] { "libraryHierarchyId", "libraryHierarchyLevelId", "displayValue", "filter" }, out parameters))
            {
                parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                parameters["libraryHierarchyLevelId"] = libraryHierarchyNode.LibraryHierarchyLevelId;
                parameters["displayValue"] = libraryHierarchyNode.Value;
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

        public IEnumerable<MetaDataItem> GetMetaData(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.DataManager.ReadContext.Connection.CreateCommand(Resources.GetLibraryHierarchyMetaDataItems, new[] { "libraryHierarchyId", "libraryHierarchyLevelId", "displayValue", "type" }, out parameters))
            {
                parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                parameters["libraryHierarchyLevelId"] = libraryHierarchyNode.LibraryHierarchyLevelId;
                parameters["displayValue"] = libraryHierarchyNode.Value;
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
