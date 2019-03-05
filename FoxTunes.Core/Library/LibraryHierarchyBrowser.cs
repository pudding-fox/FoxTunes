#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxTunes
{
    public class LibraryHierarchyBrowser : StandardComponent, ILibraryHierarchyBrowser
    {
        public ICore Core { get; private set; }

        public ILibraryHierarchyCache LibraryHierarchyCache { get; private set; }

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
                if (string.Equals(this._Filter, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
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

        public event EventHandler FilterChanged;

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LibraryHierarchyCache = core.Components.LibraryHierarchyCache;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public IEnumerable<LibraryHierarchy> GetHierarchies()
        {
            return this.LibraryHierarchyCache.GetHierarchies(this.GetHierarchiesCore);
        }

        private IEnumerable<LibraryHierarchy> GetHierarchiesCore()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var set = database.Set<LibraryHierarchy>(transaction);
                    set.Fetch.Sort.Expressions.Clear();
                    set.Fetch.Sort.AddColumn(set.Table.GetColumn(ColumnConfig.By("Sequence", ColumnFlags.None)));
                    foreach (var element in set)
                    {
                        yield return element;
                    }
                }
            }
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchy libraryHierarchy)
        {
            return this.LibraryHierarchyCache.GetNodes(libraryHierarchy, this.Filter, () => this.GetNodesCore(libraryHierarchy));
        }

        private IEnumerable<LibraryHierarchyNode> GetNodesCore(LibraryHierarchy libraryHierarchy)
        {
            return this.GetNodes(libraryHierarchy.Id);
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode)
        {
            return this.LibraryHierarchyCache.GetNodes(libraryHierarchyNode, this.Filter, () => this.GetNodesCore(libraryHierarchyNode));
        }

        private IEnumerable<LibraryHierarchyNode> GetNodesCore(LibraryHierarchyNode libraryHierarchyNode)
        {
            foreach (var child in this.GetNodes(libraryHierarchyNode.LibraryHierarchyId, libraryHierarchyNode.Id))
            {
                child.Parent = libraryHierarchyNode;
                yield return child;
            }
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
