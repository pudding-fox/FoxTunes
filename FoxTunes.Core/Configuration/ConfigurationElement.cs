using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FoxTunes
{
    [Serializable]
    public abstract class ConfigurationElement : BaseComponent, ISerializable
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

        public event EventHandler IsHiddenChanged;

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

        public event EventHandler FlagsChanged;

        public ConfigurationElement WithFlags(ConfigurationElementFlags flags)
        {
            this.Flags = flags;
            return this;
        }

        private IDictionary<string, ISet<string>> _Dependencies;

        public IDictionary<string, ISet<string>> Dependencies
        {
            get
            {
                return this._Dependencies;
            }
            private set
            {
                this._Dependencies = value;
            }
        }

        public void Update(ConfigurationElement element, bool create)
        {
            if (!this.GetType().IsAssignableFrom(element.GetType()))
            {
                //Element type was changed, cannot restore settings.
                return;
            }
            this.OnUpdate(element, create);
        }

        protected abstract void OnUpdate(ConfigurationElement element, bool create);

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

        public ConfigurationElement DependsOn(string sectionId, string elementId)
        {
            if (this.Dependencies == null)
            {
                this.Dependencies = new Dictionary<string, ISet<string>>(StringComparer.OrdinalIgnoreCase);
            }
            this.Dependencies.GetOrAdd(sectionId, _sectionId => new HashSet<string>()).Add(elementId);
            return this;
        }

        public abstract void Reset();

        #region ISerializable

        public abstract bool IsPersistent { get; }

        protected ConfigurationElement(SerializationInfo info, StreamingContext context)
        {
            this.Id = info.GetString(nameof(this.Id));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Id), this.Id);
        }

        #endregion
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
                    Logger.Write(this, LogLevel.Trace, "Setting default value for configuration element \"{0}\": {1}", this.Name, value);
                    this.DefaultValue = value;
                }
                if (object.Equals(this.Value, value))
                {
                    return;
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

        #region ISerializable

        protected ConfigurationElement(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Value = (T)info.GetValue(nameof(this.Value), typeof(T));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.Value), this.Value);
        }

        #endregion
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
