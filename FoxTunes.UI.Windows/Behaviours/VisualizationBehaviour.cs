using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    //PRIORITY_HIGH: Other components inspect our configuration on startup so make sure it's available.
    [Component("07E05BB1-EF74-43E2-9582-1EE609EBDD10", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class VisualizationBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return VisualizationBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
