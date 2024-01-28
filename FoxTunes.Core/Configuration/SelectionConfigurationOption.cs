using System;

namespace FoxTunes
{
    [Serializable]
    public class SelectionConfigurationOption : BaseComponent, IEquatable<SelectionConfigurationOption>
    {
        public SelectionConfigurationOption(string id, string name = null, string description = null)
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

        public bool IsDefault { get; private set; }

        public void Update(SelectionConfigurationOption option)
        {
            this.Name = option.Name;
            this.Description = option.Description;
            this.IsHidden = false;
        }

        public SelectionConfigurationOption Hide()
        {
            this.IsHidden = true;
            return this;
        }

        public SelectionConfigurationOption Show()
        {
            this.IsHidden = false;
            return this;
        }

        public SelectionConfigurationOption Default()
        {
            this.IsDefault = true;
            return this;
        }

        public bool Equals(SelectionConfigurationOption other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(this.Id, other.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SelectionConfigurationOption);
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (!string.IsNullOrEmpty(this.Id))
            {
                hashCode += this.Id.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(SelectionConfigurationOption a, SelectionConfigurationOption b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(SelectionConfigurationOption a, SelectionConfigurationOption b)
        {
            return !(a == b);
        }
    }
}
