using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FoxTunes
{
    public abstract class BaseLoader<T> : BaseComponent, IBaseLoader<T> where T : IBaseComponent
    {
        public virtual IEnumerable<T> Load()
        {
            var components = new List<T>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(T)))
            {
                var component = default(T);
                try
                {
                    component = ComponentActivator.Instance.Activate<T>(type);
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to activate component: {0} => {1}: {2}", type.Name, typeof(T).Name, e.Message);
                    continue;
                }
                Logger.Write(this, LogLevel.Debug, "Activated component: {0} => {1}", type.Name, typeof(T).Name);
                components.Add(component);
            }
            return components.OrderBy(this.ComponentPriority);
        }

        public Func<T, byte> ComponentPriority
        {
            get
            {
                return component =>
                {
                    var attribute = component.GetType().GetCustomAttribute<ComponentAttribute>();
                    if (attribute == null)
                    {
                        return ComponentAttribute.PRIORITY_NORMAL;
                    }
                    return attribute.Priority;
                };
            }
        }
    }
}
