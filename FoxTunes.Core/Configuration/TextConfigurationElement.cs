using System;

namespace FoxTunes
{
    [Serializable]
    public class TextConfigurationElement : ConfigurationElement
    {
        public TextConfigurationElement(string id, string name = null, string description = null, string path = null)
            : base(id, name, description, path)
        {
        }

        private string _Value { get; set; }

        public string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
                this.OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public TextConfigurationElement WithValue(string value)
        {
            this.Value = value;
            return this;
        }

        public override ConfigurationElement ConnectValue<T>(Action<T> action)
        {
            var payload = new Action(() => action((T)Convert.ChangeType(this.Value, typeof(T))));
            payload();
            this.ValueChanged += (sender, e) => payload();
            return this;
        }
    }
}
