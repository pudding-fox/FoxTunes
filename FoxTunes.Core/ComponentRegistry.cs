using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class ComponentRegistry : IComponentRegistry
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private ComponentRegistry()
        {
            this.Clear();
        }

        private List<IBaseComponent> Components { get; set; }

        public void AddComponents(params IBaseComponent[] components)
        {
            this.AddComponents(components.AsEnumerable());
        }

        public void AddComponents(IEnumerable<IBaseComponent> components)
        {
            foreach (var component in components)
            {
                this.Components.Add(component);
            }
        }

        public IBaseComponent GetComponent(Type type)
        {
            return this.GetComponents(type).FirstOrDefault();
        }

        public IBaseComponent GetComponent(string slot)
        {
            return this.GetComponents(slot).FirstOrDefault();
        }

        public IEnumerable<IBaseComponent> GetComponents(Type type)
        {
            foreach (var component in this.Components)
            {
                if (type.IsAssignableFrom(component.GetType()))
                {
                    yield return component;
                }
            }
        }

        public IEnumerable<IBaseComponent> GetComponents(string slot)
        {
            foreach (var component in this.Components)
            {
                var attribute = component.GetType().GetCustomAttribute<ComponentAttribute>();
                if (attribute == null)
                {
                    continue;
                }
                if (!string.Equals(attribute.Slot, slot, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                yield return component;
            }
        }

        public T GetComponent<T>() where T : IBaseComponent
        {
            return this.GetComponents<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetComponents<T>()
        {
            return this.Components.OfType<T>();
        }

        public void ReplaceComponents<T>(params T[] components) where T : IBaseComponent
        {
            this.ReplaceComponents(components.AsEnumerable());
        }

        public void ReplaceComponents<T>(IEnumerable<T> components) where T : IBaseComponent
        {
            this.ForEach<T>(component => this.RemoveComponent(component));
            this.AddComponents(components.OfType<IBaseComponent>());
        }

        private void RemoveComponent<T>(T component) where T : IBaseComponent
        {
            this.Components.Remove(component);
        }

        public void ForEach(Action<IBaseComponent> action)
        {
            foreach (var component in this.Components)
            {
                action(component);
            }
        }

        public void ForEach<T>(Action<T> action)
        {
            foreach (var component in this.GetComponents<T>())
            {
                action(component);
            }
        }

        public bool IsDefault(IBaseComponent component)
        {
            var type = component.GetType();
            {
                var attribute = type.GetCustomAttribute<ComponentAttribute>();
                if (attribute != null && attribute.Default)
                {
                    return true;
                }
            }
            {
                var attribute = type.GetCustomAttribute<ComponentReleaseAttribute>();
                if (attribute != null)
                {
                    var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
                    if (attribute.ReleaseType == releaseType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Clear()
        {
            this.Components = new List<IBaseComponent>();
        }

        public static readonly IComponentRegistry Instance = new ComponentRegistry();
    }
}
