using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class ComponentRegistry : IComponentRegistry
    {
        private ComponentRegistry()
        {
            this.Clear();
        }

        private ConcurrentBag<IBaseComponent> Components { get; set; }

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

        public T GetComponent<T>() where T : IBaseComponent
        {
            return this.GetComponents<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetComponents<T>() where T : IBaseComponent
        {
            foreach (var component in this.Components)
            {
                if (component is T)
                {
                    yield return (T)component;
                }
            }
        }

        public void ForEach(Action<IBaseComponent> action)
        {
            foreach (var component in this.Components)
            {
                action(component);
            }
        }

        public void ForEach<T>(Action<T> action) where T : IBaseComponent
        {
            foreach (var component in this.Components.OfType<T>())
            {
                action(component);
            }
        }

        public void Clear()
        {
            this.Components = new ConcurrentBag<IBaseComponent>();
        }

        public static readonly IComponentRegistry Instance = new ComponentRegistry();
    }
}
