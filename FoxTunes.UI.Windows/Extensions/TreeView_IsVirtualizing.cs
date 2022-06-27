using FoxTunes.Interfaces;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class TreeViewExtensions
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private static readonly ConditionalWeakTable<TreeView, IsVirtualizingBehaviour> IsVirtualizingBehaviours = new ConditionalWeakTable<TreeView, IsVirtualizingBehaviour>();

        public static readonly DependencyProperty IsVirtualizingProperty = DependencyProperty.RegisterAttached(
            "IsVirtualizing",
            typeof(bool?),
            typeof(TreeViewExtensions),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIsVirtualizingPropertyChanged))
        );

        public static bool? GetIsVirtualizing(TreeView source)
        {
            return (bool?)source.GetValue(IsVirtualizingProperty);
        }

        public static void SetIsVirtualizing(TreeView source, bool? value)
        {
            source.SetValue(IsVirtualizingProperty, value);
        }

        private static void OnIsVirtualizingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView == null)
            {
                return;
            }
            var behaviour = default(IsVirtualizingBehaviour);
            if (!IsVirtualizingBehaviours.TryGetValue(treeView, out behaviour))
            {
                IsVirtualizingBehaviours.Add(treeView, new IsVirtualizingBehaviour(treeView));
            }
            else
            {
                Logger.Write(typeof(TreeViewExtensions), LogLevel.Warn, "Cannot modify virtualization settings.");
            }
        }

        private class IsVirtualizingBehaviour : UIBehaviour
        {
            public IsVirtualizingBehaviour(TreeView treeView)
            {
                this.TreeView = treeView;
                if (GetIsVirtualizing(this.TreeView).GetValueOrDefault())
                {
                    VirtualizingStackPanel.SetIsVirtualizing(this.TreeView, true);
                    VirtualizingStackPanel.SetVirtualizationMode(this.TreeView, VirtualizationMode.Recycling);
                }
                else
                {
                    VirtualizingStackPanel.SetIsVirtualizing(this.TreeView, false);
                    VirtualizingStackPanel.SetVirtualizationMode(this.TreeView, VirtualizationMode.Standard);
                }
            }

            public TreeView TreeView { get; private set; }
        }
    }
}
