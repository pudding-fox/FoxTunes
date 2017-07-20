using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    [Serializable]
    public class SelectionConfigurationElement : ConfigurationElement
    {
        public SelectionConfigurationElement(string id, string name = null, string description = null)
            : base(id, name, description)
        {
            this.Options = new ObservableCollection<SelectionConfigurationOption>();
        }

        public ObservableCollection<SelectionConfigurationOption> Options { get; set; }

        public SelectionConfigurationOption SelectedOption { get; set; }

        public SelectionConfigurationElement WithOption(SelectionConfigurationOption option, bool selected = false)
        {
            this.Options.Add(option);
            if (selected)
            {
                this.SelectedOption = option;
            }
            return this;
        }
    }
}
