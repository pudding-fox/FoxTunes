using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace FoxTunes
{
    public partial class TabControlExtensions
    {
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
            tabControl.ContentTemplate = TemplateFactory.Template;
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

        public static readonly DependencyProperty InternalTabControlProperty = DependencyProperty.RegisterAttached(
            "InternalTabControl",
            typeof(TabControl),
            typeof(TabControlExtensions),
            new UIPropertyMetadata(null, OnInternalTabControlChanged)
        );

        private static void OnInternalTabControlChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var decorator = sender as Decorator;
            if (decorator == null)
            {
                return;
            }
            var tabControl = GetInternalTabControl(decorator);
            if (tabControl == null)
            {
                return;
            }
            var contentManager = ContentManager.GetContentManager(tabControl, decorator);
            contentManager.UpdateSelectedTab();
        }

        public static TabControl GetInternalTabControl(Decorator source)
        {
            return (TabControl)source.GetValue(InternalTabControlProperty);
        }

        public static void SetInternalTabControl(Decorator source, TabControl value)
        {
            source.SetValue(InternalTabControlProperty, value);
        }

        public static readonly DependencyProperty InternalCachedContentProperty = DependencyProperty.RegisterAttached(
            "InternalCachedContent",
            typeof(ContentControl),
            typeof(TabControlExtensions),
            new UIPropertyMetadata(null)
        );

        public static ContentControl GetInternalCachedContent(DependencyObject source)
        {
            return (ContentControl)source.GetValue(InternalCachedContentProperty);
        }

        public static void SetInternalCachedContent(DependencyObject source, ContentControl value)
        {
            source.SetValue(InternalCachedContentProperty, value);
        }

        public static readonly DependencyProperty InternalContentManagerProperty = DependencyProperty.RegisterAttached(
            "InternalContentManager",
            typeof(ContentManager),
            typeof(TabControlExtensions),
            new UIPropertyMetadata(null)
        );

        public static ContentManager GetInternalContentManager(TabControl source)
        {
            return (ContentManager)source.GetValue(InternalContentManagerProperty);
        }

        public static void SetInternalContentManager(TabControl source, ContentManager value)
        {
            source.SetValue(InternalContentManagerProperty, value);
        }

        public class ContentManager
        {
            public ContentManager(TabControl tabControl, Decorator decorator)
            {
                this.TabControl = tabControl;
                this.TabControl.SelectionChanged += this.OnSelectionChanged;
                this.Decorator = decorator;
            }

            public TabControl TabControl { get; private set; }

            public Decorator Decorator { get; private set; }

            public void ReplaceDecorator(Decorator decorator)
            {
                if (object.ReferenceEquals(this.Decorator, decorator))
                {
                    return;
                }
                this.Decorator.Child = null;
                this.Decorator = decorator;
            }

            protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                this.UpdateSelectedTab();
            }

            public void UpdateSelectedTab()
            {
                this.Decorator.Child = this.GetCurrentContent();
            }

            protected virtual ContentControl GetCurrentContent()
            {
                var item = this.TabControl.SelectedItem;
                if (item == null)
                {
                    return null;
                }

                var tabItem = this.TabControl.ItemContainerGenerator.ContainerFromItem(item);
                if (tabItem == null)
                {
                    return null;
                }

                var cachedContent = GetInternalCachedContent(tabItem);
                if (cachedContent == null)
                {
                    cachedContent = new ContentControl()
                    {
                        DataContext = item,
                        ContentTemplate = GetTemplate(this.TabControl),
                    };

                    cachedContent.SetBinding(ContentControl.ContentProperty, new Binding());
                    SetInternalCachedContent(tabItem, cachedContent);
                }

                return cachedContent;
            }

            public static ContentManager GetContentManager(TabControl tabControl, Decorator container)
            {
                var contentManager = GetInternalContentManager(tabControl);
                if (contentManager != null)
                {
                    contentManager.ReplaceDecorator(container);
                }
                else
                {
                    contentManager = new ContentManager(tabControl, container);
                    SetInternalContentManager(tabControl, contentManager);
                }

                return contentManager;
            }
        }

        private static class TemplateFactory
        {
            private static Lazy<DataTemplate> _Template = new Lazy<DataTemplate>(GetTemplate);

            public static DataTemplate Template
            {
                get
                {
                    return _Template.Value;
                }
            }

            private static DataTemplate GetTemplate()
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.TabContent)))
                {
                    var template = (DataTemplate)XamlReader.Load(stream);
                    template.Seal();
                    return template;
                }
            }
        }

        public enum VirtualizationMode : byte
        {
            Default,
            Disabled
        }
    }
}
