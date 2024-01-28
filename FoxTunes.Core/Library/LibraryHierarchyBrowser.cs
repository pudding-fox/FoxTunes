using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    public class LibraryHierarchyBrowser : StandardComponent, ILibraryHierarchyBrowser
    {
        public ICore Core { get; private set; }

        public IDatabaseComponent Database { get; private set; }

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
            var nodes = this.Database.ExecuteEnumerator<LibraryHierarchyNode>(query, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["libraryHierarchyId"] = libraryHierarchyId;
                        parameters["libraryHierarchyItemId"] = libraryHierarchyItemId;
                        if (parameters.Contains("filter"))
                        {
                            parameters["filter"] = this.GetFilter();
                        }
                        break;
                }
            }).ToArray();
            foreach (var node in nodes)
            {
                node.InitializeComponent(this.Core);
            }
            return nodes;
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
