using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FoxTunes
{
    public class TabControl : Control
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(TabControl),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged))
        );

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null)
            {
                return;
            }
            tabControl.OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
        }

        public static IEnumerable GetItemsSource(TabControl source)
        {
            return (IEnumerable)source.GetValue(ItemsSourceProperty);
        }

        public static void SetItemsSource(TabControl source, IEnumerable value)
        {
            source.SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(TabControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedItemChanged))
        );

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null)
            {
                return;
            }
            tabControl.OnSelectedItemChanged();
        }

        public static object GetSelectedItem(TabControl source)
        {
            return source.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(TabControl source, object value)
        {
            source.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty TabNamePathProperty = DependencyProperty.Register(
           "TabNamePath",
           typeof(string),
           typeof(TabControl),
           new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTabNamePathChanged))
       );

        private static void OnTabNamePathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null)
            {
                return;
            }
            tabControl.OnTabNamePathChanged();
        }

        public static string GetTabNamePath(TabControl source)
        {
            return (string)source.GetValue(TabNamePathProperty);
        }

        public static void SetTabNamePath(TabControl source, string value)
        {
            source.SetValue(TabNamePathProperty, value);
        }

        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register(
           "ContentTemplate",
           typeof(DataTemplate),
           typeof(TabControl),
           new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContentTemplateChanged))
       );

        private static void OnContentTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null)
            {
                return;
            }
            tabControl.OnContentTemplateChanged();
        }

        public static DataTemplate GetContentTemplate(TabControl source)
        {
            return (DataTemplate)source.GetValue(ContentTemplateProperty);
        }

        public static void SetContentTemplate(TabControl source, DataTemplate value)
        {
            source.SetValue(ContentTemplateProperty, value);
        }

        public TabControl()
        {
            this._TabControl = new global::System.Windows.Controls.TabControl();
            TabControlExtensions.SetDragOverSelection(this._TabControl, true);
            this.Tabs = new ConditionalWeakTable<object, TabItem>();
            this.AddVisualChild(this._TabControl);
        }

        private global::System.Windows.Controls.TabControl _TabControl { get; set; }

        public ConditionalWeakTable<object, TabItem> Tabs { get; private set; }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this._TabControl;
        }

        public IEnumerable ItemsSource
        {
            get
            {
                return GetItemsSource(this);
            }
            set
            {
                SetItemsSource(this, value);
            }
        }

        protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue is INotifyCollectionChanged collectionChanged1)
            {
                collectionChanged1.CollectionChanged -= this.OnCollectionChanged;
            }
            if (newValue is INotifyCollectionChanged collectionChanged2)
            {
                collectionChanged2.CollectionChanged += this.OnCollectionChanged;
            }
            this.Reset(newValue);
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.Add(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.Remove(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    this.Reset(e.NewItems);
                    break;
            }
        }

        public object SelectedItem
        {
            get
            {
                return GetSelectedItem(this);
            }
            set
            {
                SetSelectedItem(this, value);
            }
        }

        protected virtual void OnSelectedItemChanged()
        {

        }

        public DataTemplate ContentTemplate
        {
            get
            {
                return GetContentTemplate(this);
            }
            set
            {
                SetContentTemplate(this, value);
            }
        }

        protected virtual void OnContentTemplateChanged()
        {

        }

        public string TabNamePath
        {
            get
            {
                return GetTabNamePath(this);
            }
            set
            {
                SetTabNamePath(this, value);
            }
        }

        protected virtual void OnTabNamePathChanged()
        {

        }

        protected virtual void Reset(IEnumerable items)
        {
            if (items == null)
            {
                items = this.ItemsSource;
            }
            this._TabControl.Items.Clear();
            if (items != null)
            {
                this.Add(items);
            }
        }

        protected virtual void Add(IEnumerable items)
        {
            foreach (var element in items)
            {
                this.Add(element);
            }
        }

        protected virtual void Add(object element)
        {
            this._TabControl.Items.Add(this.GetTabItem(element));
        }

        protected virtual void Remove(IEnumerable items)
        {
            foreach (var element in items)
            {
                this.Remove(element);
            }
        }

        protected virtual void Remove(object element)
        {
            this._TabControl.Items.Remove(this.GetTabItem(element));
        }

        protected virtual TabItem GetTabItem(object content)
        {
            var tabItem = default(TabItem);
            if (!this.Tabs.TryGetValue(content, out tabItem))
            {
                var contentControl = new ContentControl()
                {
                    ContentTemplate = this.ContentTemplate,
                    Content = content
                };
                tabItem = new TabItem()
                {
                    Content = contentControl,
                };
                tabItem.SetBinding(TabItem.HeaderProperty, new Binding(this.TabNamePath)
                {
                    Source = content
                });
                this.Tabs.Add(content, tabItem);
            }
            else
            {

            }
            return tabItem;
        }

        protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._TabControl.SelectedItem is TabItem tabItem && tabItem.Content is ContentControl contentControl)
            {
                if (!object.ReferenceEquals(this.SelectedItem, contentControl.Content))
                {
                    this.SelectedItem = contentControl.Content;
                }
            }
        }
    }
}