using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class DefaultLayoutProvider : UILayoutProviderBase, IConfigurableComponent
    {
        public override string Id
        {
            get
            {
                return DefaultLayoutProviderConfiguration.ID;
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
                    return new DefaultLayout();
            }
            throw new NotImplementedException();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return DefaultLayoutProviderConfiguration.GetConfigurationSections();
        }
    }
}
