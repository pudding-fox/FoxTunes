using System.Collections;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class ListViewExtensions
    {
        private static readonly ConditionalWeakTable<ListView, SelectedItemsBehaviour> SelectedItemsBehaviours = new ConditionalWeakTable<ListView, SelectedItemsBehaviour>();

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(ListViewExtensions),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedItemsPropertyChanged))
        );

        public static IList GetSelectedItems(ListView source)
        {
            return (IList)source.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(ListView source, IList value)
        {
            source.SetValue(SelectedItemsProperty, value);
        }

        private static void OnSelectedItemsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null)
            {
                return;
            }
            var behaviour = default(SelectedItemsBehaviour);
            if (!SelectedItemsBehaviours.TryGetValue(listView, out behaviour))
            {
                SelectedItemsBehaviours.Add(listView, new SelectedItemsBehaviour(listView));
            }
            var items = (e.NewValue as IList);
            if (items == null || object.ReferenceEquals(items, listView.SelectedItems))
            {
                return;
            }
            listView.SelectedItems.Clear();
            foreach (var item in items)
            {
                listView.SelectedItems.Add(item);
            }
        }

        private class SelectedItemsBehaviour : UIBehaviour
        {
            public SelectedItemsBehaviour(ListView listView)
            {
                this.ListView = listView;
                this.ListView.SelectionChanged += this.OnSelectionChanged;
            }

            public ListView ListView { get; private set; }

            protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                SetSelectedItems(this.ListView, this.ListView.SelectedItems);
            }
        }
    }
}
