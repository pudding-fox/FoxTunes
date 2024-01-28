using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    public class LibraryHierarchyBrowser : StandardComponent, ILibraryHierarchyBrowser
    {
        public ICore Core { get; private set; }

        public IDatabase Database { get; private set; }

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
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchy libraryHierarchy)
        {
            return this.GetNodes(libraryHierarchy.Id);
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode)
        {
            return this.GetNodes(libraryHierarchyNode.LibraryHierarchyId, libraryHierarchyNode.Id);
        }

        protected virtual IEnumerable<LibraryHierarchyNode> GetNodes(int libraryHierarchyId, int? libraryHierarchyItemId = null)
        {
            var query = default(IDatabaseQuery);
            if (string.IsNullOrEmpty(this.Filter))
            {
                query = this.Database.Queries.GetLibraryHierarchyNodes;
            }
            else
            {
                query = this.Database.Queries.GetLibraryHierarchyNodesWithFilter;
            }
            return new RecordEnumerator<LibraryHierarchyNode>(this.Core, this.Database, query, parameters =>
            {
                parameters["libraryHierarchyId"] = libraryHierarchyId;
                parameters["libraryHierarchyItemId"] = libraryHierarchyItemId.HasValue ? (object)libraryHierarchyItemId.Value : DBNull.Value;
                if (parameters.Contains("filter"))
                {
                    parameters["filter"] = this.GetFilter();
                }
            });
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
