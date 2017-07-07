using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class PersistableComponent : BaseComponent, IPersistableComponent
    {
        public int Id { get; set; }
    }
}
