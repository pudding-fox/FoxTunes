#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FoxTunes
{
    public class LibraryHierarchyBrowser : StandardComponent, ILibraryHierarchyBrowser
    {
        public ICore Core { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

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
            this.DatabaseFactory = core.Factories.Database;
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
            using (var database = this.DatabaseFactory.Create())
            {
                var query = default(IDatabaseQuery);
                if (string.IsNullOrEmpty(this.Filter))
                {
                    query = database.Queries.GetLibraryHierarchyNodes;
                }
                else
                {
                    query = database.Queries.GetLibraryHierarchyNodesWithFilter;
                }
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var nodes = database.ExecuteEnumerator<LibraryHierarchyNode>(query, (parameters, phase) =>
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
                    }, transaction);
                    foreach (var node in nodes)
                    {
                        node.InitializeComponent(this.Core);
                        yield return node;
                    }
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
