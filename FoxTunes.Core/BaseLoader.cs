using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FoxTunes
{
    public abstract class BaseLoader<T> : IBaseLoader<T> where T : IBaseComponent
    {
        public virtual IEnumerable<T> Load()
        {
            var components = new List<T>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(T)))
            {
                components.Add(ComponentActivator.Instance.Activate<T>(type));
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
                        return ComponentAttribute.PRIORITY_LOW;
                    }
                    return attribute.Priority;
                };
            }
        }
    }
}
