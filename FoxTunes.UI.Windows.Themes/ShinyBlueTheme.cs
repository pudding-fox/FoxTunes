using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    public class ShinyBlueTheme : ThemeBase
    {
        public ShinyBlueTheme()
            : base("D43E7251-92CD-4633-BD72-6C3A6961000C", "ShinyBlue")
        {

        }

        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Clear();
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/ShinyBlue.xaml", UriKind.Relative)
                }
            );
        }
    }
}
