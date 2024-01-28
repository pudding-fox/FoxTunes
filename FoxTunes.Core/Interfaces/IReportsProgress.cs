namespace FoxTunes.Interfaces
{
    public interface IReportsProgress
    {
        string Name { get; }

        event AsyncEventHandler NameChanged;

        string Description { get; }

        event AsyncEventHandler DescriptionChanged;

        int Position { get; }

        event AsyncEventHandler PositionChanged;

        int Count { get; }

        event AsyncEventHandler CountChanged;

        bool IsIndeterminate { get; }

        event AsyncEventHandler IsIndeterminateChanged;
    }
}
