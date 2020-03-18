using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FoxTunes
{
    [Serializable]
    public class SelectionConfigurationElement : ConfigurationElement<SelectionConfigurationOption>
    {
        public SelectionConfigurationElement(string id, string name = null, string description = null, string path = null)
            : base(id, name, description, path)
        {
            this.Options = new ObservableCollection<SelectionConfigurationOption>();
        }

        public ObservableCollection<SelectionConfigurationOption> Options { get; set; }

        new public SelectionConfigurationOption DefaultValue
        {
            get
            {
                return base.DefaultValue as SelectionConfigurationOption;
            }
        }

        private bool Contains(string id)
        {
            return this.GetOption(id) != null;
        }

        private void Add(SelectionConfigurationOption option)
        {
            Logger.Write(this, LogLevel.Debug, "Adding configuration option: {0} => {1}", option.Id, option.Name);
            this.Options.Add(option);
        }

        private void Update(SelectionConfigurationOption option)
        {
            Logger.Write(this, LogLevel.Debug, "Updating configuration option: {0} => {1}", option.Id, option.Name);
            var existing = this.GetOption(option.Id);
            existing.Update(option);
        }

        private SelectionConfigurationOption GetOption(string optionId)
        {
            return this.Options.FirstOrDefault(option => string.Equals(option.Id, optionId, StringComparison.OrdinalIgnoreCase));
        }

        public SelectionConfigurationElement WithOptions(IEnumerable<SelectionConfigurationOption> options)
        {
            return this.WithOptions(options, false);
        }

        public SelectionConfigurationElement WithOptions(IEnumerable<SelectionConfigurationOption> options, bool clear)
        {
            var value = this.Value;
            if (clear)
            {
                this.Options.Clear();
            }
            foreach (var option in options)
            {
                this.Options.Add(option);
            }
            //If nothing is selected or the selection is no longer valid we try to select something.
            if (this.Value == null || !this.Contains(this.Value.Id))
            {
                if (value != null && this.Contains(value.Id))
                {
                    //The previous selection is valid, restore it.
                    this.Value = value;
                }
                else
                {
                    //Either no previous selection or it's invalid, select the first "default" option or just the first option if none are "default".
                    this.Value = this.Options.FirstOrDefault(option => option.IsDefault) ?? this.Options.FirstOrDefault();
                }
            }
            return this;
        }

        protected override void OnUpdate(ConfigurationElement element)
        {
            if (element is SelectionConfigurationElement)
            {
                this.OnUpdate(element as SelectionConfigurationElement);
            }
            base.OnUpdate(element);
        }

        protected virtual void OnUpdate(SelectionConfigurationElement element)
        {
            foreach (var option in element.Options.ToArray())
            {
                if (this.Contains(option.Id))
                {
                    this.Update(option);
                }
                else
                {
                    this.Add(option);
                }
                if (this.Value != null && string.Equals(this.Value.Id, option.Id, StringComparison.OrdinalIgnoreCase))
                {
                    this.Value.Update(option);
                }
            }
            if (this.DefaultValue != null)
            {
                var value = element.GetOption(this.DefaultValue.Id);
                if (value != null)
                {
                    this.DefaultValue.Update(value);
                }
            }
            if (this.Value == null)
            {
                if (element.Value != null)
                {
                    this.Value = element.Value;
                }
                else
                {
                    this.Value = this.Options.FirstOrDefault(option => option.IsDefault) ?? this.Options.FirstOrDefault();
                }
            }
        }
    }
}
