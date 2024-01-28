using FoxTunes.Interfaces;
using System.Windows;

namespace FoxTunes.Theme
{
    public interface ITheme : IBaseComponent
    {
        void Apply(Application application);
    }
}
