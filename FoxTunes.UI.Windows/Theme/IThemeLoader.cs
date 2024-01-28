using FoxTunes.Interfaces;
using System.Windows;

namespace FoxTunes.Theme
{
    public interface IThemeLoader:IBaseComponent
    {
        void Apply(Application application);
    }
}
