using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class LibraryBrowserExtensions
    {
        private static readonly ConditionalWeakTable<ListBox, ItemTemplateSelectorBehaviour> ItemTemplateSelectorBehaviours = new ConditionalWeakTable<ListBox, ItemTemplateSelectorBehaviour>();

        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.RegisterAttached(
            "ItemTemplateSelector",
            typeof(LibraryBrowserViewMode),
#pragma warning disable CS0436
            typeof(LibraryBrowserExtensions),
#pragma warning restore CS0436
            new FrameworkPropertyMetadata(LibraryBrowserViewMode.None, new PropertyChangedCallback(OnItemTemplateSelectorPropertyChanged))
        );

        public static LibraryBrowserViewMode GetItemTemplateSelector(ListBox source)
        {
            return (LibraryBrowserViewMode)source.GetValue(ItemTemplateSelectorProperty);
        }

        public static void SetItemTemplateSelector(ListBox source, LibraryBrowserViewMode value)
        {
            source.SetValue(ItemTemplateSelectorProperty, value);
        }

        private static void OnItemTemplateSelectorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            if (GetItemTemplateSelector(listBox) != LibraryBrowserViewMode.None)
            {
                var behaviour = default(ItemTemplateSelectorBehaviour);
                if (!ItemTemplateSelectorBehaviours.TryGetValue(listBox, out behaviour))
                {
                    behaviour = new ItemTemplateSelectorBehaviour(listBox);
                    ItemTemplateSelectorBehaviours.Add(listBox, behaviour);
                }
                behaviour.Refresh();
            }
            else
            {
                var behaviour = default(ItemTemplateSelectorBehaviour);
                if (ItemTemplateSelectorBehaviours.TryGetValue(listBox, out behaviour))
                {
                    ItemTemplateSelectorBehaviours.Remove(listBox);
                    behaviour.Dispose();
                }
            }
        }

        public static readonly DependencyProperty GridItemTemplateSelectorProperty = DependencyProperty.RegisterAttached(
            "GridItemTemplateSelector",
            typeof(DataTemplateSelector),
#pragma warning disable CS0436
            typeof(LibraryBrowserExtensions),
#pragma warning restore CS0436
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnGridItemTemplateSelectorPropertyChanged))
        );

        public static DataTemplateSelector GetGridItemTemplateSelector(ListBox source)
        {
            return (DataTemplateSelector)source.GetValue(GridItemTemplateSelectorProperty);
        }

        public static void SetGridItemTemplateSelector(ListBox source, DataTemplateSelector value)
        {
            source.SetValue(GridItemTemplateSelectorProperty, value);
        }

        private static void OnGridItemTemplateSelectorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //Nothing to do.
        }

        public static readonly DependencyProperty ListItemTemplateSelectorProperty = DependencyProperty.RegisterAttached(
            "ListItemTemplateSelector",
            typeof(DataTemplateSelector),
#pragma warning disable CS0436
            typeof(LibraryBrowserExtensions),
#pragma warning restore CS0436
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnListItemTemplateSelectorPropertyChanged))
        );

        public static DataTemplateSelector GetListItemTemplateSelector(ListBox source)
        {
            return (DataTemplateSelector)source.GetValue(ListItemTemplateSelectorProperty);
        }

        public static void SetListItemTemplateSelector(ListBox source, DataTemplateSelector value)
        {
            source.SetValue(ListItemTemplateSelectorProperty, value);
        }

        private static void OnListItemTemplateSelectorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //Nothing to do.
        }

        public class ItemTemplateSelectorBehaviour : UIBehaviour
        {
            public ItemTemplateSelectorBehaviour(ListBox listBox)
            {
                this.ListBox = listBox;
            }

            public ListBox ListBox { get; private set; }

            public void Refresh()
            {
                switch (GetItemTemplateSelector(this.ListBox))
                {
                    case LibraryBrowserViewMode.Grid:
                        this.ListBox.ItemTemplateSelector = GetGridItemTemplateSelector(this.ListBox);
                        break;
                    case LibraryBrowserViewMode.List:
                        this.ListBox.ItemTemplateSelector = GetListItemTemplateSelector(this.ListBox);
                        break;
                    default:
                        this.ListBox.ItemTemplateSelector = null;
                        break;
                }
            }
        }
    }
}
