using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    public class WhistlerBlueTheme : ThemeBase
    {
        public WhistlerBlueTheme()
            : base("F9F59E4F-7ECA-46CE-AEA0-3852BCCEB82C", "WhistlerBlue")
        {

        }

        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Clear();
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/WhistlerBlue.xaml", UriKind.Relative)
                }
            );
        }
    }
}
