using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FoxTunes
{
    [Serializable]
    public class UIComponentConfiguration : BaseComponent, IEquatable<UIComponentConfiguration>, ISerializable
    {
        public UIComponentConfiguration()
        {
            this.Children = new ObservableCollection<UIComponentConfiguration>();
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

        #region ISerializable

        protected UIComponentConfiguration(SerializationInfo info, StreamingContext context)
        {
            this.Component = info.GetString(nameof(this.Component));
            this.Children = new ObservableCollection<UIComponentConfiguration>(
                (UIComponentConfiguration[])info.GetValue(nameof(this.Children), typeof(UIComponentConfiguration[]))
            );
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Component), this.Component);
            info.AddValue(nameof(this.Children), this.Children.ToArray());
        }

        #endregion
    }
}
