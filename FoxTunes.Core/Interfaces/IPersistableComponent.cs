using System;

namespace FoxTunes.Interfaces
{
    public interface IPersistableComponent : IBaseComponent
    {
        Guid Id { get; set; }
    }
}
