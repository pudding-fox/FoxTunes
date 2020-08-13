using System;
using System.Collections.Concurrent;
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
            this.MetaData = new ConcurrentDictionary<string, string>();
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

        private ConcurrentDictionary<string, string> _MetaData { get; set; }

        public ConcurrentDictionary<string, string> MetaData
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
    }
}
