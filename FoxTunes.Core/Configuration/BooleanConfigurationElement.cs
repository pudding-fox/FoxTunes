using System;

namespace FoxTunes
{
    [Serializable]
    public class BooleanConfigurationElement : ConfigurationElement
    {
        public BooleanConfigurationElement(string id, string name = null, string description = null, string path = null)
            : base(id, name, description, path)
        {
        }

        private bool _Value { get; set; }

        public bool Value
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
        public event EventHandler ValueChanged = delegate { };

        public BooleanConfigurationElement WithValue(bool value)
        {
            this.Value = value;
            return this;
        }

        public void Toggle()
        {
            this.Value = !this.Value;
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
