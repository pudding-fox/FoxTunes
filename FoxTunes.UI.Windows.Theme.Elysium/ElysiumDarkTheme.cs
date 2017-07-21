using FoxTunes.Theme;
using System.Windows;

namespace FoxTunes
{
    [Theme("F346942A-7733-45A3-9309-BC295E253A70", "Elysium")]
    public class ElysiumDarkTheme : ElysiumThemeBase
    {
        protected override void Configure(Application application)
        {
            global::Elysium.Manager.Apply(
                application,
                global::Elysium.Theme.Dark,
                global::Elysium.AccentBrushes.Blue,
                global::System.Windows.Media.Brushes.White
            );
        }
    }
}
