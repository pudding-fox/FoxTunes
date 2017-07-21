using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    public class BureauBlueTheme : ThemeBase
    {
        public BureauBlueTheme()
            : base("631F59DF-175E-4D9A-812A-CD537C4304C1", "BureauBlue")
        {

        }

        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Clear();
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/BureauBlue.xaml", UriKind.Relative)
                }
            );
        }
    }
}
