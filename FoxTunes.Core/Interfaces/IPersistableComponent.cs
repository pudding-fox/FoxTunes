using System;

namespace FoxTunes.Interfaces
{
    public interface IPersistableComponent : IBaseComponent
    {
        int Id { get; set; }
    }
}
