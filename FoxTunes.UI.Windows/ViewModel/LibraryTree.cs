using System;
using System.Collections.Generic;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibraryTree : Library
    {
        const int EXPAND_ALL_LIMIT = 5;

        public void ExpandAll()
        {
            var stack = new Stack<LibraryHierarchyNode>(this.Items);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!node.IsExpanded)
                {
                    node.IsExpanded = true;
                }
                foreach (var child in node.Children)
                {
                    if (child.IsLeaf)
                    {
                        continue;
                    }
                    stack.Push(child);
                }
            }
        }

        protected override void OnFilterChanged(object sender, EventArgs e)
        {
            this.Reload();
            if (!string.IsNullOrEmpty(this.LibraryHierarchyBrowser.Filter) && this.Items.Count <= EXPAND_ALL_LIMIT)
            {
                this.ExpandAll();
            }
            base.OnFilterChanged(sender, e);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryTree();
        }
    }
}
