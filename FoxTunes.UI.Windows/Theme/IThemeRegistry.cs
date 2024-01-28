using FoxTunes.Interfaces;
using System.Collections.ObjectModel;

namespace FoxTunes.Theme
{
    public interface IThemeRegistry : IStandardComponent
    {
        ObservableCollection<ITheme> Themes { get; }

        ITheme GetTheme(string id);
    }
}
