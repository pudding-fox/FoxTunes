using FoxTunes.Interfaces;

namespace FoxTunes
{
    public interface ITheme : IStandardComponent
    {
        string Id { get; }

        string Name { get; }

        string Description { get; }

        string ArtworkPlaceholder { get; }

        void Enable();

        void Disable();
    }
}
