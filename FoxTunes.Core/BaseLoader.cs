using FoxTunes.Interfaces;
using System.Collections.Generic;

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
            return components;
        }
    }
}
