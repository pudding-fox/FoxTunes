using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class MetaDataItem : BaseComponent, IMetaDataItem
    {
        public virtual string Name { get; protected set; }

        public abstract object Value { get; }

        public abstract object[] Values { get; }
    }
}
