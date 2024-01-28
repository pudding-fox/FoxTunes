using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    [Theme("BF3790D0-033B-4526-8D06-1B3F66637EAF", "ExpressionLight")]
    public class ExpressionLightTheme : ThemeBase
    {
        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Clear();
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/ExpressionLight.xaml", UriKind.Relative)
                }
            );
        }
    }
}
