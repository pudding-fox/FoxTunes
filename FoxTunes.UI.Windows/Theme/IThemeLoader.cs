using FoxTunes.Interfaces;
using System.Windows;

namespace FoxTunes.Theme
{
    public interface IThemeLoader : IStandardComponent
    {
        Application Application { get; set; }
    }
}
