using System;

namespace FoxTunes
{
    [Serializable]
    public class BooleanConfigurationElement : ConfigurationElement
    {
        public BooleanConfigurationElement(string id, string name = null, string description = null)
            : base(id, name, description)
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
    }
}
