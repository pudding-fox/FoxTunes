using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    public class ShinyRedTheme : ThemeBase
    {
        public ShinyRedTheme()
            : base("031597D5-EC7C-4339-9854-CA3851FB3460", "ShinyRed")
        {

        }

        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Clear();
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/ShinyRed.xaml", UriKind.Relative)
                }
            );
        }
    }
}
