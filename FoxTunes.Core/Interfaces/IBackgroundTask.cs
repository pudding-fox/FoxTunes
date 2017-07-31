using System;

namespace FoxTunes.Interfaces
{
    public interface IBackgroundTask
    {
        string Id { get; }

        bool Visible { get; }

        string Name { get; }

        event EventHandler NameChanged;

        string Description { get; }

        event EventHandler DescriptionChanged;

        int Position { get; }

        event EventHandler PositionChanged;

        int Count { get; }

        event EventHandler CountChanged;

        event EventHandler Started;

        event EventHandler Completed;

        Exception Exception { get; }

        event EventHandler ExceptionChanged;

        event EventHandler Faulted;
    }
}
