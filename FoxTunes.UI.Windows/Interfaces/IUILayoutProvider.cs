namespace FoxTunes.Interfaces
{
    public interface IUILayoutProvider : IStandardComponent
    {
        string Id { get; }

        bool IsComponentActive(string id);

        UIComponentBase Load(UILayoutTemplate template);
    }

    public enum UILayoutTemplate : byte
    {
        None,
        Main
    }
}
