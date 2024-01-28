using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class ComponentLoader : IComponentLoader
    {
        private ComponentLoader()
        {

        }

        public IEnumerable<IBaseComponent> Load()
        {
            var components = new List<IBaseComponent>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(IStandardComponent)))
            {
                components.Add(ComponentActivator.Instance.Activate<IBaseComponent>(type));
            }
            return components;
        }

        public static readonly IComponentLoader Instance = new ComponentLoader();
    }
}
