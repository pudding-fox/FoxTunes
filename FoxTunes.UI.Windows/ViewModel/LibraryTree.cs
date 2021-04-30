using System;
using System.Collections.Generic;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibraryTree : LibraryBase
    {
        protected override void OnSelectedItemChanged(object sender, EventArgs e)
        {
            if (!this.IsNavigating)
            {
                this.Synchronize();
            }
            base.OnSelectedItemChanged(sender, e);
        }

        private void Synchronize()
        {
            if (this.SelectedItem == null || LibraryHierarchyNode.Empty.Equals(this.SelectedItem))
            {
                return;
            }
            var stack = new Stack<LibraryHierarchyNode>();
            var libraryHierarchyNode = this.SelectedItem;
            while (libraryHierarchyNode.Parent != null)
            {
                libraryHierarchyNode = libraryHierarchyNode.Parent;
                stack.Push(libraryHierarchyNode);
            }
            while (stack.Count > 0)
            {
                libraryHierarchyNode = stack.Pop();
                libraryHierarchyNode.IsExpanded = true;
            }
            this.SelectedItem.IsSelected = true;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryTree();
        }
    }
}
