using FoxTunes.Interfaces;
using System;

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
            var type = this.GetComponentType(configuration.Component);
            var component = ComponentActivator.Instance.Activate<UIComponentBase>(type);
            if (component is IUIComponentPanel panel)
            {
                panel.Component = configuration;
            }
            return component;
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
