using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public class SelectionConfigurationElement : ConfigurationElement
    {
        public SelectionConfigurationElement(string id, string name = null, string description = null)
            : base(id, name, description)
        {
            this.Options = new ObservableCollection<SelectionConfigurationOption>();
            this.SelectedOptions = new ObservableCollection<SelectionConfigurationOption>();
        }

        public ObservableCollection<SelectionConfigurationOption> Options { get; private set; }

        public ObservableCollection<SelectionConfigurationOption> SelectedOptions { get; private set; }

        public SelectionConfigurationElement WithOption(SelectionConfigurationOption option, bool selected = false)
        {
            this.Options.Add(option);
            if (selected)
            {
                this.SelectedOptions.Add(option);
            }
            return this;
        }
    }
}
