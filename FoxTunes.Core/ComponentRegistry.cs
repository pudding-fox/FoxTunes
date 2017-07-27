using FoxTunes.Interfaces;
using System;
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
            foreach (var component in this.Components.ToArray())
            {
                action(component);
            }
        }

        public void ForEach<T>(Action<T> action) where T : IBaseComponent
        {
            foreach (var component in this.GetComponents<T>().ToArray())
            {
                action(component);
            }
        }

        public void Clear()
        {
            this.Components = new List<IBaseComponent>();
        }

        public static readonly IComponentRegistry Instance = new ComponentRegistry();
    }
}
