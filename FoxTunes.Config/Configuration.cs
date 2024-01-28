using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FoxTunes
{
    [Component("BA77B392-1900-4931-B720-16206B23DDA1", ComponentSlots.Configuration)]
    public class Configuration : StandardComponent, IConfiguration
    {
        public ObservableCollection<ConfigurationSection> Sections { get; private set; }

        public override void InitializeComponent(ICore core)
        {

            base.InitializeComponent(core);
        }

        public void RegisterSection(ConfigurationSection section)
        {
            if (this.IsRegistered(section.Id))
            {
                return;
            }
            this.Sections.Add(section);
        }

        private bool IsRegistered(string id)
        {
            return this.Sections.Any(section => string.Equals(section.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
