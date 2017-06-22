using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BehaviourLoader : IBehaviourLoader
    {
        private BehaviourLoader()
        {

        }

        public IEnumerable<IBaseBehaviour> Load()
        {
            var behaviours = new List<IBaseBehaviour>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(IStandardBehaviour)))
            {
                behaviours.Add(ComponentActivator.Instance.Activate<IBaseBehaviour>(type));
            }
            return behaviours;
        }

        public static readonly IBehaviourLoader Instance = new BehaviourLoader();
    }
}
