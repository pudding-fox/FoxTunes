using System;

namespace FoxTunes.Interfaces
{
    public interface IUILayoutProvider : IStandardComponent
    {
        string Id { get; }

        string Name { get; }

        string Description { get; }

        UIComponentBase Load(UILayoutTemplate template);

        event EventHandler Updated;
    }

    public enum UILayoutTemplate : byte
    {
        None,
        Main
    }
}
