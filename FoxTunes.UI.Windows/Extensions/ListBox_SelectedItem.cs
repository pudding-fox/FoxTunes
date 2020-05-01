using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FoxTunes
{
    public static partial class ListBoxExtensions
    {
        private static readonly ConditionalWeakTable<ListBox, SelectedItemBehaviour> SelectedItemBehaviours = new ConditionalWeakTable<ListBox, SelectedItemBehaviour>();

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(bool),
            typeof(ListBoxExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnSelectedItemPropertyChanged))
        );

        public static bool GetSelectedItem(ListBox source)
        {
            return (bool)source.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(ListBox source, bool value)
        {
            source.SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            if (GetSelectedItem(listBox))
            {
                var behaviour = default(SelectedItemBehaviour);
                if (!SelectedItemBehaviours.TryGetValue(listBox, out behaviour))
                {
                    SelectedItemBehaviours.Add(listBox, new SelectedItemBehaviour(listBox));
                }
            }
            else
            {
                var behaviour = default(SelectedItemBehaviour);
                if (SelectedItemBehaviours.TryGetValue(listBox, out behaviour))
                {
                    SelectedItemBehaviours.Remove(listBox);
                    behaviour.Dispose();
                }
            }
        }

        private class SelectedItemBehaviour : UIBehaviour
        {
            private static readonly PropertyInfo ItemsHost = typeof(ItemsControl).GetProperty(
                "ItemsHost",
                BindingFlags.Instance | BindingFlags.NonPublic
             );

            private static readonly MethodInfo EnsureGenerator = typeof(Panel).GetMethod(
                "EnsureGenerator",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            private static readonly MethodInfo BringIndexIntoView = typeof(VirtualizingPanel).GetMethod(
                "BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            public SelectedItemBehaviour(ListBox listBox)
            {
                this.ListBox = listBox;
                BindingHelper.AddHandler(this.ListBox, Selector.SelectedItemProperty, typeof(ListBox), this.OnSelectedItemChanged);
            }

            public ListBox ListBox { get; private set; }

            public object SelectedItem
            {
                get
                {
                    return this.ListBox.SelectedItem;
                }
                set
                {
                    this.SelectItem(value);
                }
            }

            protected virtual void SelectItem(object value)
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
                        BringIndexIntoView.Invoke(panel, new object[] { index });
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
                    //Found the item, ensure it's visible and select it.
                    item.BringIntoView();
                    item.IsSelected = true;
                }
            }

            protected virtual void OnSelectedItemChanged(object sender, EventArgs e)
            {
                this.SelectItem(this.ListBox.SelectedItem);
            }
        }
    }
}
