using System.Collections.Generic;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowserFrame : ViewModelBase
    {
        private LibraryBrowserFrame()
        {

        }

        public LibraryBrowserFrame(LibraryHierarchyNode itemsSource, IEnumerable<LibraryHierarchyNode> items) : this()
        {
            this.ItemsSource = itemsSource;
            this.Items = items;
            this.Freeze();
        }

        public LibraryHierarchyNode ItemsSource { get; private set; }

        public IEnumerable<LibraryHierarchyNode> Items { get; private set; }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryBrowserFrame();
        }
    }
}
