using FoxTunes.Interfaces;
using System.IO;

namespace FoxTunes
{
    public interface ITheme : IStandardComponent
    {
        string Id { get; }

        string Name { get; }

        string Description { get; }

        Stream ArtworkPlaceholder { get; }

        void Enable();

        void Disable();
    }
}
