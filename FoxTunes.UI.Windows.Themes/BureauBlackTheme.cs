using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    public class BureauBlackTheme : ThemeBase
    {
        public BureauBlackTheme()
            : base("AF4E116B-6319-400D-880E-B080EC3811A6", "BureauBlack")
        {

        }

        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Clear();
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/BureauBlack.xaml", UriKind.Relative)
                }
            );
        }
    }
}
