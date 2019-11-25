#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class LibraryHierarchyBrowser : StandardComponent, ILibraryHierarchyBrowser
    {
        const MetaDataItemType META_DATA_TYPE = MetaDataItemType.Tag | MetaDataItemType.Image;

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public ILibraryHierarchyCache LibraryHierarchyCache { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement ShowFavorites { get; private set; }

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
            this.LibraryManager = core.Managers.Library;
            this.LibraryHierarchyCache = core.Components.LibraryHierarchyCache;
            this.DatabaseFactory = core.Factories.Database;
            this.Configuration = core.Components.Configuration;
            this.ShowFavorites = this.Configuration.GetElement<BooleanConfigurationElement>(
                LibraryFavoritesBehaviourConfiguration.SECTION,
                LibraryFavoritesBehaviourConfiguration.SHOW_FAVORITES_ELEMENT
            );
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
                    set.Fetch.Filter.AddColumn(
                        set.Table.GetColumn(ColumnConfig.By("Enabled", ColumnFlags.None))
                    ).With(filter => filter.Right = filter.CreateConstant(1));
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
            var key = new LibraryHierarchyCacheKey(libraryHierarchy, this.Filter, this.ShowFavorites.Value);
            var nodes = this.LibraryHierarchyCache.GetNodes(key, () => this.GetNodesCore(libraryHierarchy));
            if (this.LibraryManager.SelectedItem != null)
            {
                this.ApplySelection(nodes);
            }
            return nodes;
        }

        private IEnumerable<LibraryHierarchyNode> GetNodesCore(LibraryHierarchy libraryHierarchy)
        {
            return this.GetNodes(libraryHierarchy.Id);
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode)
        {
            var key = new LibraryHierarchyCacheKey(libraryHierarchyNode, this.Filter, this.ShowFavorites.Value);
            return this.LibraryHierarchyCache.GetNodes(key, () => this.GetNodesCore(libraryHierarchyNode));
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
                                if (this.ShowFavorites.Value)
                                {
                                    parameters["favorite"] = true;
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

        public IEnumerable<LibraryItem> GetItems(LibraryHierarchyNode libraryHierarchyNode, bool loadMetaData)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                var query = default(IDatabaseQuery);
                if (string.IsNullOrEmpty(this.Filter))
                {
                    query = database.Queries.GetLibraryItems;
                }
                else
                {
                    query = database.Queries.GetLibraryItemsWithFilter;
                }
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var items = database.ExecuteEnumerator<LibraryItem>(query, (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                                if (parameters.Contains("filter"))
                                {
                                    parameters["filter"] = this.GetFilter();
                                }
                                if (this.ShowFavorites.Value)
                                {
                                    parameters["favorite"] = true;
                                }
                                break;
                        }
                    }, transaction);
                    foreach (var item in items)
                    {
                        //TODO: This is awful. We should hydrate the meta data using the same query.
                        if (loadMetaData)
                        {
                            item.MetaDatas = new ObservableCollection<MetaDataItem>(
                                this.GetMetaDatas(item)
                            );
                        }
                        item.InitializeComponent(this.Core);
                        yield return item;
                    }
                }
            }
        }

        protected virtual IEnumerable<MetaDataItem> GetMetaDatas(LibraryItem libraryItem)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = MetaDataInfo.GetMetaData(database, libraryItem, META_DATA_TYPE, transaction))
                    {
                        var metaDatas = new List<MetaDataItem>();
                        foreach (var record in reader)
                        {
                            metaDatas.Add(new MetaDataItem()
                            {
                                Id = record.Get<int>("Id"),
                                Name = record.Get<string>("Name"),
                                //TODO: Make IDatabaseReaderRecord.Get<Enum> work properly.
                                Type = (MetaDataItemType)record.Get<byte>("Type"),
                                Value = record.Get<string>("Value")
                            });
                        }
                        return metaDatas;
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

        private void ApplySelection(IEnumerable<LibraryHierarchyNode> nodes)
        {
            var libraryHierarchyNode = this.LibraryManager.SelectedItem;
            var stack = new Stack<LibraryHierarchyNode>(new[] { libraryHierarchyNode });
            while (libraryHierarchyNode.Parent != null)
            {
                libraryHierarchyNode = libraryHierarchyNode.Parent;
                stack.Push(libraryHierarchyNode);
            }
            while (stack.Count > 0)
            {
                var node = this.FindNode(nodes, stack.Pop().Value);
                if (node == null)
                {
                    break;
                }
                libraryHierarchyNode = node;
                if (stack.Count == 0 || libraryHierarchyNode.IsLeaf)
                {
                    break;
                }
                if (!node.IsExpanded)
                {
                    node.LoadChildren();
                    node.IsExpanded = true;
                }
                nodes = node.Children;
            }
            if (libraryHierarchyNode != null)
            {
                libraryHierarchyNode.IsSelected = true;
            }
        }

        private LibraryHierarchyNode FindNode(IEnumerable<LibraryHierarchyNode> nodes, string value)
        {
            return nodes.FirstOrDefault(node => string.Equals(node.Value, value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
