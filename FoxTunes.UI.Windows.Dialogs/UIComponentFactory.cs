using System;
using System.Linq;

namespace FoxTunes
{
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

        public UIComponentBase CreateControl(UIComponentConfiguration configuration)
        {
            var parent = this.GetComponentType(configuration.Component);
            var children = configuration.Children.Select(
                child => this.CreateControl(child)
            ).ToArray();
            return this.CreateControl(parent, children);
        }

        protected virtual UIComponentBase CreateControl(Type type, UIComponentBase[] children)
        {
            //TODO: Handle children.
            return ComponentActivator.Instance.Activate<UIComponentBase>(type);
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
