using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class MinimalLayoutProvider : UILayoutProviderBase, IConfigurableComponent
    {
        public override string Id
        {
            get
            {
                return MinimalLayoutProviderConfiguration.ID;
            }
        }

        public override bool IsComponentActive(string id)
        {
            return false;
        }

        public override UIComponentBase Load(UILayoutTemplate template)
        {
            switch (template)
            {
                case UILayoutTemplate.Main:
                    return new MinimalLayout();
            }
            throw new NotImplementedException();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return MinimalLayoutProviderConfiguration.GetConfigurationSections();
        }
    }
}
