using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class UIComponentFactory : StandardFactory
    {
        public UIComponentConfiguration CreateConfiguration(UIComponent component)
        {
            return new UIComponentConfiguration()
            {
                Component = component.Id
            };
        }

        public UIComponent CreateComponent(UIComponentConfiguration configuration)
        {
            return LayoutManager.Instance.GetComponent(configuration.Component);
        }

        public FrameworkElement CreateControl(UIComponentConfiguration configuration, out UIComponentBase component)
        {
            var type = this.GetComponentType(configuration.Component);
            if (type == null || type == LayoutManager.PLACEHOLDER)
            {
                //A plugin was uninstalled.
                component = null;
                return null;
            }
            component = ComponentActivator.Instance.Activate<UIComponentBase>(type);
            if (component is IUIComponentPanel panel)
            {
                panel.Component = configuration;
            }
            //Some components expect to be hosted in a Grid (Artwork..)
            //We might as well add a Rectangle to make the entire thing hit testable.
            var grid = new Grid();
            grid.Children.Add(new Rectangle()
            {
                Fill = Brushes.Transparent
            });
            grid.Children.Add(component);
            return grid;
        }

        protected virtual Type GetComponentType(string id)
        {
            var component = LayoutManager.Instance.GetComponent(id);
            if (component == null)
            {
                return LayoutManager.PLACEHOLDER;
            }
            return component.Type;
        }
    }
}
