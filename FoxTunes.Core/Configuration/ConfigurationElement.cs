using System;

namespace FoxTunes
{
    [Serializable]
    public abstract class ConfigurationElement : BaseComponent
    {
        protected ConfigurationElement(string id, string name = null, string description = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.IsHidden = false;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        private bool _IsHidden { get; set; }

        public bool IsHidden
        {
            get
            {
                return this._IsHidden;
            }
            set
            {
                this._IsHidden = value;
                this.OnIsHiddenChanged();
            }
        }

        protected virtual void OnIsHiddenChanged()
        {
            if (this.IsHiddenChanged != null)
            {
                this.IsHiddenChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsHidden");
        }

        [field: NonSerialized]
        public event EventHandler IsHiddenChanged = delegate { };

        public abstract ConfigurationElement ConnectValue<T>(Action<T> action);

        public void Update(ConfigurationElement element)
        {
            this.Name = element.Name;
            this.Description = element.Description;
            this.IsHidden = false;
            this.OnUpdate(element);
        }

        protected virtual void OnUpdate(ConfigurationElement element)
        {
            //Nothing to do.
        }

        public ConfigurationElement Hide()
        {
            this.IsHidden = true;
            return this;
        }

        public ConfigurationElement Show()
        {
            this.IsHidden = false;
            return this;
        }
    }
}
