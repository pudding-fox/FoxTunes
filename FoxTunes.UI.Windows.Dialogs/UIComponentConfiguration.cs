using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FoxTunes
{
    public class UIComponentConfiguration : IEquatable<UIComponentConfiguration>
    {
        public UIComponentConfiguration()
        {
            this.Children = new ObservableCollection<UIComponentConfiguration>();
            this.MetaData = new ObservableCollection<MetaDataEntry>();
        }

        private string _Component { get; set; }

        public string Component
        {
            get
            {
                return this._Component;
            }
            set
            {
                this._Component = value;
                this.OnComponentChanged();
            }
        }

        protected virtual void OnComponentChanged()
        {
            if (this.ComponentChanged != null)
            {
                this.ComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Component");
        }

        public event EventHandler ComponentChanged;

        private ObservableCollection<UIComponentConfiguration> _Children { get; set; }

        public ObservableCollection<UIComponentConfiguration> Children
        {
            get
            {
                return this._Children;
            }
            set
            {
                this._Children = value;
                this.OnChildrenChanged();
            }
        }

        protected virtual void OnChildrenChanged()
        {
            if (this.ChildrenChanged != null)
            {
                this.ChildrenChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Children");
        }

        public event EventHandler ChildrenChanged;

        private ObservableCollection<MetaDataEntry> _MetaData { get; set; }

        public ObservableCollection<MetaDataEntry> MetaData
        {
            get
            {
                return this._MetaData;
            }
            set
            {
                this._MetaData = value;
                this.OnMetaDataChanged();
            }
        }

        protected virtual void OnMetaDataChanged()
        {
            if (this.MetaDataChanged != null)
            {
                this.MetaDataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MetaData");
        }

        public event EventHandler MetaDataChanged;

        public bool TryGet(string name, out string value)
        {
            var metaData = this.MetaData.FirstOrDefault(
                _metaData => string.Equals(_metaData.Name, name, StringComparison.OrdinalIgnoreCase)
            );
            if (metaData == null)
            {
                value = default(string);
                return false;
            }
            value = metaData.Value;
            return true;
        }

        public void AddOrUpdate(string name, string value)
        {
            var metaData = this.MetaData.FirstOrDefault(
                _metaData => string.Equals(_metaData.Name, name, StringComparison.OrdinalIgnoreCase)
            );
            if (metaData == null)
            {
                metaData = new MetaDataEntry()
                {
                    Name = name
                };
                this.MetaData.Add(metaData);
            }
            metaData.Value = value;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual bool Equals(UIComponentConfiguration other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!string.Equals(this.Component, other.Component, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!Enumerable.SequenceEqual(this.Children, other.Children))
            {
                return false;
            }
            if (!Enumerable.SequenceEqual(this.MetaData, other.MetaData))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as UIComponentConfiguration);
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                if (!string.IsNullOrEmpty(this.Component))
                {
                    hashCode += this.Component.ToLower().GetHashCode();
                }
                foreach (var child in this.Children)
                {
                    hashCode += child.GetHashCode();
                }
                foreach (var metaData in this.MetaData)
                {
                    hashCode += metaData.GetHashCode();
                }
            }
            return hashCode;
        }

        public static bool operator ==(UIComponentConfiguration a, UIComponentConfiguration b)
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

        public static bool operator !=(UIComponentConfiguration a, UIComponentConfiguration b)
        {
            return !(a == b);
        }

        public class MetaDataEntry : IEquatable<MetaDataEntry>
        {
            public string Name { get; set; }

            public string Value { get; set; }

            public virtual bool Equals(MetaDataEntry other)
            {
                if (other == null)
                {
                    return false;
                }
                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }
                if (!string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (!string.Equals(this.Value, other.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as MetaDataEntry);
            }

            public override int GetHashCode()
            {
                var hashCode = default(int);
                unchecked
                {
                    if (!string.IsNullOrEmpty(this.Name))
                    {
                        hashCode += this.Name.ToLower().GetHashCode();
                    }
                    if (!string.IsNullOrEmpty(this.Value))
                    {
                        hashCode += this.Value.ToLower().GetHashCode();
                    }
                }
                return hashCode;
            }

            public static bool operator ==(MetaDataEntry a, MetaDataEntry b)
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

            public static bool operator !=(MetaDataEntry a, MetaDataEntry b)
            {
                return !(a == b);
            }
        }
    }
}
