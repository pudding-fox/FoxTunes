using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class ManagerLoader : IManagerLoader
    {
        private ManagerLoader()
        {

        }

        public IEnumerable<IBaseManager> Load()
        {
            var managers = new List<IStandardManager>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(IStandardManager)))
            {
                managers.Add(ComponentActivator.Instance.Activate<IStandardManager>(type));
            }
            return managers;
        }

        public static readonly IManagerLoader Instance = new ManagerLoader();
    }
}
