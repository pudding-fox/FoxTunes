using FoxTunes.Interfaces;
using FoxTunes.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibraryHierarchyProvider : ViewModelBase
    {
        public ILibrary Library { get; private set; }

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
            this.OnPropertyChanged("LibraryNodes");
        }

        public event EventHandler HierarchyChanged = delegate { };

        public ObservableCollection<LibraryNode> LibraryNodes
        {
            get
            {
                if (this.Library == null || this.Hierarchy == null)
                {
                    return new ObservableCollection<LibraryNode>();
                }
                return new ObservableCollection<LibraryNode>(this.BuildHierarchy(this.Library.Set, 0));
            }
        }

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
            //TODO: This is a hack in order to make the view refresh when the selected hierarchy is changed.
            this.OnPropertyChanged("LibraryNodes");
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryHierarchyProvider();
        }
    }
}
