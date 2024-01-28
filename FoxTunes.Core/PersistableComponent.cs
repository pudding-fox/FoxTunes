using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class PersistableComponent : BaseComponent, IPersistableComponent
    {
        public int Id { get; set; }

        public virtual bool Equals(IPersistableComponent other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IPersistableComponent);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public static bool operator ==(PersistableComponent a, PersistableComponent b)
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

        public static bool operator !=(PersistableComponent a, PersistableComponent b)
        {
            return !(a == b);
        }
    }
}
