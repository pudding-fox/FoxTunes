using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoxTunes
{
    public partial class TabControlExtensions
    {
        private static readonly ConditionalWeakTable<TabControl, NonVirtualizedTabControlBehaviour> NonVirtualizedTabControlBehaviours = new ConditionalWeakTable<TabControl, NonVirtualizedTabControlBehaviour>();

        public static readonly DependencyProperty VirtualizationModeProperty = DependencyProperty.RegisterAttached(
            "VirtualizationMode",
            typeof(VirtualizationMode),
            typeof(TabControlExtensions),
            new UIPropertyMetadata(VirtualizationMode.Default, OnVirtualizationModeChanged)
        );

        private static void OnVirtualizationModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null)
            {
                return;
            }
            if (GetVirtualizationMode(tabControl) != VirtualizationMode.Disabled)
            {
                return;
            }
            var behaviour = default(NonVirtualizedTabControlBehaviour);
            if (!NonVirtualizedTabControlBehaviours.TryGetValue(tabControl, out behaviour))
            {
                behaviour = new NonVirtualizedTabControlBehaviour(tabControl);
                NonVirtualizedTabControlBehaviours.Add(tabControl, behaviour);
            }
        }

        public static VirtualizationMode GetVirtualizationMode(TabControl source)
        {
            return (VirtualizationMode)source.GetValue(VirtualizationModeProperty);
        }

        public static void SetVirtualizationMode(TabControl source, VirtualizationMode value)
        {
            source.SetValue(VirtualizationModeProperty, value);
        }

        public static readonly DependencyProperty TemplateProperty = DependencyProperty.RegisterAttached(
         "Template",
         typeof(DataTemplate),
         typeof(TabControlExtensions),
         new UIPropertyMetadata(null)
        );

        public static DataTemplate GetTemplate(TabControl source)
        {
            return (DataTemplate)source.GetValue(TemplateProperty);
        }

        public static void SetTemplate(TabControl source, DataTemplate value)
        {
            source.SetValue(TemplateProperty, value);
        }

        public class NonVirtualizedTabControlBehaviour : UIBehaviour
        {
            private NonVirtualizedTabControlBehaviour()
            {
                this.Content = new ConditionalWeakTable<DependencyObject, ContentControl>();
            }

            public NonVirtualizedTabControlBehaviour(TabControl tabControl) : this()
            {
                this.TabControl = tabControl;
                this.TabControl.Loaded += this.OnLoaded;
                this.TabControl.SelectionChanged += this.OnSelectionChanged;
            }

            public ConditionalWeakTable<DependencyObject, ContentControl> Content { get; private set; }

            public TabControl TabControl { get; private set; }

            protected virtual void OnLoaded(object sender, RoutedEventArgs e)
            {
                this.UpdateSelectedTab();
            }

            protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                this.UpdateSelectedTab();
            }

            public void UpdateSelectedTab()
            {
                var contentPresenter = this.TabControl.Template.FindName("PART_SelectedContentHost", this.TabControl) as ContentPresenter;
                if (contentPresenter != null)
                {
                    var content = this.GetCurrentContent();
                    if (!object.ReferenceEquals(contentPresenter.Content, content))
                    {
                        contentPresenter.Content = content;
                    }
                }
            }

            protected virtual ContentControl GetCurrentContent()
            {
                var item = this.TabControl.SelectedItem;
                if (item == null)
                {
                    return null;
                }

                var container = this.TabControl.ItemContainerGenerator.ContainerFromItem(item);
                if (container == null)
                {
                    return null;
                }

                var content = default(ContentControl);
                if (!this.Content.TryGetValue(container, out content))
                {
                    if (item is TabItem)
                    {
                        throw new InvalidOperationException("Expected data bound items.");
                    }
                    else
                    {
                        content = this.CreateContentFromDataContext(item);
                    }
                    this.Content.Add(container, content);
                }

                return content;
            }

            protected virtual ContentControl CreateContentFromDataContext(object dataContext)
            {
                var contentControl = new ContentControl()
                {
                    DataContext = dataContext,
                    ContentTemplate = GetTemplate(this.TabControl)
                };
                contentControl.SetBinding(ContentControl.ContentProperty, new Binding());
                return contentControl;
            }

            protected override void OnDisposing()
            {
                if (this.TabControl != null)
                {
                    this.TabControl.Loaded -= this.OnLoaded;
                    this.TabControl.SelectionChanged -= this.OnSelectionChanged;
                }
                base.OnDisposing();
            }
        }

        public enum VirtualizationMode : byte
        {
            Default,
            Disabled
        }
    }
}
