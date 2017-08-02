using System;
using System.Collections.Generic;
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
            }
            return this;
        }

        public override void ConnectValue<T>(Action<T> action)
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
        }
    }
}
