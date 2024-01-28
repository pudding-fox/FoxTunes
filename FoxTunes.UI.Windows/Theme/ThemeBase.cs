using System.Windows;

namespace FoxTunes.Theme
{
    public abstract class ThemeBase : BaseComponent, ITheme
    {
        public abstract void Apply(Application application);
    }
}
