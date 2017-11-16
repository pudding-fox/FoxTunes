using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        private SelectionConfigurationOption _SelectedOption { get; set; }

        public SelectionConfigurationOption SelectedOption
        {
            get
            {
                return this._SelectedOption;
            }
            set
            {
                this._SelectedOption = value;
                this.OnSelectedOptionChanged();
            }
        }

        protected virtual void OnSelectedOptionChanged()
        {
            if (this.SelectedOptionChanged != null)
            {
                this.SelectedOptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedOption");
        }

        [field: NonSerialized]
        public event EventHandler SelectedOptionChanged = delegate { };

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

        private void Remove(SelectionConfigurationOption option)
        {
            Logger.Write(this, LogLevel.Debug, "Removing configuration option: {0} => {1}", option.Id, option.Name);
            var existing = this.GetOption(option.Id);
            this.Options.Remove(existing);
        }

        public SelectionConfigurationOption GetOption(string optionId)
        {
            return this.Options.FirstOrDefault(option => string.Equals(option.Id, optionId, StringComparison.OrdinalIgnoreCase));
        }

        public SelectionConfigurationElement WithOption(SelectionConfigurationOption option, bool selected = false)
        {
            this.Options.Add(option);
            if (selected)
            {
                this.SelectedOption = option;
            }
            return this;
        }

        public SelectionConfigurationElement WithOptions(Func<IEnumerable<SelectionConfigurationOption>> options)
        {
            foreach (var option in options())
            {
                this.Options.Add(option);
                if (option.IsDefault && this.SelectedOption == null)
                {
                    this.SelectedOption = option;
                }
            }
            return this;
        }

        public override ConfigurationElement ConnectValue<T>(Action<T> action)
        {
            if (this.SelectedOption == null)
            {
                action(default(T));
            }
            else
            {
                action((T)Convert.ChangeType(this.SelectedOption.Id, typeof(T)));
            }
            this.SelectedOptionChanged += (sender, e) => this.ConnectValue(action);
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
                if (this.SelectedOption != null && string.Equals(this.SelectedOption.Id, option.Id, StringComparison.OrdinalIgnoreCase))
                {
                    this.SelectedOption.Update(option);
                }
            }
            foreach (var option in this.Options.ToArray())
            {
                if (!element.Contains(option.Id))
                {
                    this.Remove(option);
                    if (this.SelectedOption != null && string.Equals(this.SelectedOption.Id, option.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        this.SelectedOption = null;
                    }
                }
            }
            if (this.SelectedOption == null)
            {
                this.SelectedOption = this.Options.FirstOrDefault(option => option.IsDefault);
            }
        }
    }
}
