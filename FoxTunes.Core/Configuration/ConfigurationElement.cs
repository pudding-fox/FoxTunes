using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    [Serializable]
    public abstract class ConfigurationElement : BaseComponent
    {
        private ConfigurationElement()
        {
            this.IsHidden = false;
            this.Flags = ConfigurationElementFlags.None;
        }

        protected ConfigurationElement(string id, string name = null, string description = null, string path = null)
            : this()
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Path = path;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string Path { get; private set; }

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
        public event EventHandler IsHiddenChanged;

        [field: NonSerialized]
        private ObservableCollection<ValidationRule> _ValidationRules;

        public ObservableCollection<ValidationRule> ValidationRules
        {
            get
            {
                return this._ValidationRules;
            }
            set
            {
                this._ValidationRules = value;
                this.OnValidationRulesChanged();
            }
        }

        protected virtual void OnValidationRulesChanged()
        {
            if (this.ValidationRulesChanged != null)
            {
                this.ValidationRulesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ValidationRules");
        }

        [field: NonSerialized]
        public event EventHandler ValidationRulesChanged;

        public ConfigurationElement WithValidationRule(ValidationRule validationRule)
        {
            if (this.ValidationRules == null)
            {
                this.ValidationRules = new ObservableCollection<ValidationRule>();
            }
            this.ValidationRules.Add(validationRule);
            return this;
        }

        private ConfigurationElementFlags _Flags { get; set; }

        public ConfigurationElementFlags Flags
        {
            get
            {
                return this._Flags;
            }
            set
            {
                this._Flags = value;
                this.OnFlagsChanged();
            }
        }

        protected virtual void OnFlagsChanged()
        {
            if (this.FlagsChanged != null)
            {
                this.FlagsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Flags");
        }

        [field: NonSerialized]
        public event EventHandler FlagsChanged;

        public ConfigurationElement WithFlags(ConfigurationElementFlags flags)
        {
            this.Flags = flags;
            return this;
        }

        public void Update(ConfigurationElement element)
        {
            this.Name = element.Name;
            this.Description = element.Description;
            this.Path = element.Path;
            this.IsHidden = element.IsHidden;
            this.ValidationRules = element.ValidationRules;
            this.Flags = element.Flags;
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

        public abstract void Reset();
    }

    [Serializable]
    public abstract class ConfigurationElement<T> : ConfigurationElement
    {
        protected ConfigurationElement(string id, string name = null, string description = null, string path = null) : base(id, name, description, path)
        {

        }

        public object DefaultValue { get; private set; }

        private T _Value { get; set; }

        public T Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                if (this.DefaultValue == null)
                {
                    this.DefaultValue = value;
                }
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

        public ConfigurationElement<T> WithValue(T value)
        {
            this.Value = value;
            return this;
        }

        public ConfigurationElement<T> ConnectValue(Action<T> action)
        {
            var handler = new EventHandler((sender, e) =>
            {
                try
                {
                    action(this.Value);
                }
                catch (Exception exception)
                {
                    this.OnError(exception);
                }
            });
            handler(this, EventArgs.Empty);
            this.ValueChanged += handler;
            return this;
        }

        public override void Reset()
        {
            this.Value = (T)Convert.ChangeType(this.DefaultValue, typeof(T));
        }
    }

    [Flags]
    public enum ConfigurationElementFlags : byte
    {
        None = 0,
        MultiLine = 1,
        FileName = 2,
        FolderName = 4
    }
}
