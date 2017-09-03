using System;
using FoxTunes.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BuildLibraryHierarchiesTask : BackgroundTask
    {
        public const string ID = "B6AF297E-F334-481D-8D60-BD5BE5935BD9";

        public BuildLibraryHierarchiesTask()
            : base(ID)
        {
        }

        public ILibrary Library { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public IDatabase Database { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Library = core.Components.Library;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            this.Position = 0;
            this.Count = this.Database.Interlocked(() => this.Library.LibraryItemQuery.Count() * this.Library.LibraryHierarchyQuery.Count());
            foreach (var libraryHierarchy in this.Library.LibraryHierarchyQuery)
            {
                Logger.Write(this, LogLevel.Debug, "Building library hierarchy: {0} => {1}", libraryHierarchy.Id, libraryHierarchy.Name);
                this.Description = libraryHierarchy.Name;
                this.ClearHierarchy(libraryHierarchy);
                var libraryHierarchyItems = this.BuildHierarchy(libraryHierarchy);
                foreach (var libraryHierarchyItem in libraryHierarchyItems)
                {
                    this.Database.Interlocked(() => libraryHierarchy.Items.Add(libraryHierarchyItem));
                }
            }
            this.Position = this.Count;
            return this.SaveChanges();
        }

        private void ClearHierarchy(LibraryHierarchy libraryHierarchy)
        {
            Logger.Write(this, LogLevel.Debug, "Clearing existing library hierarchy: {0} => {1}", libraryHierarchy.Id, libraryHierarchy.Name);
            this.Database.Interlocked(() => libraryHierarchy.Items.Clear());
        }

        private IEnumerable<LibraryHierarchyItem> BuildHierarchy(LibraryHierarchy libraryHierarchy)
        {
            return this.BuildHierarchy(libraryHierarchy, null, this.Library.LibraryItemQuery, 0);
        }

        private IEnumerable<LibraryHierarchyItem> BuildHierarchy(LibraryHierarchy libraryHierarchy, LibraryHierarchyItem parent, IEnumerable<LibraryItem> libraryItems, int level)
        {
            var isLeaf = level >= libraryHierarchy.Levels.Count - 1;
            var libraryHierarchyLevel = libraryHierarchy.Levels[level];
            Logger.Write(this, LogLevel.Trace, "Building library hierarchy level: {0} => {1}", libraryHierarchyLevel.Id, libraryHierarchyLevel.DisplayScript);
            var query = default(IEnumerable<LibraryHierarchyItem>);
            if (!isLeaf)
            {
                query =
                    from libraryItem in libraryItems
                    group libraryItem by new
                    {
                        Display = this.ExecuteScript(libraryItem, libraryHierarchyLevel.DisplayScript),
                        Sort = this.ExecuteScript(libraryItem, libraryHierarchyLevel.SortScript),
                    } into hierarchy
                    select new LibraryHierarchyItem(hierarchy.Key.Display, hierarchy.Key.Sort, isLeaf)
                    {
                        Parent = parent,
                        Items = new ObservableCollection<LibraryItem>(hierarchy)
                    };
            }
            else
            {
                query =
                    from libraryItem in libraryItems
                    select new LibraryHierarchyItem(
                        this.ExecuteScript(libraryItem, libraryHierarchyLevel.DisplayScript),
                        this.ExecuteScript(libraryItem, libraryHierarchyLevel.SortScript),
                        isLeaf
                    )
                    {
                        Parent = parent,
                        Items = new ObservableCollection<LibraryItem>(new[] { libraryItem })
                    };
            }
            var libraryHierarchyItems = this.OrderBy(query).ToList();
            if (!isLeaf)
            {
                foreach (var libraryHierarchyItem in libraryHierarchyItems)
                {
                    libraryHierarchyItem.Children = new ObservableCollection<LibraryHierarchyItem>(
                        this.BuildHierarchy(libraryHierarchy, libraryHierarchyItem, libraryHierarchyItem.Items, level + 1)
                    );
                }
            }
            else
            {
                this.Position = this.Position + libraryHierarchyItems.Count;
            }
            return libraryHierarchyItems;
        }

        private IEnumerable<LibraryHierarchyItem> OrderBy(IEnumerable<LibraryHierarchyItem> libraryHierarchyItems)
        {
            var query =
                from libraryHierarchyItem in libraryHierarchyItems
                orderby libraryHierarchyItem.SortValue, libraryHierarchyItem.DisplayValue
                select libraryHierarchyItem;
            return query;
        }

        private Task SaveChanges()
        {
            this.Name = "Saving changes";
            this.Position = this.Count;
            Logger.Write(this, LogLevel.Debug, "Saving changes to library.");
            return this.Database.Interlocked(() => this.Database.WithAutoDetectChanges(async () => await this.Database.SaveChangesAsync()));
        }

        private string ExecuteScript(LibraryItem libraryItem, string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                return string.Empty;
            }
            this.EnsureScriptingContext();
            var runner = new LibraryItemScriptRunner(this.ScriptingContext, libraryItem, script);
            runner.Prepare();
            return Convert.ToString(runner.Run());
        }

        private void EnsureScriptingContext()
        {
            if (this.ScriptingContext != null)
            {
                return;
            }
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
        }
    }
}
