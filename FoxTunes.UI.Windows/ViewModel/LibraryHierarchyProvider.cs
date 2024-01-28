using FoxTunes.Interfaces;
using FoxTunes.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Linq.Expressions;

namespace FoxTunes.ViewModel
{
    public class LibraryHierarchyProvider : ViewModelBase
    {
        public ILibrary Library { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public static readonly DependencyProperty HierarchyProperty = DependencyProperty.Register(
            "Hierarchy",
            typeof(LibraryHierarchy),
            typeof(LibraryHierarchyProvider),
            new PropertyMetadata(new PropertyChangedCallback(OnHierarchyChanged))
        );

        public static void OnHierarchyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var libraryHierarchyProvider = sender as LibraryHierarchyProvider;
            if (libraryHierarchyProvider == null)
            {
                return;
            }
            libraryHierarchyProvider.OnHierarchyChanged();
        }

        public LibraryHierarchy Hierarchy
        {
            get
            {
                return this.GetValue(HierarchyProperty) as LibraryHierarchy;
            }
            set
            {
                this.SetValue(HierarchyProperty, value);
            }
        }

        protected virtual void OnHierarchyChanged()
        {
            if (this.HierarchyChanged != null)
            {
                this.HierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Hierarchy");
            //TODO: This is a hack in order to make the view refresh when the selected hierarchy is changed.
            this.OnLibraryNodesChanged();
        }

        public event EventHandler HierarchyChanged = delegate { };

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
            //TODO: This is a hack in order to make the view refresh when the filter is changed.
            this.OnLibraryNodesChanged();
        }

        public event EventHandler FilterChanged = delegate { };

        private Expression<Func<LibraryItem, bool>> GetFilterExpression(string filter)
        {
            return libraryItem =>
                libraryItem.FileName.Contains(filter) ||
                libraryItem.MetaDatas.Any(
                    metaDataItem => metaDataItem.TextValue != null &&
                        metaDataItem.TextValue.Contains(filter)
                );
        }

        public ObservableCollection<LibraryNode> LibraryNodes
        {
            get
            {
                if (this.Library == null || this.Hierarchy == null)
                {
                    return new ObservableCollection<LibraryNode>();
                }
                return new ObservableCollection<LibraryNode>(this.BuildHierarchy(this.GetLibraryItems(), 0));
            }
        }

        private IEnumerable<LibraryItem> GetLibraryItems()
        {
            if (string.IsNullOrEmpty(this.Filter))
            {
                return this.Library.Query;
            }
            return this.Library.Query.Where(this.GetFilterExpression(this.Filter));
        }

        protected virtual void OnLibraryNodesChanged()
        {
            if (this.LibraryNodesChanged != null)
            {
                this.LibraryNodesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("LibraryNodes");
        }

        public event EventHandler LibraryNodesChanged = delegate { };

        private IEnumerable<LibraryNode> BuildHierarchy(IEnumerable<LibraryItem> libraryItems, int level)
        {
            if (level >= this.Hierarchy.Levels.Count)
            {
                return Enumerable.Empty<LibraryNode>();
            }
            var query =
                from libraryItem in libraryItems
                group libraryItem by this.ExecuteScript(libraryItem, this.Hierarchy.Levels[level].Script) into hierarchy
                select new LibraryNode(hierarchy.Key, hierarchy, this.BuildHierarchy(hierarchy, level + 1));
            return query;
        }

        private string ExecuteScript(LibraryItem libraryItem, string script)
        {
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

        protected override void OnCoreChanged()
        {
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.Library = this.Core.Components.Library;
            this.LibraryManager = this.Core.Managers.Library;
            this.LibraryManager.Updated += (sender, e) => this.OnLibraryNodesChanged();
            //TODO: This is a hack in order to make the view refresh when the selected hierarchy is changed.
            this.OnLibraryNodesChanged();
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryHierarchyProvider();
        }
    }
}
