using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class LibraryBrowserExtensions
    {
        private static readonly ConditionalWeakTable<ListBox, ItemsPanelSelectorBehaviour> ItemsPanelSelectorBehaviours = new ConditionalWeakTable<ListBox, ItemsPanelSelectorBehaviour>();

        public static readonly DependencyProperty ItemsPanelSelectorProperty = DependencyProperty.RegisterAttached(
            "ItemsPanelSelector",
            typeof(LibraryBrowserViewMode),
#pragma warning disable CS0436
            typeof(LibraryBrowserExtensions),
#pragma warning restore CS0436
            new FrameworkPropertyMetadata(LibraryBrowserViewMode.None, new PropertyChangedCallback(OnItemsPanelSelectorPropertyChanged))
        );

        public static LibraryBrowserViewMode GetItemsPanelSelector(ListBox source)
        {
            return (LibraryBrowserViewMode)source.GetValue(ItemsPanelSelectorProperty);
        }

        public static void SetItemsPanelSelector(ListBox source, LibraryBrowserViewMode value)
        {
            source.SetValue(ItemsPanelSelectorProperty, value);
        }

        private static void OnItemsPanelSelectorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            if (GetItemsPanelSelector(listBox) != LibraryBrowserViewMode.None)
            {
                var behaviour = default(ItemsPanelSelectorBehaviour);
                if (!ItemsPanelSelectorBehaviours.TryGetValue(listBox, out behaviour))
                {
                    behaviour = new ItemsPanelSelectorBehaviour(listBox);
                    ItemsPanelSelectorBehaviours.Add(listBox, behaviour);
                }
                behaviour.Refresh();
            }
            else
            {
                var behaviour = default(ItemsPanelSelectorBehaviour);
                if (ItemsPanelSelectorBehaviours.TryGetValue(listBox, out behaviour))
                {
                    ItemsPanelSelectorBehaviours.Remove(listBox);
                    behaviour.Dispose();
                }
            }
        }

        public static readonly DependencyProperty GridItemsPanelProperty = DependencyProperty.RegisterAttached(
            "GridItemsPanel",
            typeof(ItemsPanelTemplate),
#pragma warning disable CS0436
            typeof(LibraryBrowserExtensions),
#pragma warning restore CS0436
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnGridItemsPanelPropertyChanged))
        );

        public static ItemsPanelTemplate GetGridItemsPanel(ListBox source)
        {
            return (ItemsPanelTemplate)source.GetValue(GridItemsPanelProperty);
        }

        public static void SetGridItemsPanel(ListBox source, ItemsPanelTemplate value)
        {
            source.SetValue(GridItemsPanelProperty, value);
        }

        private static void OnGridItemsPanelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //Nothing to do.
        }

        public static readonly DependencyProperty ListItemsPanelProperty = DependencyProperty.RegisterAttached(
            "ListItemsPanel",
            typeof(ItemsPanelTemplate),
#pragma warning disable CS0436
            typeof(LibraryBrowserExtensions),
#pragma warning restore CS0436
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnListItemsPanelPropertyChanged))
        );

        public static ItemsPanelTemplate GetListItemsPanel(ListBox source)
        {
            return (ItemsPanelTemplate)source.GetValue(ListItemsPanelProperty);
        }

        public static void SetListItemsPanel(ListBox source, ItemsPanelTemplate value)
        {
            source.SetValue(ListItemsPanelProperty, value);
        }

        private static void OnListItemsPanelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //Nothing to do.
        }

        public class ItemsPanelSelectorBehaviour : UIBehaviour
        {
            public ItemsPanelSelectorBehaviour(ListBox listBox)
            {
                this.ListBox = listBox;
            }

            public ListBox ListBox { get; private set; }

            public void Refresh()
            {
                switch (GetItemsPanelSelector(this.ListBox))
                {
                    case LibraryBrowserViewMode.Grid:
                        this.ListBox.ItemsPanel = GetGridItemsPanel(this.ListBox);
                        break;
                    case LibraryBrowserViewMode.List:
                        this.ListBox.ItemsPanel = GetListItemsPanel(this.ListBox);
                        break;
                    default:
                        this.ListBox.ItemsPanel = null;
                        break;
                }
            }
        }
    }
}
