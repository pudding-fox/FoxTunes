using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    [Theme("3E9EFE8C-5245-4F8B-97D1-EB47CC70E373", "ExpressionDark")]
    public class ExpressionDarkTheme : ThemeBase
    {
        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/ExpressionDark.xaml", UriKind.Relative)
                }
            );
        }
    }
}
