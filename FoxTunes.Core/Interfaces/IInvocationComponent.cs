using System;

namespace FoxTunes.Interfaces
{
    public interface IInvocationComponent : IBaseComponent
    {
        string Category { get; }

        string Id { get; }

        string Name { get; }

        string Description { get; }

        string Path { get; }

        byte Attributes { get; set; }

        event EventHandler AttributesChanged;
    }
}
