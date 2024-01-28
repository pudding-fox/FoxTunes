using FoxTunes.Interfaces;
using System.Windows;

namespace FoxTunes.Theme
{
    public interface ITheme : IStandardComponent
    {
        string Id { get; }

        string Name { get; }

        string Description { get; }

        void Apply(Application application);
    }
}
