using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class ListBoxExtensions
    {
        private static readonly ConditionalWeakTable<ListBox, EnsureSelectedItemVisibleBehaviour> EnsureSelectedItemVisibleBehaviours = new ConditionalWeakTable<ListBox, EnsureSelectedItemVisibleBehaviour>();

        public static readonly DependencyProperty EnsureSelectedItemVisibleProperty = DependencyProperty.RegisterAttached(
            "EnsureSelectedItemVisible",
            typeof(bool),
            typeof(ListBoxExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnEnsureSelectedItemVisiblePropertyChanged))
        );

        public static bool GetEnsureSelectedItemVisible(ListBox source)
        {
            return (bool)source.GetValue(EnsureSelectedItemVisibleProperty);
        }

        public static void SetEnsureSelectedItemVisible(ListBox source, bool value)
        {
            source.SetValue(EnsureSelectedItemVisibleProperty, value);
        }

        private static void OnEnsureSelectedItemVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            if (GetEnsureSelectedItemVisible(listBox))
            {
                var behaviour = default(EnsureSelectedItemVisibleBehaviour);
                if (!EnsureSelectedItemVisibleBehaviours.TryGetValue(listBox, out behaviour))
                {
                    EnsureSelectedItemVisibleBehaviours.Add(listBox, new EnsureSelectedItemVisibleBehaviour(listBox));
                }
            }
            else
            {
                var behaviour = default(EnsureSelectedItemVisibleBehaviour);
                if (EnsureSelectedItemVisibleBehaviours.TryGetValue(listBox, out behaviour))
                {
                    EnsureSelectedItemVisibleBehaviours.Remove(listBox);
                    behaviour.Dispose();
                }
            }
        }

        private class EnsureSelectedItemVisibleBehaviour : UIBehaviour
        {
            private static readonly PropertyInfo ItemsHost = typeof(ItemsControl).GetProperty(
                "ItemsHost",
                BindingFlags.Instance | BindingFlags.NonPublic
             );

            private static readonly MethodInfo EnsureGenerator = typeof(Panel).GetMethod(
                "EnsureGenerator",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

#if NET40

            private static readonly MethodInfo BringIndexIntoView = typeof(VirtualizingPanel).GetMethod(
                "BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

#endif

            public EnsureSelectedItemVisibleBehaviour(ListBox listBox)
            {
                this.ListBox = listBox;
                this.ListBox.SelectionChanged += this.OnSelectionChanged;
            }

            public ListBox ListBox { get; private set; }

            protected virtual void EnsureVisible(object value)
            {
                //Try the easy method.
                var item = this.ListBox.ItemContainerGenerator.ContainerFromItem(value) as ListBoxItem;
                if (item == null)
                {
                    //Looks like the item hasn't been generated.
                    //Apply any templates we might need.
                    if (this.ListBox.Template != null)
                    {
                        this.ListBox.ApplyTemplate();
                        var presenter = this.ListBox.Template.FindName("ItemsHost", this.ListBox) as ItemsPresenter;
                        if (presenter != null)
                        {
                            presenter.ApplyTemplate();
                        }
                    }
                    //Update the layout and get the panel.
                    this.ListBox.UpdateLayout();
                    var panel = ItemsHost.GetValue(this.ListBox, null) as VirtualizingPanel;
                    if (panel != null)
                    {
                        //Enssure the ItemContainerGenerator is constructed.
                        EnsureGenerator.Invoke(panel, null);
                        //Get the index of the value.
                        var index = this.ListBox.Items.IndexOf(value);
                        if (index < 0)
                        {
                            //There is no item corresponding to the current value.
                            //Nothing can be done.
                            return;
                        }
                        //Tell the panel to being the index into view.
                        //This will create the item we're looking for.
#if NET40
                        BringIndexIntoView.Invoke(panel, new object[] { index });
#else
                        panel.BringIndexIntoViewPublic(index);
#endif
                    }
                    //Try the easy method (again).
                    item = this.ListBox.ItemContainerGenerator.ContainerFromItem(value) as ListBoxItem;
                    if (item == null)
                    {
                        //Looks like the item hasn't been generated.
                        //Nothing can be done.
                        return;
                    }
                }

                if (item != null)
                {
                    //Found the item, ensure it's visible.
                    item.BringIntoView();
                }
                else
                {

                }
            }

            protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                this.EnsureVisible(this.ListBox.SelectedItem);
            }
        }
    }
}
